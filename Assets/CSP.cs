using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


/**
Building Generation
One csp with flexible constraints or something else for generating a list of buildings to place
Use city simulation properties like population (military, civilian, unemployed, elderly, youth), economy (exports-gathering, imports-manufacturing, oppulence), security (strength, defense, standing)
Then have a csp where the variables is simply a list of buildings the size of the city (note that maybe some buildings occupy more or less than a single variable), 
    selecting a state that satisfies rigid constraints like population while being weighted towards the flavours of population like military, civilian, elderly, youth etc...

Building Placement
Once we have a list of buildings to place, those become the new domains and we will let the variables become the location in the cities 3d space.
    Generating a proper layout will involve assigning random layouts and iterating through all buildings ensuring there is no overlapping
    Another constraint will be the connectivity of roads or something like that?
    Another constraint might be the zoning of the city so that military buildings are grouped etc... zoning might involve walls which will be like the dual to roads
    A lot of constraints could simply be weighted to pseudo enforce zoning instead, similar style buildings will attempt to be adjacent? (randomly generate n locations and try them in order of proximity to similar flavour building)

People Generation
Once we have a list of distinct buldings with associated locations, we can use the buildings as variables and assign them people as their domain and then assign the people a location around the building
    More specifically everyone needs a home, so use the housing as variables and assign them #people according to capacity. These people simply need names and appearances everything else can be partial assignments. 
    and then a building that isn't housing needs employees, these employees can be either taken from the closest partially assigned person, or generated and added to the neatest empty housing
    In this way people can be generated as needed (if part of the city is never visited, nothing is determined of the people there)
    We can use a marker for people if they need to be stored as a permanent feature of the buildings (if a player interacts with a person they become part of the building otherwise they are recyclable background)

People Placement
Although we could simply place people around there building or something simplistic like that, it makes sense to give them a task and place them somewhere along the execution of the task 
    this way when the rendered simulation takes over, it can continue guiding the people through their task.
    The domain of task will be sum of tasks from the housing (chores like cleaning, sleeping, chopping firewood etc...) and from their job (military barracks might have a patrolling job etc...)
    It is importatnt that all the tasks at least start at the building because the buildings will be populating at the edge of render distance
    so if the people are generated and get a task we want to ensure they can start their simulation at the edge of render distance too.
    This means that without buildings in proximity there wont be any people. So maybe there could be a 'wandering task' and a base number of population assigned to it and set to wander somewhere within render distance

Loading saving simulation and visualization
    integrate with JSON, Unity, and build a UI to have people placement keep refreshing, maybe have it as a function weighted by time too
    Have a way to simulate changes to the city by having dynamic constraint changes (new weights most likely) working on both building generation and placement
    Have a way to mark People and Buildings as unique to make permanent their residency or location respectively.

**/

/**
For large n, m this is taking an absurd amount of time, I think it has to do with allocating space for the arrays and since theyre holding classes not structs thats also an issue....
I think I could fix this by shrinking the objects to structs, using arrays instead of lists, and using a delta from the default instead....
**/

public class CSP<TVariable, TDomain>
{
    private readonly List<TVariable> variables;
    private readonly Dictionary<TVariable, List<TDomain>> domains;
    private readonly Dictionary<TVariable, List<Constraint<TVariable, TDomain>>> constraints;
    private Dictionary<TVariable, TDomain> solution;

    public CSP(List<TVariable> variables, Dictionary<TVariable, List<TDomain>> domains, Dictionary<TVariable, List<Constraint<TVariable, TDomain>>> constraints)
    {
        this.variables = variables;
        this.domains = domains;
        this.constraints = constraints;
    }

    public Dictionary<TVariable, TDomain> Solve()
    {
        //solution = new Dictionary<TVariable, TDomain>();
        var assignment = new Dictionary<TVariable, TDomain>();
        Backtrack(assignment);
        return solution;
    }

    private void Backtrack(Dictionary<TVariable, TDomain> assignment)
    {
        if (assignment.Count == variables.Count)
        {
            solution = new Dictionary<TVariable, TDomain>(assignment);
            return;
        }

        var var = SelectUnassignedVariable(assignment);
        foreach (var value in OrderDomainValues(var, assignment))
        {
            if (IsConsistent(var, value, assignment))
            {
                assignment[var] = value;
                Backtrack(assignment);
                assignment.Remove(var);
            }
        }
    }

    private TVariable SelectUnassignedVariable(Dictionary<TVariable, TDomain> assignment)
    {
        return variables.Except(assignment.Keys).First();
    }

    private IEnumerable<TDomain> OrderDomainValues(TVariable var, Dictionary<TVariable, TDomain> assignment)
    {
        return domains[var];
    }

    private bool IsConsistent(TVariable var, TDomain value, Dictionary<TVariable, TDomain> assignment)
    {
        foreach (var constraint in constraints[var])
        {
            if (!constraint.IsSatisfied(var, value, assignment))
            {
                return false;
            }
        }
        return true;
    }
}
