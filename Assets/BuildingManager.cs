using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine.AI;

/**
meant to be instanced and attached to the city game object, then it will have the cities transform etc to act locally
**/
public class BuildingManager : MonoBehaviour
{
    /**
        Loading saving simulation and visualization
        integrated with JSON, Unity.
        
        TODO:  build a UI to have people placement keep refreshing, maybe have it as a function weighted by time too
        Have a way to simulate changes to the city by having dynamic constraint changes (new weights most likely) working on both building generation and placement
        Have a way to mark People and Buildings as unique to make permanent their residency or location respectively.
    **/
    public string buildingsJSONPath = "Assets/Buildings.json";

    private Dictionary<string, BuildingTemplate> buildingTemplates = new Dictionary<string, BuildingTemplate>();
    private Dictionary<string, List<BuildingTemplate>> buildingTemplatesByTag = new Dictionary<string, List<BuildingTemplate>>();
    // Dictionary to map building names to BuildingObject instances
    private Dictionary<long, BuildingObject> buildingLookup = new Dictionary<long, BuildingObject>();
    private Dictionary<long, GameObject> renderedBuildings = new Dictionary<long, GameObject>();
    //Counts the number of instanced building objects in our lookup to provide unique IDs based on ordering
    private Dictionary<string, List<BuildingObject>> buildingsByTag = new Dictionary<string, List<BuildingObject>>();
    private long buildingCounter = 0;
    void Start()
    {
        LoadBuildingDictionary();
        //Do something like generate the set of buildings, sampling from the templates (Add buildings to lookup)
        //Now do something like distribute the buildings across the city respecting symettry (Assign positions)
        //Lastly draw all the buildings?
    }

    // Method to add a BuildingObject to the building lookup
    //This method is called on loading the city and on generation
    public void AddBuildingToLookup(BuildingObject buildingObject)
    {
        //If the object has no id (-1) then we increment the counter and assign a new one
        //This means that we can use this method even with saved city data
        long uniqueId = (buildingObject.id != -1L) ? buildingObject.id : buildingCounter++; 
        if (!buildingLookup.ContainsKey(uniqueId))
        {
            buildingLookup.Add(uniqueId, buildingObject);
            buildingObject.id = uniqueId;
        }
        else
        {
            Debug.LogWarning($"Building with id '{buildingObject.id}' already exists in the lookup.");
        }
        
        //Add the building to the proper tag list
        foreach(string tag in GetTemplate(buildingObject).tags){

        }
    }

    // Method to get a BuildingObject by ID from the building lookup
    public BuildingObject GetBuildingByID(long id)
    {
        if (buildingLookup.ContainsKey(id))
        {
            return buildingLookup[id];
        }
        else
        {
            Debug.LogWarning($"Building with id: '{id}' not found in the lookup.");
            return null;
        }
    }
    //Method to create the building template dictionary and lists
    void LoadBuildingDictionary()
    {
        string jsonString = File.ReadAllText(buildingsJSONPath);
        
        Debug.Log(jsonString);

        List<BuildingTemplate> buildingDictionary = JsonConvert.DeserializeObject<List<BuildingTemplate>>(jsonString);

        foreach (BuildingTemplate building in buildingDictionary)
        {
            buildingTemplates.Add(building.name, building);

            // Sort buildings by tags
            foreach (string tag in building.tags)
            {
                if (!buildingTemplatesByTag.ContainsKey(tag))
                {
                    buildingTemplatesByTag[tag] = new List<BuildingTemplate>();
                }
                buildingTemplatesByTag[tag].Add(building);
            }
        }

        // Example usage
        Debug.Log("Building dictionary loaded:");
        foreach (var kvp in buildingTemplates)
        {
            Debug.Log($"Building Name: {kvp.Key}, Tags: {string.Join(", ", kvp.Value.tags)}, Size: {kvp.Value.size}, prefab: {kvp.Value.prefab}");
        }
    }

    //Saves our list of building objects to our file
    public void SaveBuildingData(List<BuildingObject> buildingObjects, string outputPath)
    {
        List<string> buildingStrings = new List<string>();

        // Convert each BuildingObject to JSON string
        foreach (BuildingObject buildingObject in buildingObjects)
        {
            string buildingJson = JsonConvert.SerializeObject(buildingObject);
            buildingStrings.Add(buildingJson);
        }

        // Write JSON strings to file
        string jsonString = string.Join(",\n", buildingStrings.ToArray());
        jsonString = '[' + jsonString + ']';
        File.WriteAllText(outputPath, jsonString);

        Debug.Log($"Building data saved to {outputPath}");
    }
    public List<BuildingObject> LoadBuildingData(string inputPath)
    {
        List<BuildingObject> buildingObjects = new List<BuildingObject>();

        if (File.Exists(inputPath))
        {
            string[] buildingJsons = File.ReadAllLines(inputPath);

            foreach (string buildingJson in buildingJsons)
            {
                BuildingObject buildingObject = JsonUtility.FromJson<BuildingObject>(buildingJson);
                buildingObjects.Add(buildingObject);

                // Add the building to the lookup dictionary, this generates an error if it already exists
                AddBuildingToLookup(buildingObject);
            }

            Debug.Log($"Building data loaded from {inputPath}");
        }
        else
        {
            Debug.LogError($"Failed to load building data from {inputPath}. File does not exist.");
        }

        return buildingObjects;
    }

