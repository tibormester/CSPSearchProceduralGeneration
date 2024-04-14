using System;
using System.Collections.Generic;
using System.Linq;
/**
Each variable is a property of an object and the generated value, there is also the domain of all values as well as partial solutions (list of indicies of objects in the domain)
**/
public class Variable{
    public string name;
    public object[] domain;
    //uninitialized means copy the indicies of the domain on request, otherwise just lists the indicies referencing domain objects
    protected List<int> PartialSolution;
    public List<int> partialSolution {
        get {   if (PartialSolution == null){
                    partialSolution = Enumerable.Range(0, domain.Length).ToList();
                }return PartialSolution;}
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
    public virtual object[] GetValues(){
            object[] vals = new object[partialSolution.Count];
            for(int i = 0; i < partialSolution.Count; i++){vals[i] = domain[partialSolution[i]];}
            return vals;   
    }
    public virtual object[] GetValues(int[] indicies){
            object[] vals = new object[indicies.Length];
            for(int i = 0; i < indicies.Length; i++){vals[i] = domain[indicies[i]];}
            return vals;  
    }
    public virtual object GetValue(int index){return domain[index];}
    public virtual object GetValue(){return domain[partialSolution[0]];}
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
    public int minvalues = 1;
    public int maxvalues = 1;
    private int[][] DomainBacking;
    private int[][] Domain {get  {if(DomainBacking == null)Domain = GenerateDomain(); return DomainBacking;} set=> DomainBacking = value;} 
    //uninitialized means copy the indicies of the domain on request, otherwise just lists the indicies referencing domain objects
    public new List<int> partialSolution {
        get  {
            if(PartialSolution == null){
                partialSolution = Enumerable.Range(0, Domain.Length).ToList();
            } return PartialSolution;
        }
        set => PartialSolution = value; 
        }
        

    public MultiVariable(string n, object[] d){
        name = n;
        domain = d;
        constraints = new Constraint[0];
    }

    public int[][] GenerateDomain(){
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
        return layer.AllSolutions();
    }
    public MultiVariable(){constraints = new Constraint[0];}

    public override object[] GetValues(){
            object[] vals = new object[partialSolution.Count];
            int[] indicies = new int[partialSolution.Count];
            for(int i = 0; i < partialSolution.Count; i++){
                indicies = Domain[partialSolution[i]];
                object[] val = new object[indicies.Length];
                for(int j = 0; j < indicies.Length; j++){
                    val[j] = (indicies[j] < domain.Length - 1) ? domain[indicies[j]] : null;
                }
                vals[i] = val;
            }
            return vals;   
    }
    public override object[] GetValues(int[] indicies){
            object[] vals = new object[indicies.Length];
            for(int i = 0; i < indicies.Length; i++){
                var indexer = Domain[indicies[i]];
                object[] val = new object[indexer.Length];
                for(int j = 0; j < indexer.Length; j++){
                    val[j] = (indexer[j] < domain.Length - 1) ? domain[indexer[j]] : null;
                }
                vals[i] = val;
            }
            return vals; 
    }
    public override object GetValue(int index){
        int[] indicies = Domain[index];
        object[] vals = new object[indicies.Length];
        for(int i = 0; i < indicies.Length; i++){
            vals[i] = (indicies[i] < domain.Length - 1) ? domain[indicies[i]] : null;
        }
        return vals;
        }
    public override object GetValue(){
        int[] indicies = Domain[partialSolution[0]];
        object[] vals = new object[indicies.Length];
        for(int i = 0; i < indicies.Length; i++){
            vals[i] = (indicies[i] < domain.Length - 1) ? domain[indicies[i]] : null;
        }
        return vals;
        }

}
