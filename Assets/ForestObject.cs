using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForestObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public class ProceduralForest : ProceduralObject
{
    public ProceduralForest() {
        /**
            Defining the eco system layer, we need a meta layer determining things like size thus number of creatures (size of domain) and maybe themes to enforce on the ecosystem (constraints on the edges)
            For all creatures in the ecosystem we need to assign jobs that let it fit into the ecosytem graph constrained by things like conservation of energy
            For each creature, we need to define traits that are suitable for one to complete the job it is tasked with while also ensuring that the traits dont make it  more suited for a different job
        **/
        /**
            Meta layer, 
                determioning size of the forest and any global themes
        **/
        Variable forestSize = new("size", new object[5]{3,4,5,6,7});
        Variable forestTheme = new("theme", new object[2]{"symbiotic", "predation"});
        CSPGraph metaLayer = new(new Variable[2]{forestSize, forestTheme}, new Constraint[0]{});
        //Solve the first layer immediately to get info for the rest of the layers
        this.layers = new CSPGraph[1]{metaLayer};
        this.Solve(0);
        int size = (int)this.solution["size"];

        /**
            The ecosystem graph layer, 
                assiging nodes their values and arcs their values making sure jobs and relations match 
                as well as satisfying basic composition of a general ecosystem as well as the theme of the forest
        **/
        //Create the domain of jobs and relations (variable values and arc values)
        string[] jobs = new string[5]{"source", "decomposer", "predator", "scavenger", "producer"};
        string[] relations = new string[4]{"symbiotic", "parisitic", "predatory", "asymbitotic"};
        //Somhowe constrain the jobs and relations (variable values and arc values) can depend on the theme too???
        Constraint[] cons = new Constraint[0]{};
        Variable[] ecoNodes = new Variable[size];
        //Define an edge for each node to node thus node*(node-1) edges, could also define self edges...?
        Variable[] ecoArcs = new Variable[size * (size - 1)];
        for(int i = 0; i < size; i++){
            ecoNodes[size] = new Variable("ecoNode"+i, jobs);
            for(int j = i * (size - 1); j < (i + 1) * (size - 1); j++){
                ecoArcs[j] = new Variable("ecoEdge"+i+"to"+j, relations);
            }
        }
        CSPGraph ecoLayer = new(ecoArcs.Concat(ecoNodes).ToArray(), cons);
        this.layers.Append(ecoLayer);
        this.Solve(1);

        /**
            The population layer,
                creating creature populations that fill the role of their associated node on the graph and are constrained by their relations with other populations

            Maybe see about tweaking the CSP algorithms to enable a layer to assign a set of domains as a solution instead of a single domain???
                with traits listing all permutations of traits would expand the domain size significantly,
                alternatively one could define a layer per population and have each trait as a variable with a boolean if it is inherited or not...
        **/
        Variable[] type = new Variable[size]; // The general template of creature to use, can have preferences towards strengths and weknesses
        string[] types = new string[6]{"avian", "tree", "grass", "shrub", "quadraped", "bipedal"}; //helps define the physical structure of anatomy and limbs

        Variable[] traits = new Variable[size]; //Set of traits that explain "why" a population has its job and specific relations
        //Create the domain of traits (strengths and weaknesses) and the domains for other properties
        string[] strengths = new string[6]{"fast", "strong", "agile", "flight", "burrowing", "tough"};
        string[] weaknesses = new string[3]{"slow", "weak", "clumsy"};

        Variable[] popSize = new Variable[size]; // popsize = # of creatures, popenergy = % of ecosystem resources in this population
        object[] populationSizes = Enumerable.Range(10, 101).Select(i => (object)i).ToArray();

        Variable[] popEnergy = new Variable[size];// so when creating individual creatures on avg can spend popenergy / popsize on each creature
        object[] populationEnergy = Enumerable.Range(75,100).Select(i => (object)i).ToArray();

        Variable[] packSize = new Variable[size]; //How grouped are they on avg
        object[] packSizes = Enumerable.Range(1, 50).Select(i => (object)i).ToArray();

        Variable[] packDistribution = new Variable[size]; //How they are distributed
        string[] packDistributions = new string[4]{"even", "random", "near food", "away predators"};

        //Constraints on traits like cant be both fast and slow, constrain the strengths to satisfy arcs and weakness to satisfy arcs leading into it
        Constraint[] populationConstraints = new Constraint[0]{};
        for (int i = 0; i < size; i++){
            traits[i] = new Variable("traits"+i, strengths.Concat(weaknesses).ToArray());
            popSize[i] = new Variable("popsize"+i, populationSizes); 
            popEnergy[i] = new Variable("popenergy"+i, populationEnergy);
            packSize[i] = new Variable("packsize"+i, packSizes);
            packDistribution[i] = new Variable("packdistribution"+i, packDistributions);
        }
        var vars = traits.Concat(popSize.Concat(popEnergy.Concat(packSize.Concat(packDistribution)))).ToArray();//Make more efficient with copy
        this.layers.Append(new CSPGraph(vars, cons));

        /**
            Creature layers: template + instance,
                each population has a layer that produces an instance of a creature from the population
                these creatures are constrained by their type, traits, energy (pop energy / popsize)       
        **/
        Variable numLimbs = new Variable("numLimb", new object[5]{3,4,5,6,7});

        //The domain can contain delegates that return objects?
        //Maybe this is where I should separate the code into a creature class???


        /**
            Location Layer,
                and lastly their location is determined by their pack and pack distribution
                if pack distribution is even then there is a layer per chunk of forest, 
                otherwise all packs across all creatures are part of one layer so that nearness and farness can be satisfied 
        **/
    }
    public ProceduralForest(Variable[] vars, Constraint[] cons) : base(vars, cons){}
    public ProceduralForest(Variable[] vars, Constraint[] cons, CSPGraph[] layers) : base(vars, cons, layers) {}
}

public class Forest {
    
}


public class Creature : Entity{
    
}
public class Entity{
    Vector3 location;
    Quaternion orientation;
    bool drawn;
    bool simulated; 
}