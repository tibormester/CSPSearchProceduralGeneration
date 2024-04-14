using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

public class TestCSP : MonoBehaviour{
    
    /**
    public void Start(){
        //Can create a default domain
        object[] d1 = new object[8]{9,2,3,4,5,6,7,8};
        //When creating variables can assign whatever domain is necessary
        Variable var1 = new(){name = "var1", domain = d1};
        Variable var2 = new(){name = "var2", domain = d1};
        Variable var3 = new(){name = "var3", domain = d1};

        //Create the arrays to pass to test testobject
        Variable[] vars = new Variable[3]{var1, var2, var3};

        //When creating constraints assign the relation and the list of variables to check
        //Requires all vars to be disjoints
        Constraint con1 = new(vars, Disjoint);
        //requires {var1,var2} to contain 2
        Constraint con2 = new( new Variable[2]{var1,var2},  Contains(2));
        //requires all vars to contain 4
        Constraint con3 = new(vars, Contains(4));

        //Create the array for constraints
        Constraint[] cons = new Constraint[3]{con2, con1, con3};


        //Creates the object and runs it
        CSPGraph[] layers = new CSPGraph[]{new CSPGraph(vars,cons)};
        ProceduralObject testObject = new ProceduralObject(vars, cons, layers);
        
        var solution = testObject.Solve(new int[]{0});

        //prints the solution in a hopefully human readable format
        Debug.Log(JsonConvert.SerializeObject(solution));

    }
    **/
    

    public void Start(){
        var test = new EcosystemObject();
        var sol = test.layers[0].BacktrackingSolve(1);
        for(int i = sol.Count - 1; i >= 0; i--){
            KeyValuePair<string,object> kvp = sol.ElementAt(i);
            if (kvp.Value is string && (string)kvp.Value == "Neutralism"){
                sol.Remove(kvp.Key);
            }
        }
        Debug.Log(JsonConvert.SerializeObject(sol, Formatting.Indented));

        var tree = new TreeObject();
    }
    
    public Func<object[], ProceduralObject, int> Disjoint = (values,  obj) =>
        {
            int count = 0;
            foreach(object val in values){
                //I think this should tally up the number of times val is repeated in values
                count += values.Sum(obj => obj.Equals(val) ? 1 : 0);
            }
            return count - values.Length;
        };

    public Func<object[], ProceduralObject, int> Contains(object value) {
        Func<object[], ProceduralObject, int> f = (values, obj) => {
            return values.Contains(value) ? 0 : 1;
            };
        return f;
    }

}
