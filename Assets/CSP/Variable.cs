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
    public int minvalues = 1;
    public int maxvalues = 1;
    public bool ordered = false;
    //uninitialized means copy the indicies of the domain on request, otherwise just lists the indicies referencing domain objects
    private List<int> PartialSolution;
    public MultiVariable(string n, object[] d){
        name = n;
        domain = d;
        constraints = new Constraint[0];
    }
    public MultiVariable(){constraints = new Constraint[0];}


}