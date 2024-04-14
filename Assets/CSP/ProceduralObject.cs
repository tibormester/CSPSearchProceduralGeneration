using System.Collections.Generic;
using Unity.VisualScripting;

public class ProceduralObject{
    public Graph[] layers;
    //Variable.name as a the string and a value from variable.domain as the object
    public Dictionary<string,object> solution; // to store all solved values in case solve is called on different layers at different times...?
    public ProceduralObject(Graph[] layers){
        this.layers = layers;
        solution = new();
    }
    /**
        if the list of layers is omitted default to creating a layer with all the vars and constraints
    **/
    public ProceduralObject(Variable[] vars, Constraint[] cons){;
        layers = new Graph[1]{new Graph(vars, cons, this)};
        
    }
    public ProceduralObject(){}

    public Dictionary<string, object> Solve(int[] layersIndex, int errorThreshold = 1){
        Dictionary<string, object> values = new();
        foreach(int layerIndex in layersIndex){
            Graph layer = this.layers[layerIndex];
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
            Graph layer = this.layers[layerIndex];
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
        foreach(Graph layer in layers){
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


