using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class Graph{
    public Variable[] variables;
    public Constraint[] constraints;
    public Func<Variable, int> variableSelector = leastConstrained;
    public static Func<Variable, int> MostConstrained(Variable[] assigned){
        return (Variable var) => {
            int occurences = 0;
            for(int i = 0; i < assigned.Length; i++){
                foreach(Constraint con in assigned[i].constraints){
                    if(con.variables.Contains(var)) occurences--;
                }
            }
            return occurences;
        };
    }
    public static int trivial(int x){return x;}
    public static int leastConstrained(Variable var){return -1 * var.constraints.Length;}
    public System.Random rand = new System.Random();

    public Graph(Variable[] vars, Constraint[] cons, ProceduralObject sys, bool randomDomainVals = true){
        variables = vars;
        constraints = cons;
        foreach(var c in constraints){c.obj = sys;}
        if(randomDomainVals){
            Func<int,int> randValue = (x) => rand.Next();
            foreach(Variable var in vars){var.domainSelector = randValue;}
        }
        //Debug.Log("Created layer:\n" + JsonConvert.SerializeObject(CastSolution(true)));
    }

    public void ResetPartialSolutions(){foreach(Variable variable in variables)variable.partialSolution = null;}
    /**
    Backtracking is DFS algorithm that checks each state if there are inconsistencies. If so, it backtracks early instead of going to max depth
    This is done by using some ordering to select a value for a variable then check it. if it is good recurse with the update assignment otherwise try a new value.
    if no values are valid then go return invalid so the frame above can try a new value

    note that when we use value and values we are actually using the int index of the value/values in the domain array
    **/
    public Dictionary<string, object> BacktrackingSolve(int errorThreshold = 1){
        Dictionary<Variable, int> assignments = new();
        int errors = BacktrackingHelper(assignments, errorThreshold, 0);
        if(errors > 0)Debug.Log("Warning Errors: " + errors);
        return CastSolution();
    }
    private int BacktrackingHelper(Dictionary<Variable, int>assignments, int errorThreshold = 1, int errors = 0){
        if(assignments.Keys.Count == variables.Length)return errors;
        //sort variables by number of constriants and start with most to least
        
        //Avg time for 6x6 sudoko is 1:10.8
        Variable variable = NextVariable(variables.Except(assignments.Keys), MostConstrained(assignments.Keys.ToArray()));
        //Avg time for 6x6x took longer than 10mins
        //Variable variable = NextVariable(variables.Except(assignments.Keys), variableSelector);
        
        //Get the ordered list of values
        int[] values = variable.partialSolution.ToArray();
        int maxError = errors;
        foreach(int value in values.OrderBy(variable.domainSelector)){
            int oldError = errors;
            variable.partialSolution = new(){value};//collapse the partial solution so that the constraint knows that it is assigned
            foreach(Constraint constraint in variable.constraints){
                if(constraint.VariablesAssigned()){
                    object[] vals = new object[constraint.variables.Length];
                    //Foreach variable in the constraint, get its value
                    for (int i = 0; i < constraint.variables.Length; i++) {
                        vals[i] = constraint.variables[i].GetValue();
                    }   
                    errors += constraint.Evaluate(vals);
                    //Since we check that all variables are assigned we dont need to check consistency, we can just evaluate the constraint
                    //errors += constraint.ArcConsistency(variable, value, errorThreshold);
                }
            }
            if (errors < errorThreshold){
                assignments.Add(variable, value);
                errors = BacktrackingHelper(assignments, errorThreshold, errors);
                //If the backtracking was a success then itll return the error < threshold so... check that all assignments are presenet and propogate error back up
                if(assignments.Keys.Count == variables.Length)return errors;
                //Otherwise remove the assignment 
                assignments.Remove(variable);
            }//reset the error before trying the next value
            maxError = Math.Max(maxError, errors);
            errors = oldError;
        }
        //Reset the partial solution to the original since we need to backtrack
        variable.partialSolution = values.ToList();
        //If we get to hear then none of the values are valid so we backtrack
        //If assignmnets.keys.count == 0 then we are at the base and it is overly constrained
        if(assignments.Keys.Count == 0){
            Debug.LogWarning("Backtracking was exhausted without a solution, problem is overly constrained");
            return BacktrackingHelper(assignments, errorThreshold * 2, 0);
        }
        return maxError;
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
        Queue<(Variable, Constraint)> queue = new();
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

    public int[][] AllSolutions(int errorThreshold = 1){
        Dictionary<Variable, int> assignments = new();
        List<int[]> allSolutions = new List<int[]>();

        int[] indicies = Enumerable.Repeat(0, variables.Length).ToArray();
        //indicies[i] represents the index of the partial solution array of index i that the value is in
        object[] values = new object[variables.Length];
        //Gets the value from the variables partial domain
        bool done = false;
        //While we havent checked every combination of indicies
        while (!done){
            //Assign the values object with the proper objects
            for (int i = 0; i < indicies.Length; i++) {
                values[i] = variables[i].GetValue(variables[i].partialSolution[indicies[i]]);
            }
            //Check if these values work
            int error = 0;
            foreach(Constraint con in constraints){error += con.Evaluate(values);}
            if (error < errorThreshold){
                //if they do add the indicies to the sollutions
                allSolutions.Add(indicies.ToArray());
            }
            //increments indicies[j] value by 1 and handles carrying over and skipping the headIndex
            int j = indicies.Length - 1;
            while (!done) {  //iterate until breaking by j<0 or index[j]++;
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
        return allSolutions.ToArray();
    }

    public void RandomWalkSolution(){
        /**
            Start with a random solution then try changing assignments until the number of errors goes down
        **/
    }

}