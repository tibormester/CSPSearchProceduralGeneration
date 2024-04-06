using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

public class ProceduralObject{
    public Variable[] variables;
    public Constraint[] constraints; 
    public CSPGraph[] layers;
    //Variable.name as a the string and a value from variable.domain as the object
    public Dictionary<string,object> solution; // to store all solved values in case solve is called on different layers at different times...?
    public ProceduralObject(Variable[] vars, Constraint[] cons, CSPGraph[] layers){
        variables = vars;
        constraints = cons;
        this.layers = layers;
        solution = new();
        foreach(var c in constraints){c.obj = this;}
    }
    /**
        if the list of layers is omitted default to creating a layer with all the vars and constraints
    **/
    public ProceduralObject(Variable[] vars, Constraint[] cons){
        variables = vars;
        constraints = cons;
        layers = new CSPGraph[1]{new CSPGraph(vars, cons)};
        foreach(var c in constraints){c.obj = this;}
    }
    public ProceduralObject(){}

    public Dictionary<string, object> Solve(int[] layersIndex, int errorThreshold = 1){
        Dictionary<string, object> values = new();
        foreach(int layerIndex in layersIndex){
            CSPGraph layer = this.layers[layerIndex];
            //shrinks the partial domains
            layer.ArcConsistencySolve(errorThreshold);
            //Solves the shrunk problem
            Dictionary<string, object> vals = layer.BacktrackingSolve();
            values.AddRange(vals);
            //solution.AddRange(vals);
        }
        return values;
    }
    public Dictionary<string, object> Solve(int layerIndex, int errorThreshold = 1){
        Dictionary<string, object> values = new();
            CSPGraph layer = this.layers[layerIndex];
            //shrinks the partial domains
            layer.ArcConsistencySolve(errorThreshold);
            //Solves the shrunk problem
            var vals = layer.BacktrackingSolve();
            values.AddRange(vals);
            solution.AddRange(vals);
        return values;
    }
    public Dictionary<string, object> Solve(int errorThreshold = 1){
        Dictionary<string, object> values = new();
        foreach(CSPGraph layer in layers){
            //shrinks the partial domains
            layer.ArcConsistencySolve(errorThreshold);
            //Solves the shrunk problem
            var vals = layer.BacktrackingSolve();
            values.AddRange(vals);
            solution.AddRange(vals);

        }
        return values;
    }
}

public class CSPGraph{
    public Variable[] variables;
    public Constraint[] constraints;

    public Func<int, int> orderSelector = trivial;
    public static int trivial(int x){return x;}

    public CSPGraph(Variable[] vars, Constraint[] cons){
        variables = vars;
        constraints = cons;
        Debug.Log("Created layer:\n" + JsonConvert.SerializeObject(CastSolution(true)));
    }

    public void ResetPartialSolutions(){foreach(Variable variable in variables)variable.partialSolution = null;}
    /**
    Backtracking is DFS algorithm that checks each state if there are inconsistencies. If so, it backtracks early instead of going to max depth
    This is done by using some ordering to select a value for a variable then check it. if it is good recurse with the update assignment otherwise try a new value.
    if no values are valid then go return invalid so the frame above can try a new value

    note that when we use value and values we are actually using the int index of the value/values in the domain array
    **/
    public Dictionary<string, object> BacktrackingSolve(){
        Dictionary<Variable, int> assignments = new();
        BacktrackingHelper(assignments, 1, 0);
        return CastSolution();
    }
    private int BacktrackingHelper(Dictionary<Variable, int>assignments, int errorThreshold = 1, int errors = 0){
        if(assignments.Keys.Count == variables.Length)return errors;
        //sort variables by number of constriants and start with most to least
        Variable variable = NextVariable(variables.Except(assignments.Keys), var => -1 * var.constraints.Length);
        //Get the ordered list of values
        int[] values = variable.NextValues(index => index);
        foreach(int value in values.OrderBy(orderSelector)){
            int oldError = errors;
            foreach(Constraint constraint in variable.constraints){
                errors += constraint.ArcConsistency(variable, value, errorThreshold);
            }
            if (errors < errorThreshold){
                assignments.Add(variable, value);
                variable.partialSolution = new(){value};//collapse the partial solution so our arc consistency checking incorporates this value
                errors = BacktrackingHelper(assignments, errorThreshold, errors);
                //If the backtracking was a success then itll return the error < threshold so... check that all assignments are presenet and propogate error back up
                if(assignments.Keys.Count == variables.Length)return errors;
                //Otherwise remove the assignment 
                assignments.Remove(variable);
                variable.partialSolution = null;
            }//reset the error before trying the next value
            errors = oldError;
        }
        //If we get to hear then non of the values are valid so we backtrack
        //If assignmnets.keys.count == 0 then we are at the base and it is overly constrained
        if(assignments.Keys.Count == 0) Debug.LogWarning("Backtracking was exhausted without a solution, problem is overly constrained");
        return errorThreshold;
    }

    /**
    A method to return our next variable to assign for backtracking, we can use algorithms like most constrained, might also want to store some values here....
    **/
    private Variable NextVariable(IEnumerable<Variable> vars, Func<Variable,int> selector){
        //given the list of vars to choose from choose
        return vars.OrderBy(selector).First();
    }
    
