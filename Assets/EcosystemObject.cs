using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

public class EcosystemObject : ProceduralObject
{
    public static int MIN_SPECIES = 4;
    public static int MAX_SPECIES = 7;
    public static int MIN_TROPHIC_LEVELS = 2;
    public static int MAX_TROPHIC_LEVELS = 4;

    public int speciesCount;

    // our dictionaries that store stuff, instead of using objects just use strings for niches, relations, etc...
    public Dictionary<string, object> valueLookup; //keys will be from jobs or relationships array and values will be idk...
    
    //Our domains for each node and edge
    public object[] jobs = new string[4]{
        "Photosynthesizer",
        "Herbivore",
        "Carnivore",
        "Decomposer",
    };
    public object[] relationships = new string[4]{
        "Predation",
        "Mutualism",
        "Competition",
        "Neutralism",
    };
    public object[] trophicLevels;
    

    public Variable[] ecoVariables;
    public Constraint[] ecoConstraints;

    public Variable[] speciesVariables;
    public Constraint[] speciesConstraints;


    public EcoNode[] nodes;
    public class EcoNode{
        public string name;
        public Variable job;
        public Variable trophicLevel;
        public Variable[] edges;
        public EcosystemObject sys;
        public EcoNode(EcosystemObject sys, string name = ""){
            this.name = name;
            this.sys = sys;
            job = new Variable(name + " niche", sys.jobs);
            trophicLevel = new Variable(name + " trophic level", sys.trophicLevels);
            edges = new Variable[sys.speciesCount - 1];
        }
        /**
            Given an ecosystem object with a full set of nodes and a relationships domain, populates the node's edges array with new variables representing directed arcs to all other nodes
        **/
        public void PopulateEdges(){
            for(int i=0,j=0; i < edges.Length; i++){
                if(sys.nodes[j] == this)j++;
                EcoNode tail = sys.nodes[j];
                edges[i] = new Variable(name + " to  " + tail.name, sys.relationships);
                j++;
            }
        }
        public int EdgeIndex(EcoNode node){
            foreach(int edgeIndex in EdgeIndicies()){
                if(Tail(edgeIndex) == node) return edgeIndex;
            }
            Debug.Log("The given Node isn't in the set of edges");
            return -1;
        }
        public Variable Edge(EcoNode tail){
            foreach(int edgeIndex in EdgeIndicies()){
                if(Tail(edgeIndex) == tail){
                    int relativeIndex = edgeIndex - (JobIndex() + 2);
                    return edges[relativeIndex];
                }
            }
            Debug.LogWarning("Couldnt find an edge to the specified tail");
            return null;
        }

        /** Get the tail's job index from a given edge index **/
        public int TailIndex(int edgeIndex){
            int index = JobIndex();
            int relativeIndex = edgeIndex - (index + 2);
            //If this node appears before the tail, increment relative index
            if(relativeIndex >= index / (sys.speciesCount + 1)) relativeIndex++;
            return (sys.speciesCount + 1) * relativeIndex;
        }
        public EcoNode Tail(int edgeIndex){
            int index = JobIndex();
            int relativeIndex = edgeIndex - (index + 2);
            //If this node appears before the tail, increment relative index
            if(relativeIndex >= index / (sys.speciesCount + 1)) relativeIndex++;
            return sys.nodes[relativeIndex];
        }
        /** Gets the indicies of all the edges from the variables array **/
        public int[] EdgeIndicies(){
            int start = JobIndex();
            return Enumerable.Range(start + 2, sys.speciesCount - 1).ToArray();
        }
        /** Gets the starting index of this Node in the variables array **/
        public int JobIndex(){
            int nodeIndex = Array.IndexOf(sys.nodes, this);
            return nodeIndex * (sys.speciesCount + 1);
        }
        public int TrophicLevelIndex(){
            int nodeIndex = Array.IndexOf(sys.nodes, this);
            return (nodeIndex * (sys.speciesCount + 1)) + 1;
        }
    }
    public EcosystemObject(){
        //Determine Meta Attributes (basically the size of the ecosystem)
        speciesCount = UnityEngine.Random.Range(MIN_SPECIES,MAX_SPECIES);
        trophicLevels  = Enumerable.Range(0, UnityEngine.Random.Range(MIN_TROPHIC_LEVELS,MAX_TROPHIC_LEVELS)).Select(i => (object)i).ToArray();

        //Create all the nodes and edges of our graph
        nodes = new EcoNode[speciesCount];
        for (int i = 0; i < nodes.Length; i++){
            nodes[i] = new EcoNode(this, "species " + i);
        }
        foreach(EcoNode node in nodes)node.PopulateEdges();
        //Flatten the nodes into a list of variables and construct the set of constraints
        ecoVariables = GetVariables();
        ecoConstraints = GetConstraints();
        foreach(Constraint cons in ecoConstraints){cons.obj = this;};

        layers = new CSPGraph[]{new CSPGraph(ecoVariables, ecoConstraints)};
        
        //this.Solve(new int[]{0});//Tells this object to try and solve our first layer, uses arc consistency first which is slow
        //Debug.Log(JsonConvert.SerializeObject(solution));

        var rand = new System.Random();
        layers[0].domainSelector = (x) => rand.Next();

        //need to order evaluation of variables so it does niches first everything else after
        layers[0].variableSelector= (var) => (var.name.Contains("niche")) ? 0 : 1;

        var sol = layers[0].BacktrackingSolve();
        Debug.Log(JsonConvert.SerializeObject(sol));
    }
    /**
        Iterate through all nodes, adding together their job, trophic level, and all their edge values
    **/
    public Variable[] GetVariables(){
        List<Variable> vars = new();
        for (int i = 0; i < nodes.Length; i++){
            EcoNode node = nodes[i];
            vars.Add(node.job);
            vars.Add(node.trophicLevel);
            vars.AddRange(node.edges);
        }
        return vars.ToArray();
    }
    public EcoNode GetEcoNode(int variableIndex){
        // the node is the index divided by (edges = # species - 1) + (2 = job + trophic level)
        return nodes[variableIndex / (speciesCount + 1)];
    }
    public List<Constraint> EachNode(Func<object[], EcoNode, EcosystemObject, int> f){
        List<Constraint> cons = new();
        foreach(EcoNode node in nodes){
            Variable[] vars = new Variable[] {node.job, node.trophicLevel }.Concat(node.edges).ToArray();
            Func<object[], ProceduralObject, int> func = (vals, obj) => f.Invoke(vals, node, (EcosystemObject)obj);
            Constraint c = new Constraint(vars, func);
            cons.Add(c);
        }
        return cons;
    }
    public List<Constraint> EachEdge(Func<object, object, object, object, object, object, EcosystemObject, int> f){
        List<Constraint> cons = new();
        foreach(EcoNode node in nodes){
            foreach(int edgeIndex in node.EdgeIndicies()){
                var tail = node.Tail(edgeIndex);
                Variable[] vars = new Variable[] {node.job, node.trophicLevel, node.Edge(tail), tail.Edge(node), tail.job, tail.trophicLevel };
                Func<object[], ProceduralObject, int> func = (vals, obj) => f.Invoke(vals[0], vals[1], vals[2], vals[3], vals[4], vals[5],(EcosystemObject)obj);
                Constraint c = new Constraint(vars, func);
                cons.Add(c);
            }
        }
        return cons;
    }