    public void CreateBuildingCubes(List<BuildingObject> buildingObjects)
    {
        foreach (BuildingObject buildingObject in buildingObjects)
        {
            Vector3 location = buildingObject.location;
            BuildingTemplate template = GetTemplate(buildingObject);
            int size = template.size;
            List<string> tags = template.tags;

            // Determine color based on tags
            Color color = Color.white; // Default color
            string prefab = GetTemplate(buildingObject).prefab;
            // Create cube
            GameObject buildingCube = (prefab == "") ? GameObject.CreatePrimitive(PrimitiveType.Cube) : Instantiate(Resources.Load<GameObject>(prefab));
            renderedBuildings.Add(buildingObject.id, buildingCube);
            //parent the cube to the city
            buildingCube.transform.parent = this.transform;
            buildingCube.transform.localPosition = location;
            buildingCube.transform.localScale = new Vector3(size, size, size);
            foreach (string tag in tags)
            {
                switch (tag)
                {
                    case "residential":
                        color = Color.white;
                        break;
                    case "commercial":
                        color = Color.yellow;
                        break;
                    case "industrial":
                        color = Color.blue;
                        break;
                    case "military":
                        color = Color.red;
                        break;
                    case "road":
                        color = Color.gray;
                        buildingCube.transform.localScale = new Vector3(size, size, 0.1f);
                        break;

                    // Add more cases for other tags as needed
                }
            }
            //set the color
            //buildingCube.GetComponent<Renderer>().material.color = color;
        }
    }

    public bool displayDictionary = false;
    public string dictionaryDisplayTag = "";
    public void OnDrawGizmos() {
        if (displayDictionary){
            displayDictionary = false;
            DisplayDictionary(dictionaryDisplayTag);
        }
    }
    public void DisplayDictionary(string tag = ""){
        //Gets the list of templates we want to use
        List<BuildingTemplate> templates = GetBuildingTemplatesByTag(tag);
        //Generates the list of buildings to be placed, spaced slightly apart
        float position = 0;
        float spacing = 1;
        List<BuildingObject> buildings =  new();
        foreach (BuildingTemplate template in templates){
            position += spacing + template.size * 0.5f;
            Vector3 location = new Vector3(position, template.size * 0.5f, 0f);
            BuildingObject building = new(template.name, location);
            building.id = (int)position;
            position += spacing + template.size * 0.5f;
            buildings.Add(building);
        }
        ClearRenderedBuildings();
        CreateBuildingCubes(buildings);
    }
    //Removes all rendered buildings
    public void ClearRenderedBuildings(){
        foreach ((long id, GameObject obj) in renderedBuildings){
            Destroy(obj);
        }
        renderedBuildings = new();
    }
    

    // Example function to get templates by tag
    public List<BuildingTemplate> GetBuildingTemplatesByTag(string tag)
    {
        if (buildingTemplatesByTag.ContainsKey(tag))
        {
            return buildingTemplatesByTag[tag];
        }
        else
        {
            Debug.LogWarning($"No buildings in dictionary found with tag: {tag}, returning the whole dictionary instead");
            return buildingTemplates.Values.ToList();
        }
    }
    /**
        Creates an instance of the template at the specified location and adds it to the building list
    **/
    public BuildingObject InstanceBuildingObject(BuildingTemplate template, Vector3 location){
        var building = new BuildingObject(template.name, location);
        AddBuildingToLookup(building);
        return building;
    }
    //Function to get all the buildings with a certain tag
    public List<BuildingObject> GetBuildingsByTag(string tag)
    {
        if (buildingsByTag.ContainsKey(tag))
        {
            return buildingsByTag[tag];
        }
        else
        {
            Debug.LogWarning($"No buildings in city found with tag: {tag}");
            return new List<BuildingObject>();
        }
    }

    public BuildingTemplate GetTemplate(BuildingObject obj){
        if(buildingTemplates.ContainsKey(obj.name)) return buildingTemplates[obj.name];
        else{
            Debug.LogWarning($"A building template doesn't exist for object '{obj.name}', providing a default template.");
            return new BuildingTemplate();
        }
    }
    public BuildingTemplate GetTemplate(string name){
        if (buildingTemplates.ContainsKey(name)) return buildingTemplates[name];
        else {
            Debug.LogWarning($"Failed to find a template with name '{name}', returning a default template instead.");
            return new BuildingTemplate();
        }
    }
    public BuildingTemplate GetTemplate(long id){
        if (buildingLookup.ContainsKey(id))return GetTemplate(buildingLookup[id]);
        else{
            Debug.LogWarning($"Failed to find a building with id '{id}', returning a default template.");
            return new BuildingTemplate();
        }      
    }
}

/**
A class representing a single prefab of a building stored in the building dictionary
**/
[Serializable]
public class BuildingTemplate
{
    //A unique identifier for this building template
    public string name;
    //A list of traits about the building
    public List<string> tags;
    //How big this building is
    public int size;
    //How many people reside, work, play in this building
    public int capacity;

    public string prefab;
}
/**
A class representing an initialized building that is either rendered or saved to JSON
**/
[Serializable]
public class BuildingObject
{
    public long id = -1;
    //Use to look up the building template from the dictionary to determine the tag, size, and capacity
    public string name = "Default";
    public Vector3 location = new Vector3();
    //A list of names (ID's to look up in our people dictionary) of the occupants of the building, they could be residents, workers, members etc... the info is stored in the people object
    public List<string> occupancy = new();
    //Similar to tag but what makes it unique from everthing of the same template
    public List<string> attributes = new();

    public BuildingObject(string n, Vector3 loc){
        name = n;
        location = loc;
    }
}
