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
        int population = 5;

        // Create variables representing slots in the city
        List<int> variables = Enumerable.Range(0, citySize).ToList();

        // Create domains where each slot can be assigned any building template
        Dictionary<int, List<BuildingTemplate>> domains = new Dictionary<int, List<BuildingTemplate>>();
        foreach (int slot in variables)
        {
            domains[slot] = new List<BuildingTemplate>(buildingTemplates);
        }

        // Create constraints
        var constraints = new Dictionary<int, List<Constraint<int, BuildingTemplate>>>();
        foreach (int slot in variables)
        {
            constraints[slot] = new List<Constraint<int, BuildingTemplate>>
            {
                new BuildingSlotConstraint(buildingTemplates.Where(b => b.tags.Contains("residential")).ToList(), population)
            };
        }

        // Create CSP instance
        var csp = new CSP<int, BuildingTemplate>(variables, domains, constraints);

        // Solve the CSP
        var solution = csp.Solve();

        // Print the solution
        foreach (var kvp in solution)
        {
            Debug.Log($"Slot {kvp.Key}: Building {kvp.Value.name}, Tags: {string.Join(", ", kvp.Value.tags)}, Capacity: {kvp.Value.capacity}");
        }
    }
}