    public Constraint[] GetConstraints(){
        List<Constraint> cons = new();
        cons.AddRange(EachEdge((hj, hl, hr, tr, tj, tl, system) => {
            return ((string)hj == "Carnivore") ? ((string)hr == "Predation" && ((string)tj == "Photosynthesizer")) ? 1 : 0 : 0;}));
        cons.AddRange(EachNode((vals, node, system) => {
                int minimum_predation = 1;
                int maximum_reward = 0;
                if ((string)vals[0] == "Carnivore"){
                    foreach(int edge in node.EdgeIndicies()){
                        int e = edge - node.JobIndex();
                        minimum_predation -= ((string)vals[e] == "Predation" ) ? 1 : 0;
                    }
                }
                return Math.Max(maximum_reward, minimum_predation);
            }));
    /**
        cons.AddRange(EachNode((vals, node, system) => {
                int minimum_predation = 1;
                int maximum_reward = 0;
                int errors = 0;
                if ((string)vals[node.JobIndex()] == "Herbivore"){
                    foreach(int edge in node.EdgeIndicies()){
                        int e = edge - node.JobIndex();
                        minimum_predation -= ((string)vals[edge] == "Predation" && ((string)vals[node.TailIndex(edge)] == "Photosynthesizer")) ? 1 : 0;
                        errors += ((string)vals[edge] == "Predation" && ((string)vals[node.TailIndex(edge)] != "Photosynthesizer")) ? 1 : 0;
                    }
                    return Math.Max(maximum_reward, Math.Max(minimum_predation, errors));
                }
                return 0;
            }));
        **/
        return cons.ToArray();
    }

    /**
        A relation that sums the error over all nodes, the function given in its construction is basically an ecosystem relation too
        It must take in the set of values, the specific node, and then the ecosystem object and return an int value
    
    public class EachNode : Relation{
        public Func<object[], EcoNode, EcosystemObject, int> function;
        public EachNode(Func<object[], EcoNode, EcosystemObject, int> f){function = f;}

        public int evaluate(object[] values, ProceduralObject obj)
        {   
            EcosystemObject sys = (EcosystemObject)obj;
            int errors = 0;
            foreach(EcoNode node in sys.nodes){
                errors += function(values, node, sys);
            }
            return errors;
        }
    }
    **/

    /**
        A relation that sums the error over all edges, each edge is counted twice, once from each direction
        the given function takes in a head value, trophic level, forward relation value, backwards relation value, the tail value, and trophic level
    
    public class EachEdge: Relation{
        public Func<object, object, object, object, object, object, int> function;
        public EachEdge(Func<object, object, object, object, object, object, int> jobA_levelA_relationA_relationB_jobB_levelB){function = jobA_levelA_relationA_relationB_jobB_levelB;}

        public int evaluate(object[] values, ProceduralObject obj)
        {   
            EcosystemObject sys = (EcosystemObject)obj;
            int errors = 0;
            foreach(EcoNode node in sys.nodes){
                foreach(int edgeIndex in node.EdgeIndicies()){
                    var tail = node.Tail(edgeIndex);
                    errors += function(values[node.JobIndex()], values[node.TrophicLevelIndex()], values[edgeIndex], values[tail.EdgeIndex(node)], values[tail.JobIndex()], values[tail.TrophicLevelIndex()]);
                }
            }
            return errors;
        }
    }
    **/
}
