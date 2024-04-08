
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

/**
Hand Crafted constraints for our set of building templates
**/
public class BuildingSlotCitySizeConstraint : Constraint<int, BuildingTemplate> {
    private readonly int citySize;
    public BuildingSlotCitySizeConstraint(int size){this.citySize = size;}
    public override bool IsSatisfied(int variable, BuildingTemplate value, Dictionary<int, BuildingTemplate> assignment)
    {
        int sum = assignment.Values.Sum(b => b.size) + value.size;
        return sum <= citySize;
    }
}
public class BuildingSlotIndustryCapacityConstraint : Constraint<int, BuildingTemplate> {
    //Stores the maximum limits on industry building capacities
    //Basically for each tag how many jobs are there in the city, make sure our assignment doesnt exceed that
    private readonly Dictionary<string, int> industryMaximums;

    public BuildingSlotIndustryCapacityConstraint( Dictionary<string, int> industryMaximums){this.industryMaximums = industryMaximums;}
    public override bool IsSatisfied(int variable, BuildingTemplate value, Dictionary<int, BuildingTemplate> assignment)
    {
        //Check all the tags on our current building, by adding the capcities per tag of all currently assigned and if it doesnt exceed the cap for all of them, then return true
        foreach(string tag in value.tags){
            int sum = assignment.Values.ToArray().Where(b => b.tags.Contains(tag)).Sum(b => b.capacity) + value.capacity;
            if (industryMaximums.ContainsKey(tag) && sum >= industryMaximums[tag]) return false;
        }
        return true;
    }
}

public class BuildingSlotHousingFirstConstraint : Constraint<int, BuildingTemplate>{
    private readonly int population;
    public BuildingSlotHousingFirstConstraint( int population){this.population = population;}
    public override bool IsSatisfied(int variable, BuildingTemplate value, Dictionary<int, BuildingTemplate> assignment)
    {
        //Check all the tags on our current building, by adding the capcities per tag of all currently assigned and if it doesnt exceed the cap for all of them, then return true
        int sum = assignment.Values.ToArray().Where(b => b.tags.Contains("residential")).Sum(b => b.capacity);
        if ( sum > population || value.tags.Contains("residential")) return true;
        else  return false;
    }
}
/** 
somehow is broken and adds way too many house...
**/
public class BuildingSlotHousingOnlyFirstConstraint : Constraint<int, BuildingTemplate>{
    private readonly int population;
    public BuildingSlotHousingOnlyFirstConstraint( int population){this.population = population;}
    public override bool IsSatisfied(int variable, BuildingTemplate value, Dictionary<int, BuildingTemplate> assignment)
    {
        //Check all the tags on our current building, by adding the capcities per tag of all currently assigned and if it doesnt exceed the cap for all of them, then return true
        int sum = assignment.Values.ToArray().Where(b => b.tags.Contains("residential")).Sum(b => b.capacity);
        Debug.Log("Total population is: " + sum + " / " + population);
        if ( sum < population && value.tags.Contains("residential")) return true;
        else  if ( sum >= population && !value.tags.Contains("residential")) return true;
        else return false;
    }
}

