using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EcosystemObject : ProceduralObject
{
    public static int MIN_SPECIES = 3;
    public static int MAX_SPECIES = 5;
    public static int MIN_TROPHIC_LEVELS = 2;
    public static int MAX_TROPHIC_LEVELS = 5;

    public int speciesCount;

    // our dictionaries that store stuff, instead of using objects just use strings for niches, relations, etc...
    public Dictionary<string, object> valueLookup; //keys will be from jobs or relationships array and values will be idk...
    
    //Our domains for each node and edge
    public object[] jobs = new string[]{
        "Photosynthesizer",
        "Herbivore",
        "Carnivore",
    };
    public static System.Random rand = new System.Random();
    //if the object is closer to the front of the domain array, it is more likely to be sorted higher
    Func<int,int> WEIGHTED_RANDOM = (x) => {
        int variability = 8; //how many values should a given value overlap with, anything beyond variablity distance in the domain will be checked afterwards
        return rand.Next( x - (variability / 2), x + (variability / 2));
    };
    public object[] relationships = new string[]{
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
    //Nodes in the graph relating species niches, trophic levels, and jobs... 
    public class EcoNode{
        public string name;
        public Variable job;
        public Variable trophicLevel;
        public Variable[] edges;
        public EcosystemObject sys;
        public Variable[] speciesAttributes;
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
        public (Variable, EcoNode)[] GetArcs(){
            var arcs = new (Variable, EcoNode)[sys.speciesCount - 1];
            int[] eIndicies = EdgeIndicies();
            int jobIndex = JobIndex();
            for(int i = 0; i < eIndicies.Length; i++){
                int relativeIndex = eIndicies[i] - (jobIndex + 1);
                arcs[i] = (edges[relativeIndex], Tail(eIndicies[i]));
            }
            return arcs;
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

        layers = new Graph[]{new Graph(ecoVariables, ecoConstraints, this)};
        
        /**
        Func<int,int> randValue = (x) => rand.Next();
        foreach(Variable var in ecoVariables){var.domainSelector = randValue;}
        **/

        //Later can implement probability distributions on the random value to influence the solution
        foreach(Variable var in ecoVariables){var.domainSelector = WEIGHTED_RANDOM;}
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

        //After changes to the algorithm, its no longer necessary to combine all the constraints or to minimize the variables included
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
    
    /**
        Once the graph variables have their values set, generate variables for traits and create creatures off of that...
    **/
    
    /**
        in an eco system there is finding (camo + stealth vs senses), catching (speed + agility vs endurance + shelter), killing (jaws + claws vs armour), then digesting (posion + seeds)...
        Each species has a value for each of these traits and if it is a predator then its attributes work so it can catch its prey...
        domains with as a type,cost tuple, specific domains are cross product ({types}, cost) typles where costs are multiplied
        general domains: (underground, 2f), (ground, 1f), (canopy, 1.5f), (sky, 2.5f)...
        cross product with the specific domains
        shelter: permanent, temporary, mobile 
        movement: none, standard, speed, endurance
        feeding: search, wait
    **/
    public struct Trait{
        public string name;
        public string[] tags;
        public float cost;
        public string[] counters;
        public string[] weaknesses; 
        public Trait(string n = "", float c = 1f, string[] t = null, string[] s = null, string[] w = null){
            name = n;
            cost = c;
            tags = t;
            counters = s;
            weaknesses = w;
        }
        //Basically sums together the traits and returns the list of tags that aren't countered by the other person's traits....
        //The final list of traits is useful why??? Certain relations require each side to have different effective tags. If it is neutral with a carnivore then...??
        public (string[], string[])  EffectiveTags(List<Trait> traitsA, List<Trait> traitsB){
            //Construct a list of all present tags and traits
            List<string> tagsA = new();
            List<string> tagsB = new();
            List<string> countersA = new();
            List<string> countersB = new();
            foreach(Trait t in traitsA){
                tagsA.AddRange(t.tags);
                tagsA.Add(t.name);
            }
            foreach(Trait t in traitsB){
                tagsB.AddRange(t.tags);
                tagsB.Add(t.name);
            }
            //Find the traits that have present weaknesses and remove them
            traitsB.RemoveAll(trait => tagsA.Intersect(trait.weaknesses).Any());
            traitsA.RemoveAll(trait => tagsB.Intersect(trait.weaknesses).Any());
            
            //Create the new list of tags without the traits that had weaknesses
            tagsA = new();
            tagsB = new();
            foreach(Trait t in traitsA){
                tagsA.AddRange(t.tags);
                countersA.AddRange(t.counters);
            }
            foreach(Trait t in traitsB){
                tagsB.AddRange(t.tags);
                countersB.AddRange(t.counters);
            }
            
            //Remove the tags that are countered
            tagsA.RemoveAll(tag => countersB.Contains(tag));
            tagsB.RemoveAll(tag => countersA.Contains(tag));

            return (tagsA.ToArray(), tagsB.ToArray());

        }
        // A trait gives the creature the tags.  it also removes the oppenets tags from the strengths (or if a trait name counters it...) iff the weakness doesn't exist in the opponent (as a trait or a tag)
        //If a trait is countered remove an instance of all of its tags... if a tag is countered remove all instances of the tag
    }
    public object[] traits = new object[]{
        //Predators hunting or prey avoiding being stalked
        new Trait("Scent", 1.1f, new string[]{"Alert", "Search"}), new string[]{"Hidden"}, new string[]{"Stink"},
        new Trait("Sight", 2.2f, new string[]{"Alert", "Search"}), new string[]{"Hidden"}, new string[]{"Camoflage"},
        new Trait("Hearing", 1.2f, new string[]{"Alert", "Search"}), new string[]{"Hidden"}, new string[]{"Quiet"},

        new Trait("Stink", 1.5f, new string[]{}), new string[]{"Scent", "Smell"}, new string[]{},
        new Trait("Camoflage", 1f, new string[]{"Hidden"}), new string[]{"Sight"}, new string[]{},
        new Trait("Stealth", 1.5f, new string[]{"Quiet", "Hidden"}), new string[]{"Hearing"}, new string[]{"Alert"},
        
        //Fighting traits
        new Trait("Claws", 1f, new string[]{"Cutting"}), new string[]{}, new string[]{"Armoured"},
        new Trait("Fangs", 1f, new string[]{"Piercing"}), new string[]{}, new string[]{},
        new Trait("Jaws", 1.5f, new string[]{"Crushing"}), new string[]{"Scales"}, new string[]{"Dense"},

        new Trait("Scales", 1f, new string[]{"Armoured"}), new string[]{"Cutting", "Piercing"}, new string[]{},
        new Trait("Thickness", 1f, new string[]{"Dense"}), new string[]{"Crushing"}, new string[]{},
        new Trait("Spikes", 1.5f, new string[]{"Piercing"}), new string[]{}, new string[]{},
    };

    public object[] generalDomains = new object[]{("Underground", 2f),("Ground", 1f), ("Canopy", 1.5f), ("Sky", 2.5f)};
    //What advantages does the creature have to escape from predators or catch prey?
    public object[] movement  = new object[]{("None", 0f), ("Standard", 1f), ("Speed", 2f), ("Endurance", 1.5f)};
    //What advantages does the creature have to avoid getting caught...
    public object[] defenses = new object[]{("Alert", 2f), ("Hidden", 1f), ("None", 0f)};
    //the senses that need to be hidden from or alerted too
    public object[] senses = new object[]{("Smell", 1.1f), ("Sound", 0.8f), ("Sight", 1.8f), ("None", 0f)};
    //The physical traits the creature has to attack or defend from predators and prey
    public object[] physicalTraits = new object[]{("Claws", 1f), ("Poison", 1.2f), ("Spikes", 1f), ("Armour", 1.2f), ("Acid", 1f)};
   
    public static float BUDGET = 1.5f;

    public object[] CrossDomains(object[] general, object[] specific){
        object[] cross = new object[general.Length * specific.Length];
        for(int i =0; i < general.Length; i++){
            (string gs, float gf) = ((string, float))general[i];
            for(int j=0; j< specific.Length; j++){
                (string ss, float sf) = ((string, float))specific[j];
                cross[i*specific.Length + j] = ((gs, ss), gf * sf);
            }
        }
        return cross;
    }
    public void GenerateSpecies(){
        
        var senseDomain = CrossDomains(defenses, senses);
        var movementDomain = CrossDomains(generalDomains, movement);
        List<Variable> vars = new();
        List<Constraint> cons = new();
        foreach(EcoNode node in nodes){
            /** for each node, generate some basic variables and constraints over those variables... **/
            int trophicLevel = (int)node.trophicLevel.GetValue();
            string job = (string)node.job.GetValue();
            Variable moveVar = new Variable(node.name + " movement", movementDomain);
            Variable senseVar = new Variable(node.name + " senses", senseDomain);
            Variable physicalVar = new Variable(node.name + " physical traits", physicalTraits);
            node.speciesAttributes = new Variable[]{moveVar, senseVar, physicalVar};
            vars.AddRange(node.speciesAttributes);
            //Ads a constraint that all the advantages cost less than the budget for the trophic level
            cons.Add(new Constraint(node.speciesAttributes, (vals, sys) => {
                float budget = BUDGET * (float)Math.Pow(2, trophicLevel);
                foreach(object obj in vals){
                    (object tags, float cost) = ((object, float))obj;
                    budget -= cost;
                }
                return Math.Max(0, (int)Math.Round(budget));
            }));
        }
        /**Adds constraints on each arc, can use the nodes relationships as parameters...**/
        foreach(EcoNode node in nodes){
            int trophicLevel = (int)node.trophicLevel.GetValue();
            string job = (string)node.job.GetValue();
            foreach((Variable relation, EcoNode tail) in node.GetArcs()){
                string rel = (string)relation.GetValue();
                string tailJob = (string)tail.job.GetValue();
                int tailLevel = (int)tail.trophicLevel.GetValue();

                var arcVars = new Variable[]{};
                arcVars.AddRange(node.speciesAttributes);
                arcVars.AddRange(tail.speciesAttributes);
                cons.Add(new Constraint(arcVars, (vals, sys) => {
                        return 0;
                    }));
            }
        }
    }
}
