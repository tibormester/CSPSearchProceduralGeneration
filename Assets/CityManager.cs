using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class CityManager : MonoBehaviour 
{
    public BuildingManager buildingManager;
    public void Start()
    {
        buildingManager = gameObject.GetComponent<BuildingManager>();
        // Sample building templates data
        List<BuildingTemplate> buildingTemplates = buildingManager.GetBuildingTemplatesByTag("");

        // City size and population
        int citySize = 5;
        int population = 500;

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

        // Create constraints
        var constraints = new Dictionary<int, List<Constraint<int, BuildingTemplate>>>();
        foreach (int slot in variables)
        {
            constraints[slot] = new List<Constraint<int, BuildingTemplate>>
            {
                new BuildingSlotConstraint(population)
            };
        }

        // Create CSP instance
        var csp = new CSP<int, BuildingTemplate>(variables, domain, constraints, initialState);

        // Solve the CSP
        var solution = csp.Solve();

        // Print the solution
        foreach (var kvp in solution)
        {
            Debug.Log($"Slot {kvp.Key}: Building {kvp.Value.name}, Tags: {string.Join(", ", kvp.Value.tags)}, Capacity: {kvp.Value.capacity}");
        }
    }
}