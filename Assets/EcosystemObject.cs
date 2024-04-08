using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class EcosystemObject : ProceduralObject
{
    public static int MIN_SPECIES = 6;
    public static int MAX_SPECIES = 9;
    public static int MIN_TROPHIC_LEVELS = 3;
    public static int MAX_TROPHIC_LEVELS = 5;

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
    public static System.Random rand = new System.Random();
    //if the object is closer to the front of the domain array, it is more likely to be sorted higher
    Func<int,int> WEIGHTED_RANDOM = (x) => {
        int variability = 8; //how many values should a given value overlap with, anything beyond variablity distance in the domain will be checked afterwards
        return rand.Next( x - (variability / 2), x + (variability / 2));
    };
    public object[] relationships = new string[4]{
        "Neutralism",
        "Competition",
        "Predation",
        "Mutualism",
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
                edges[i] = new Variable(name + " to " + tail.name, sys.relationships);
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
        
        /**
        Func<int,int> randValue = (x) => rand.Next();
        foreach(Variable var in ecoVariables){var.domainSelector = randValue;}
        **/

        //Later can implement probability distributions on the random value to influence the solution
        foreach(Variable var in ecoVariables){var.domainSelector = WEIGHTED_RANDOM;}

        //need to order evaluation of variables so it does niches first everything else after
        //layers[0].variableSelector= (var) => (var.name.Contains("niche")) ? 0 : 1;

       
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
        //Each constraint checks for complete arc consistency of the set of variables, so we want to minimize the number of variables included and the number of constraints
        //A good way to do this is to have a single constraint that sums the results of all the relations
        cons.AddRange(EachEdge((hj, hl, hr, tr, tj, tl, system) => {
                //Carnivores cant eat plants
                int errors = ((string)hj == "Carnivore") ? ((string)hr == "Predation" && ((string)tj == "Photosynthesizer")) ? 1 : 0 : 0;;
                //Herbivores cant eat anything but plants
                errors += ((string)hj == "Herbivore") ? ((string)hr == "Predation" && ((string)tj != "Photosynthesizer")) ? 1 : 0 : 0;;
                //plants cant eat anything
                errors += ((string)hj == "Photosynthesizer") ? ((string)hr == "Predation") ? 1 : 0 : 0;
                //Something eating something else must be at a higher trophic level
                errors += ((string)hr == "Predation") ? ((int)hl >= (int)tl) ? 0 : 1 : 0;
                //Things with the same job at the same trophic level must compete
                errors += (hj == tj && hl == tl &&(string)hr != "Competition") ? 1 : 0;
                errors += ((string)hr == "Competition" && hj != tj) ? 1 : 0;
                //Competition should be mutual
                errors += ((string)hr == "Competition" && (string)tr != "Competition") ? 1 : 0;
                return errors;}));
        cons.AddRange(EachNode((vals, node, system) => {
                int errors = 0;
                int MAX_REWARD = 0;
                //Carnivores and herbivores must eat at least 1 thing
                if ((string)vals[0] == "Carnivore" || (string)vals[0] == "Herbivore"){
                    int minimum_predation = 1;
                    foreach(int edge in node.EdgeIndicies()){
                        int e = edge - node.JobIndex();
                        minimum_predation -= ((string)vals[e] == "Predation" ) ? 1 : 0;
                    }
                    errors += Math.Max(minimum_predation, MAX_REWARD);
                }
                return errors;
            }));
        //Adds a constraint on the whole graph
        cons.Add(new Constraint(ecoVariables, (vals, obj) => {
            int errors = 0;
            //Checks that there is at least 1 carnivore
            int minimum_carnivores = 1;
            foreach(EcoNode node in nodes){if ((string)vals[node.JobIndex()] == "Carnivore"){ minimum_carnivores--;}}
            errors += Math.Max(minimum_carnivores, 0);

            return errors;
            }));
        return cons.ToArray();
    }

}
