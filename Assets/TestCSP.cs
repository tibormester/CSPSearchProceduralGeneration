using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class TestCSP : MonoBehaviour{
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
        Constraint con1 = new(vars, Disjoint.singleton.Value);
        //requires {var1,var2} to contain 2
        Constraint con2 = new( new Variable[2]{var1,var2}, new Contains(2));
        //requires all vars to contain 4
        Constraint con3 = new(vars, new Contains(4));

        //Create the array for constraints
        Constraint[] cons = new Constraint[3]{con2, con1, con3};


        //Creates the object and runs it
        CSPGraph[] layers = new CSPGraph[]{new CSPGraph(vars,cons)};
        ProceduralObject testObject = new ProceduralObject(vars, cons, layers);
        
        var solution = testObject.Solve(new int[]{0});

        //prints the solution in a hopefully human readable format
        Debug.Log(JsonConvert.SerializeObject(solution));

    }

    public sealed class Disjoint : Relation
    {
        public static readonly Lazy<Disjoint> singleton = new Lazy<Disjoint>();

        public int evaluate(object[] values)
        {
            int count = 0;
            foreach(object val in values){
                //I think this should tally up the number of times val is repeated in values
                count += values.Sum(obj => obj.Equals(val) ? 1 : 0);
            }
            return count - values.Length;
        }
        public int evaluate(object[] values, ProceduralObject obj){return evaluate(values);}
    }
    public sealed class Contains : Relation
    {
        private object value;
        public Contains(object equals){value = equals;}

        public int evaluate(object[] values)
        {
            if(values.Contains(value)) return 0;
            else return 1;
        }
        public int evaluate(object[] values, ProceduralObject obj){return evaluate(values);}
    }
}
