using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CSP<TVariable, TDomain>
{
    //A list of our variables that we need to assign values from the domain to
    private readonly TVariable[] variables;
    /**
    private readonly Dictionary<TVariable, List<TDomain>> domains;
    this stores an individual domain per variable, but if the domains are all initially the same this is quite wasteful
    Instead we will use a single domain that is the union of all domains as well as an initial partial solution
    the partial solution will handle restricting domains of variables if necessary
    **/
    private readonly TDomain[] domains;
    /**
        saves the variable index associated with the set of domain indexes that are valid for the variable
        Note: if the variable index doesnt appear then the variable's domain is the entire domain
    **/
    private readonly Dictionary<int,List<int>> initialState;
    /**
        Each variables associated constriants, I might need to create a universal set similar to what i did to domains
        However a lot of these constraints although might be similar across variables might be unique per variable
        In otherwords it is likely to create these constraints depends on the specific variable
        as a result the universal set of constraints would be the same size as this dictionary, but just as an array
    **/

    private readonly Dictionary<int, List<Constraint<TVariable, TDomain>>> constraints;
    //After we finish solving the solution will be stored here in case other operations need to work on it
    private Dictionary<TVariable, TDomain> solution;

    public CSP(TVariable[] variables, TDomain[] domains, Dictionary<int, List<Constraint<TVariable, TDomain>>> constraints, Dictionary<int,List<int>> initialState)
    {
        this.variables = variables;
        this.domains = domains;
        this.constraints = constraints;
        this.initialState = initialState;
    }

    public Dictionary<TVariable, TDomain> Solve()
    {
        //solution = new Dictionary<TVariable, TDomain>();
        /**
            changed TAssignment to List<int>
            the list is intended to handle partial assignments by adding assignments/removing aspects of the domain
            the int is to help shrink the size of the object we are working with, creating a List of all domains is a polynomial growth
            This could likely be more efficient but hopefully is sufficient for this use case
            I could also make the variables ints and have them be looked up but I dont think that saves space since the variable data wont be duplicated exponentially
        **/
        Dictionary<int,List<int>> assignment = new(initialState);
        Backtrack(assignment);
        return solution;
    }
    private bool IsFullyAssigned(Dictionary<int,List<int>> assignment){
        if (assignment.Count() != variables.Count()) return false;
        foreach ((int varIndex, List<int> assignments) in assignment){
            if (assignments.Count() != 1) return false;
        }
        //If all variables have 1 and only 1 assignment then we are fully assigned
        return true;
    }
    
    //Converts from the dictionary based on indexes to the a dictionary based on the objects (so can be used externally)
    private Dictionary<TVariable, TDomain> AssignmentToSolution(Dictionary<int,List<int>> assignment, bool first = true){
        Dictionary<TVariable, TDomain> solution = new();
        foreach ((int varIndex, List<int> assignments) in assignment){
            if(assignments.Count == 1 || first) solution.Add(variables[varIndex], domains[assignments.First<int>()]);
        }
        //Create a dictionary where each variable indexes the first of its partial assignments or if first is false then only those with a single assignment
        return solution;
    }
    private void Backtrack(Dictionary<int, List<int>> assignment)
    {
        if (IsFullyAssigned(assignment))
        {
            solution = AssignmentToSolution(assignment);
            return;
        }
        //Breaks if there are no unassigned variables since it returns -1, but then it shouldnt have gotten past isfullyassigned
        int varIndex = SelectUnassignedVariable(assignment);
        if (varIndex == -1){
            solution = AssignmentToSolution(assignment);
            Debug.LogWarning("Couldn't find an unassigned variable despite thinking there should be one, likely IsFullyAssigned doesnt match SelectUnassignedVariable");
            return;
        }
        //Check each value in the partial domain, if it works shrink the domain to that and continue
        //Alternatively could shrink the domain by removing values that dont work
        int[] domainIndicies = (assignment.Keys.Contains(varIndex)) ? 
            assignment[varIndex].ToArray() : Enumerable.Range(0, domains.Length).ToArray(); 
        foreach (int domainIndex in domainIndicies)
        {
            if (IsConsistent(varIndex, domainIndex, assignment))
            {
                assignment[varIndex] = new List<int>{domainIndex};
                Backtrack(assignment);
                assignment.Remove(varIndex);
            }
        }
    }
    private int SelectUnassignedVariable(Dictionary<int,List<int>> assignment)
    {   //Maybe in the future I will have this find the one with the smallest domain etc...
        for (int i = 0; i < variables.Length; i++){
            if (assignment.ContainsKey(i)){
                if (assignment[i].Count != 1) return i;
            } else{
                return i;
            }
        }
        Debug.LogError("Can't find an unassigned variable anymore");
        return -1;
    }

    private bool IsConsistent(int varIndex, int domainIndex, Dictionary<int, List<int>> assignment)
    {
        foreach (Constraint<TVariable, TDomain> constraint in constraints[varIndex])
        {
            if (!constraint.IsSatisfied(variables[varIndex], domains[domainIndex], AssignmentToSolution(assignment, false)))
            {
                return false;
            }
        }
        return true;
    }
}
