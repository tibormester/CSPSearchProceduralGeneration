using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TestCSP : MonoBehaviour{
    
    public TMP_InputField hinput;
    public TMP_InputField winput;
    public TMP_InputField rinput;
    public TMP_Text output_text;

    public int h = 1;
    public int w = 1;
    public int range = 10;
    public void OnSubmitH()
    {
        int x;
        if(int.TryParse(winput.text, out x)){
            if( 0 < x && x < 3){
                if(h != x){
                    h = x;
                    GenerateSudoku(h,w,range);
                }
                
            }
        }
    }
    public void OnSubmitW()
    {
        int x;
        if(int.TryParse(hinput.text, out x)){
            if( 0 < x && x < 3){
                if(w != x){
                    w = x;
                    GenerateSudoku(h,w,range);
                }
                
            }
        }
    }
    public void OnSubmitR()
    {
        int x;
        if(int.TryParse(rinput.text, out x)){
            if( 9 < x){
                if(range != x){
                    range = x;
                    GenerateSudoku(h,w,range);
                }
                
            }
        }
    }

    public void Start(){
         //prints the solution in a hopefully human readable format
        var solution = GenerateSudoku();
        Debug.Log(JsonConvert.SerializeObject(solution));
    }
    public Dictionary<string, object> GenerateSudoku( int h = 1, int w = 1,int range = 10){
        object[] d = (object[])Enumerable.Range(0,range).Select((x) => (object)x).ToArray();
        //When creating variables can assign whatever domain is necessary

        Variable[] vars = new Variable[9*w*h];
        for(int i = 0; i < (9*w*h); i++){
            vars[i] = new Variable((i / (3*w)) + ", " + (i % (3*w)), d);
        }
        List<Constraint> cons = new();
        //i,j coordinates for each i,j 3x3
        for (int i = 0; i < h; i++){
            for (int j = 0; j < w; j++){
                //Get all the variables in the 3x3 grid and make sure they arent repeated
                Variable[] variables = new Variable[9];
                for(int k = 0; k < 9; k++){
                    int distance = ((k / 3)*(3*w)) + (k % 3);
                    variables[k] = vars[(3*3*w)*i + 3*j + distance];
                }
                cons.Add(new Constraint(variables, Disjoint));
            }
        }
        //Gather sets of all rows and columns, and make sure they arent repeated
        //All the columns
        for (int i = 0; i < 3*w; i++){
            var vert = new Variable[3*h];
            for (int j = 0; j < 3*h; j++){
                vert[j] =  vars[3*w*j + i];
            }
            cons.Add(new Constraint(vert, Disjoint));
        }
        //All the rows
        for (int i = 0; i < 3*h; i++){
            var hori = new Variable[3*w];
            for (int j = 0; j < 3*w; j++){
                hori[j] = vars[3*w*i + j];
            }
            cons.Add(new Constraint(hori, Disjoint));
        }

        //Creates the object and runs it
        ProceduralObject suduko = new ProceduralObject();
        var graph = new Graph[]{new Graph(vars,cons.ToArray(), suduko)};
        suduko.layers = graph;

        int n = 1;
        var startT = DateTime.Now;
        Dictionary<string, object> solution = new();
        for(int i = 0; i < n; i++){
            suduko.layers[0].ResetPartialSolutions();
            solution = suduko.layers[0].BacktrackingSolve();
        }
        Debug.Log("Average time: " + (DateTime.Now - startT).Divide(n));
        string output = "";
        for(int i = 0; i < 3*w; i++){
            output += "\n";
            for(int j = 0; j < 3*h; j++){
                output += "  " + (int)(vars[3*w*j + i].GetValue());
            }
        }
        output_text.text = output;
        Debug.Log(output);
       return solution;
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






 /**
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
    **/