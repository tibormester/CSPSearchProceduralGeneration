using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
    Define a base class for constraints
        Even categories of constraints perhaps or structural like and / or etc...
    then implement specific constraints ie. road adjacency constraint 
**/
public abstract class Constraint<TVariable, TDomain>
{
    public abstract bool IsSatisfied(TVariable variable, TDomain value, Dictionary<TVariable, TDomain> assignment);
}
public class BuildingSlotConstraint : Constraint<int, BuildingTemplate>
{
    private readonly List<BuildingTemplate> residentialTemplates;
    private readonly int population;

    public BuildingSlotConstraint(List<BuildingTemplate> residentialTemplates, int population)
    {
        this.residentialTemplates = residentialTemplates;
        this.population = population;
    }

    public override bool IsSatisfied(int slot, BuildingTemplate buildingTemplate, Dictionary<int, BuildingTemplate> assignment)
    {
        // Check if the building template is residential
        if (buildingTemplate.tags.Contains("residential"))
        {
            // Calculate the sum of capacities of residential buildings in the assignment
            int sum = assignment.Values.ToArray().Where(b => b.tags.Contains("residential")).Sum(b => b.capacity) + buildingTemplate.capacity;
            return sum <= population;
        }
        return true; // Constraint satisfied for non-residential buildings
    }
}