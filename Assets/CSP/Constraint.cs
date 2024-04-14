using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class Constraint{
    public Variable[] variables; 
    //Maybe make a way to sum together constraints if they act on the same set of variables???
    public Func<object[], ProceduralObject, int> relation;
    public ProceduralObject obj;

    /**
        when we initialize a constraint it is done with variable objects, 
        set these variable objects to have a refrence to the constraint
    **/
    public Constraint(Variable[] vars, Func<object[], ProceduralObject, int> rel){
        variables = vars;
        relation = rel;
        for(int i = 0; i < variables.Length; i++){
            Variable var = variables[i];
            if(!var.constraints.Contains(this)) var.constraints = var.constraints.Append(this).ToArray();
        }
    }
    
    public int Evaluate(object[] vals){
        return relation.Invoke(vals, obj);
    }
    /**
        Checks each value in the partial domain of head against all combinations of values in the partial domains of the other variables against the constraint
        prunes values that are inconsistent and returns the number of values pruned
        returns -1 if the partial solution becomes empty (this should indicate backtracking is necessary)
    **/
    public int ArcConsistencyPruning(Variable head, int errorThreshold = 1){
        if(head is MultiVariable) Debug.LogError("ArcConsistencyPruning isn't updated to accept Variables with Multiplicity...");
        //For each value in the partial solution of head, check if there is valid solutions, if not remove it from the partial solution
        if(!variables.Contains(head)) Debug.LogError("Head node: " + head.name + "isn't part of this constraint");
        int initialDomainSize = head.domainSize;
        if(initialDomainSize < 1) Debug.LogError("Attempting to Prune with Arc Consistency, but head node's domain is empty!");

        List<int> arcConsistentValues = new();
        foreach(int valueIndex in head.partialSolution){
            if(ArcConsistency(head, valueIndex) < errorThreshold){
                arcConsistentValues.Add(valueIndex);
            }
        }
        if(arcConsistentValues.Count == 0) {
            Debug.LogWarning("Found variable: " + head.name + " with no consistent values in its domain: " + JsonConvert.SerializeObject(head.GetValues()));
            return -1;
        }
        head.partialSolution = arcConsistentValues;
        int diff = initialDomainSize - head.domainSize;
        if(diff > 0)
            Debug.Log("Pruned: " + head.name + " removing "+ diff+" values from the domain: " + JsonConvert.SerializeObject(head.GetValues()));
        return diff;
        
    }
    public bool VariablesAssigned(){
        foreach(Variable var in variables){
            if (!var.assigned) return false;
        }
        return true;
    }
    /**
    Given a variable and the index from the domain array of the value check to see if it is arc consistent, error threshold has to do with the relation and is returned on failure
    **/
    public int ArcConsistency(Variable head, int valueIndex, int errorThreshold = 1){
        int[] indicies = Enumerable.Repeat(0, variables.Length).ToArray();
        //indicies[i] represents the index of the partial solution array of index i that the value is in
        object[] values = new object[variables.Length];
        //Gets the value from the variables partial domain
        int headIndex = Array.IndexOf(variables, head);
        //Stores the set of values in the head that are arc consistent
        values[headIndex] = head.GetValue(valueIndex);
        bool done = false;
        //While we havent checked every combination of indicies
        while (!done){
            //Assign the values object with the proper objects
            for (int i = 0; i < indicies.Length; i++) {
                if (i != headIndex){
                    values[i] = variables[i].GetValue(variables[i].partialSolution[indicies[i]]);
                }
            }
            int error = relation.Invoke(values, obj);
            if (error < errorThreshold){
                //Check if these values work, if they do, we are done and check the next value
                return error;
            }
            //increments indicies[j] value by 1 and handles carrying over and skipping the headIndex
            int j = indicies.Length - 1;
            while (!done) {  //iterate until breaking by j<0 or index[j]++;
                if (j == headIndex) j--;
                if (j < 0) { done = true; break; }
                if (indicies[j] + 1 >= variables[j].domainSize){
                    indicies[j] = 0;
                    j -= 1;
                }else{
                    indicies[j] += 1;
                    break;
                }
            }
        }
        return errorThreshold;
    }
}