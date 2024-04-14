using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    public bool assigned {get => domainSize == 1 ? true : false;}
    public Func<int, int> domainSelector = (x) => x;

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
    public  object GetValue(){return domain[partialSolution[0]];}
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



public class MultiVariable : Variable{

    //Maybe define this as its own csp?? where we take any constraints that are only on this variable.... 
    //define variable primes that include the domain and zero, 
    //then get the set of all possible solutions and use that as the resulting domain and partial solution...
    public int minvalues = 1;
    public int maxvalues = 1;
    public int count = 0;
    public bool ordered = false;
    //uninitialized means copy the indicies of the domain on request, otherwise just lists the indicies referencing domain objects
    private List<int>[] PartialSolution;
    public List<int> partialSolution {
        //NEEDS to return the list of indicies that indicate all possible values to try...
        //The partial solutions are stored in a 2d array, this basically just flattens it or unflattens it...
        get {   if (PartialSolution == null){
                    PartialSolution = new List<int>[maxvalues];
                    for(int i = 0; i < PartialSolution.Length; i++){
                        PartialSolution[i] = Enumerable.Range(0, domain.Length).ToList();
                    }
                    return Enumerable.Range(0, domain.Length * PartialSolution.Length).ToList();
                }
                List<int> indicies = new List<int>();
                for(int i = 0; i < PartialSolution.Length; i++){
                    indicies.AddRange(PartialSolution[i].Select((x) => x + (i * domain.Length)));
                }
                return indicies;}
        set  {
                PartialSolution = new List<int>[maxvalues];
                for(int i = 0; i < PartialSolution.Length; i++){
                    PartialSolution[i] = new();
                }
                foreach(int val in value){
                    PartialSolution[val / domain.Length].Add(val % domain.Length);
                }
        }}

    public MultiVariable(string n, object[] d){
        name = n;
        domain = d;
        constraints = new Constraint[0];
    }

    public void GenerateDomain(){
        //Create the csp by appending null to the end of the domain
        object[] vals = domain.Append(null).ToArray();
        //Create the max number of variables
        var vars = new Variable[maxvalues];
        for(int i = 0; i < maxvalues; i++){
            vars[i] = new Variable(name + " " + i, vals);}
        //Take any constraint that was on the multi var and set it up to act on all the variables
        Constraint[] cons = constraints.Where((con) =>
            (con.variables.Length == 1) ? true : false).ToArray();
        //Ensure that the minimum number of variables has an assignment
        cons = cons.Append(new Constraint(vars, (vals, sys) => {
            int nonzero = 0;
            foreach(object val in vals){
                if(val != null)nonzero += 1;
            }
            return (minvalues - nonzero) <= 0 ? 0 : -1 * (minvalues - nonzero); 
            })).ToArray();
        //Change the existing constraints and variables so they can find each other
        foreach(Constraint con in cons){
            con.variables = vars;
        }foreach(Variable var in vars){
            var.constraints = cons;
        }
        ProceduralObject multiVar = new ProceduralObject();
        Graph layer = new Graph(vars, cons, multiVar, true);
        var sol = layer.BacktrackingSolve();
    }
    public MultiVariable(){constraints = new Constraint[0];}

    public object GetValue(int valueIndex){
        //return Array of values indexed by that value index...
        //value index = domain.length * ith variable + domain index
        //its going to be given the value index from 0 to maxvariables * domain length, checking each one...
        return null;
    }

}