    /**
        Algorithm: Enqueue all arcs: (variable, constraint)
        
        we want our queue to be a set since if we are checking an arc theres no need to double check it

        pop an arc and prune for arc-consistency
        if the domain of the arc changed the enqueue all arcs leading to it:
            all arcs where variable_arc != variable_changed and variable_changed in constraint


        TODO:: Add a case handler for if theres 1 solution versus many possible solutions then split domains and check if theres a solution...
        **/
    public Dictionary<string, object> ArcConsistencySolve(int errorThreshold = 1, bool acceptErrors = true){
        //Enqueue all arcs initially
        Queue<(Variable,Constraint)> queue = new();
        foreach(Constraint con in constraints){
            foreach(Variable var in con.variables){
                queue.Enqueue((var, con));
            }
        }
        //While arcs remain in queue, pop the top and test
        while(queue.TryDequeue(out var arc)){
            (Variable var, Constraint con) = arc;
            //Prunes the partial solutions of var based on arc consistency on this constraint
            int pruned = con.ArcConsistencyPruning(var, errorThreshold);
            if(pruned == -1){
                Debug.LogError("Exhausted a domain during arc consistency solve, problem is over constrained");
                if(acceptErrors)return ArcConsistencySolve(errorThreshold * 2, acceptErrors);
            }
            if (pruned != 0){
                //If we update the domain then we need to enqueue any arcs leading to this variable
                foreach(Constraint c in var.constraints){
                    foreach(Variable v in c.variables){
                        if(v != var && !queue.Contains((v, c))) queue.Enqueue((v,c));
                    }
                }
            }
        }

        return CastSolution(true);
    }

    /**
        returns a dictionary with variable names and the first value in partial solution or the whole partial as an array
        goes from int[] -> object = domain[partialSolution[0]] if not multi
        otherwise it maps int[] -> object[] = domain[partialSolutions]
    **/
    public Dictionary<string,object> CastSolution(bool multiple = false){
        Dictionary<string, object> solution = new();
        foreach(Variable var in variables){
            if (!multiple)
                solution.Add(var.name, var.GetValue(var.partialSolution[0]));
            else{
                solution.Add(var.name, var.GetValues());
            }
        }
        return solution;
    }

}
/**
Each variable is a property of an object and the generated value, there is also the domain of all values as well as partial solutions (list of indicies of objects in the domain)
**/
public class Variable{
    public string name;
    public object[] domain;
    //uninitialized means copy the indicies of the domain on request, otherwise just lists the indicies referencing domain objects
    private List<int> PartialSolution;
    public List<int> partialSolution {
        get {   if (PartialSolution == null){
                    PartialSolution = Enumerable.Range(0, domain.Length).ToList();
                    return PartialSolution;
                }else {return PartialSolution;}}
        set  {PartialSolution = value;}}

    public int domainSize {get => partialSolution.Count;}

    //Initialize this to size zero with the constructor so we can append constraints as we define them
    public Constraint[] constraints;

    //idk this syntax public readonly object[] values {get => partialSolution.Where(index => domain[index]);}
    //public object value;
    public Variable(string n, object[] d){
        name = n;
        domain = d;
        constraints = new Constraint[0];
    }
    public Variable(){constraints = new Constraint[0];}

    //Returns the list of values from the partial solution, from the val index
    public  object[] GetValues(){
            object[] vals = new object[partialSolution.Count];
            for(int i = 0; i < partialSolution.Count; i++){vals[i] = domain[partialSolution[i]];}
            return vals;   
    }
    public  object[] GetValues(int[] indicies){
            object[] vals = new object[indicies.Length];
            for(int i = 0; i < indicies.Length; i++){vals[i] = domain[indicies[i]];}
            return vals;  
    }
    public  object GetValue(int index){return domain[index];}
    /**
    returns the partialsolution in the desired ordering
    A method for getting the next value using either a heursitic, randomness, or least constraining values
    send it a selector to use

    Maybe define some default selectors within this class for common heuristics as well as randomness and least constraining values
    **/
    public int[] NextValues(Func<int, int> selector){
        return partialSolution.OrderBy(selector).ToArray();
    }

}
public class Constraint{
    //public string[] variableNames;
    public Variable[] variables; 
    public Relation relation;
    public ProceduralObject obj;

    /**
        when we initialize a constraint it is done with variable objects, 
        set these variable objects to have a refrence to the constraint
    **/
    public Constraint(Variable[] vars, Relation rel){
        variables = vars;
        relation = rel;
        for(int i = 0; i < variables.Length; i++){
            Variable var = variables[i];
            if(!var.constraints.Contains(this)) var.constraints = var.constraints.Append(this).ToArray();
            //variableNames[i] = var.name;
        }
    }

    /**
        Checks each value in the partial domain of head against all combinations of values in the partial domains of the other variables against the constraint
        prunes values that are inconsistent and returns the number of values pruned
        returns -1 if the partial solution becomes empty (this should indicate backtracking is necessary)
    **/
    public int ArcConsistencyPruning(Variable head, int errorThreshold = 1){
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
            int error = relation.evaluate(values, obj);
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

/**
    Use a reference to the procedural object incase it needs access to data from other layers
    the int values 0 represents true, > 0 represents false with increasing magnitude being more false
    The int values < 0 represents a solution that enables future errors.... ?
**/
public interface Relation{
    public int evaluate(object[] values, ProceduralObject obj);
}