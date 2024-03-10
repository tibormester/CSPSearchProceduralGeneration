using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class CityManager : MonoBehaviour 
{
    public BuildingManager buildingManager;
    public void Start()
    {
        
        var csp = CreateCSP();
        // Solve the CSP
        var solution = csp.Solve();
        

        // Print the solution
        foreach (var kvp in solution)
        {
            Debug.Log($"Slot {kvp.Key}: Building {kvp.Value.name}, Tags: {string.Join(", ", kvp.Value.tags)}, Capacity: {kvp.Value.capacity}");
        }
        //Display it
        DisplaySolution(solution);
    }
    public CSP<int, BuildingTemplate> CreateCSP(){
        buildingManager = gameObject.GetComponent<BuildingManager>();
        // Sample building templates data
        List<BuildingTemplate> buildingTemplates = buildingManager.GetBuildingTemplatesByTag("");

        // City size and population
        int citySize = 100;
        int population = 50;

        // Create variables representing slots in the city
        int[] variables = Enumerable.Range(0, citySize).ToArray();
        //Create the default set of domain values from our list of building templates
        BuildingTemplate[] domain = buildingTemplates.ToArray();
        // Create an inital state mapping variable indicies to domain indicies
        Dictionary<int, List<int>> initialState = new();
        //For example we can loop through all the variables and create a subset of the global domain
        //By default not having a key indicates that we should just use the global domain
        for(int varIndex = 0; varIndex < variables.Length; varIndex++)
        {
            initialState[varIndex] = new List<int>(Enumerable.Range(0,domain.Length));
        }
        //Create universal constraints
        Dictionary<string, int> industryCapacities = new();
        //industryCapacities.Add("military", 15);
        //industryCapacities.Add("crafting", 15);
        Constraint<int, BuildingTemplate>[] universalConstraints = {new BuildingSlotCitySizeConstraint(citySize * 2), new BuildingSlotIndustryCapacityConstraint(industryCapacities), new BuildingSlotHousingFirstConstraint(population)};
        // Create unique constraints
        Dictionary<int, List<Constraint<int, BuildingTemplate>>> constraints = new();
        foreach (int slot in variables)
        {
            constraints[slot] = new List<Constraint<int, BuildingTemplate>>
            {
                //new BuildingSlotConstraint(variable)
            };
        }

        // Create CSP instance
        return new CSP<int, BuildingTemplate>(variables, domain, universalConstraints, constraints, initialState);
    }

    public bool newSolution = false;
    public void Update() {
        if (newSolution){
            newSolution = false;
            DisplaySolution(CreateCSP().Solve());
        }
    }
    public void DisplaySolution(Dictionary<int,BuildingTemplate> solution){
        //Gets the list of templates we want to use
        List<BuildingTemplate> templates = solution.Values.ToList();
        //Generates the list of buildings to be placed, spaced slightly apart
        float position = 0;
        float spacing = 5;
        float newLine = 100f;
        int line = 0;
        //Makes a list of our buildings so we can send the to the create function
        List<BuildingObject> buildings =  new();
        //For each temmplete create a building
        foreach (BuildingTemplate template in templates){
            position += spacing + template.size * 0.5f;
            Vector3 location = new Vector3(position, template.size * 0.5f, (float)line * spacing);
            BuildingObject building = new(template.name, location);
            building.id = (int)position + ((int)line * (int)newLine);
            position += spacing + template.size * 0.5f;
            buildings.Add(building);
            if (position > newLine){
                position = 0f;
                line++;
            }
        }
        buildingManager.ClearRenderedBuildings();
        buildingManager.CreateBuildingCubes(buildings);
    }
}