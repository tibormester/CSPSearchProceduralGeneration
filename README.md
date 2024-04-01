
**

## Procedural Forest Simulation by Constraint Satisfaction Problems
**CMSC421 Final Project**

**

## Current Implementation and Changes
March 26
 - New class design
 - Changed from City application to forest application
 - Refactored Backtracking
 - Implemented Arc Consistency Algorithm
 - Updated README with plans

Feb 27

 - Changed CSP to work with a list of ints that reference static arrays of variables and domain objects. Cuts down on memory issues significantly. 


## **Proposal**

**Project Overview:** 

The project aims to develop a tool for procedural generation and simulation of complex objects in a game environment using Constraint Satisfaction Problem (CSP) techniques. Procedural generation typically utilizes algorithms that often model real world physics actions, e.g. using tectonic plates in terrain generation. By utilizing a CSP instead of creating an object through a sequence of intermediary processes, we bruteforce search the space of all possible objects until we find one that looks like what we want. 

This project will implement these techniques in C# and using the Unity engine to interface with the user. Our specific implementation will generate a Forest with an ecosystem graph that describes the roles and evolution of creature populations which in turn provide a template for the insantiation of flora and fauna entities in the 3d environment. 

We hope that our implementation of these techniques will provide analysis into the effectiveness of this approach as well as illuminate areas in need of optimization.


**Key Features:**

- Complexity: The CSP model is advantageous since the underlying algorithms are independent of the set of end constraints that define an object. Instead of encoding an algorithm that results in an object, we define constraints that our object satisfies, decreasing code complexity during development. Additionally, this enables the encoding of more complex behaviours or emergent properties more easily. Specifically, this approach benefits the mapping of qualitative values to quantative properties. Describing the values a set of constraints on the properties, enables the system to generate objects with an intended 'feeling,' without a developer worrying about how to design a system to mix and match the qualitative properties.

- Variety: Another advantage is that not only can the developer manually craft content by creating the set of constraints the bound it, but can automate this process with genetic algorithms combining similar objects or utilizing gradients between sets of constraints. Especially for exploration themed games, this would be a valuable way to add an imense amount of themetically appropriate variety while limiting code and asset complexity. We plan to use this in our forest by defining model creatures for each 'pack' of creatures instanced and adding a gradient across some properties within the pack, resulting in minor variation in packs of creatures but avoiding chaotic randomness.

- Simulation: To perform a simulation, it scales with the number of objects being simulated. To optimize the computation, we may utilize a simplified simulation at a higher level of abstraction: e.g. instead of tracking individual creatures interacting we track populations. Figuring out the emergent higher level systems can be difficult and result in complex equations. Luckily CSPs are ideal for this situation. By describing the emergent system as a set of simple rules, the developer can avoid worrying about the details of what algorithm is needed to produce that end result. Additionally, to ensure that the simulation is continuous once can incorporate the old object as the initial guess for the new solution, using an algorithm that searches out from the initial guess. Thus one gets the minimal change necessary to satisfy the simulation constraints. We plan to model evolution in our forest by defining constraints on how types of populations change based on their roles and interactions (e.g. predation, adaption, random mutation). 


**Optimizations:**

Unfortunately the cost of solving a CSP is dependent on the size of the solution space (number of values ^ number of variables) and the number of constraints. Where a traditional algorithm is likely to have runtime only associated with the number of variables. There are a handful of established techniques for optimization that fall into the three catagories of strcture, filtering, and ordering. Additionally, we believe further application specific optimizations can be introduced. It is common for a video game to only simulate and render what is immediately in front of the player, omitting and occluding the rest. For our CSP we can implement this broad technique by allowing errors and splitting complex problems into locally consistent pieces that are generated independently. 

- Layers (Structure): By structuring objects into discrete layers, we can separate the generation into the sum of sequentional more managable CSPs, e.g. buildings layer, roads layer, people layer, jobs layer, etc... The result of precceding CSPs inform the following CSPs, so although they must be solved in sequence, each layer's csp searches a much smaller space than the state space of the entire complex object.

- Partial Determination (Relaxed Problem): Layers can also define repeated information that is spatially indepenent. Complex objects are likely to not fit entirely within the render distance. So by only determining the values needed on screen, we can postpone most computations and save on memory (or perform the computations in parallel). e.g. For a city, the people only need to be generated when the buildings they live an work in are rendered. And if not all the buildings are ever visited then this layer will never be completely determined. Additionally, the player wont remember most people they meet, so any insignificant people can be omitted from permanent memory. 

- Arc Consistency (Filtering): Instead of brute forcing the entire search space, we can filter with arc consistency using the AC-3 algorithm which is O(c d^3) where c is the number of constraints and d is the maximum domain size. Note that this only shrinks the domains and doesn't necessarily yield a solution.

- Heuristics (Ordering): Often CSPs use orderings to determine which variable and value to assign first with techniques like least constraining value or most constrained variable to increase the odds that a specific run of backtracking will be successful. Instead we can use heuristic functions that are similar to a traditional procedural generation function to produce these values and ensure variable ordering if the heuristics are dependent on previous variables. What this means is that this framework should be able to incorportate the traditional approach to procedural generation and take advantage of it's average time complexity.

    
**Project Specific Implementation Features:**

-   Unity Integration: Use the 3D Unity engine to visualize the results of the generator as well as to provide an environment for interfacing with dynamic constraint simulation. 


-   Save/Load: Use JSON serialization for storing object data. Represent constraints as objects using relation and operator primitives, enabling dynamic constraint creation. 

-   Example: Demonstrate all features of the framework through the implementation of a procedural forest ecosystem. Show how qualitative traits can be related to quantitive values with a set of simple rules, enabling automatic microscopic quantative changes to the ecoystem governed by macroscopic qualitative changes.
    

**Conclusion:** 

The project will provide a foundation for the development of diverse sets of assets utilizing CSP techniques, enabling immersive and coherent game worlds with increased variety at reduced design complexity. Additionally, simulation of the world can be optimized by represention through evolution of the system instead of simulating individual actions of the actors in the system. The dynamic simulation of the appropriate level of detail will enable an infinite levels of detail for large scale worlds.

A lot of these techniques for this project are only useful for games. We are constrained by having our generation not hinder the user's frame rate, but benefit that our goal is more broad. A game seeks to immerse a player in it's world, and let create feelings. As long as little details and inconsistencies are too minor to be noticed, the resulting world will have the desired effect. As such our CSP techniques need only be good 'enough.' For real world generation applications (planning systems), CSPs are still extremely useful. However, more effort would be put into generating a set of solution states and conducting analysis across the set, since time would be less important than optimality of the solition.

## Implementation of our CSP Framework

**Infinite Variables and Domains**

For large complex objects it may be desirable for a random number of objects to have random positions or more generally to have a random value from an infinite domain. We propose two solutions to reformat these desires to fit our framework:

- Random Number of Objects: Define a previous layer such that the variable is the population of objects with the value assigned the population's size constrain this by your desired bounds. e.g. if density d is desired constrain size = d * Area. Next let this layer inform the following layer of population size # of variables, assigning each variable object instance specific details.

- Infinite Domains: Infinite domains are a lot harder to compute and also uneccessarily precise for a game environment. Instead use a function that evenly spans the domain with the desired level of detail. For evenly distributed values from infinite domains like placing all trees anywhere in a forest, it is easier to assign each tree to a location on a grid with a value for displacement from the grid. This has the advantage of enabling partitioning of the forest into chunks of trees local to each other which can be utilized for partial rendering or determination of the forest object. Alternatively for the case of assigning a few objects not neccessarily evenly distributed but still random values from a large domain one can assign each order of magnitude separately. This is advantageous for constraints with smooth and continuous values over the domain. Since a value at a higher order of magnitude is likely to be evaluated similarily to itself plus the lower orders of magnitude.

**Classes and Fields**

 - Procedural Object: a procedural object has three parts, a template object (definition of variables and domains), a csp graph (formulation of the search problem as a list of constraints), and instances (solutions). For example, in our forest, we desire the generation of procedural plants, specifically trees and shrubs. The template object describes all plant objects defining things like location, where to draw leaves, trunk, and branches in a unified format. Next the csp graph has constraints such that searching for a tree yields something with a tall trunk and a lot of large branches while searching for a shrub yields the opposite. And lastly, when we get our solutions and wish to visualize the results, we can pass our soltions into the plant model to view converter.

 - Variables and Domains: Variables are the nodes in our CSP graph, each node has a set of values called the domain. The domain is a list of objects. It is convenient having variables and the algorithms acting on or from them work with the indicies of objects in the domain array instead of the objects themselves. Additionally, variables are designed to track a partial solution that is by default set to null when full to save space.

 - Constraints: A constraint is a k-nary relation between not necessarily same typed variables. A Unary constraint is a constraint onto the variable itself, while a binary constraint relates two variables. A k-nary constraint involves k variables relating some m variables to the remaining n variables such that n + m = k. If the relation is true, we say the constraint is satisfied. In our graph representaiton, we draw edges between variables if they are connected with constraints, these directed edges called arcs. An arc is said to be consistent if for every value in the head node, there exists some value in the tail such that the relation holds. For a k-nary constraint we can enforce arc consistency by treating the relation as a 1 to k-1 variable relation, computing for every value in the domain if there exists some combination of the k-1 variables' domains that satisfy the value. Note that this cost can be quite expensive for simply pruning so it may be more efficient to enforce pruning depth and consistency limits. If this is false we can prune the domain of the head of all inconsistent values and then it will be true. This arc consistency algorithm forms the basis for pruning the domains of the search tree for faster compution. For a weighted constraint graph, let 0 be consistent and greater values representing their cost. A partial solution will terminate when a cost exceeds its threshold. If no solutions are found find a solution that is likely to be close enough. Before backtracking calculate the average cost per varible and store the partial solution with minimal avg cost. If no solution is found proceed from the initial state, increasing the threshold to current cost + threshold. This resolves with a solution that is either valid or close enough to optimal in reasonable time. 

 - Ordering: For determining a solution to a CSP we can save time by picking an ordering like least constraining most constrained etc... additionally using a heuristic approach that orders values of the domain by the likelyhood of them being in a valid final solution could work. And lastly, having a method of randomizing these orderings such that the same object can generate a variety of instances but still being deterministic. For each variable and its domain, we need an ordering function....

 - Layers: With increasing complexity of an object, can lead to exponential growth in the size of the CSP graph. To utilize this framwork optimally it is best to decouple properties of objects into independent layers that can be computed in parallel or sequential layers. For example if the location of branches depends on the location of the trunk, one can decouple this by having the branch location property describe local displacament from the trunk so it can be calculated in parallel or for the trunk location to be used to generate the domain for the branches so that it could be calculated sequentially. 

**Procedural Forest Implementation**

The forest is fantasy themed in a game environment, filled with unusual creatures described by RPG stats (Strength, Dexterity, Constitution etc...). Like a real ecosystem, we want to generate the forest such that energy flows through it in a cycle with clear predator-prey (or more complex) relations. To create these relations, we will describe the strengths and weakness of these creatures with generalizations on their stats and abilities like strong and fast or cunning and agile etc... Then the relations between creatures define the constraints on those creature's traits, e.g. predators strengths should capitalize on prey's weaknesses or symbiotes should cover each other's weakenesses. The creatures themselves consist of a set of body parts nested in a tree like structure. Each part granting stats, abilities, or room to attach more parts. The final set and layout of parts satisfies the given strengths and weaknesses. In this way, we generate the ecosystem top down; starting with qualitive descriptions, we constrain the values of our final creature objects until we get the desired quantitive outcome that matches our descriptions.

- Forest Object:
    - Meta Layer:
        - size: Number of populations of creatures inhabiting the forest as well as the physical size
        - theme: A template of constraints to impose onto the ecosystem graph, gives a specific 'feeling' to the forest harmony vs competition vs. etc...
    - Ecosystem Graph Layer: i from 0 to size, j = i_pop1 * (size - 1) + i_pop2:
        - ecoNode[i]: Defines the ith population's job in the ecosystem
        - ecoArc[j]: Defines the relation between pop1 and pop2 from the perspective of pop1
    - Population Layer:
        - traits[i]: The physical characteristics of the creatures in population i, constraints on the population's ideal creature that garuntee certain body parts, min / max stats, or statuses necessary for fulfilling the job or ensuring the ecosystem graph's arcs hold true
        - popSize[i]: How many individual creatures exist in this population
        - popEnergy[i]: How many total resources to allocate to this population
        - packSize[i]: How many creatures should be generated together
        - packDistribution[i]: Where should packs generate, if random perturb a grid, otherwise do something else.

 - Population:
    - Ideal Creature: An instance of a creature in perfect health, so instances of undamaged creatures can point to this readonly copy until changes need to be made. 
    - Pack Details: Describes the size and distribution.
    - Creatures: A list of all creatures in the population organized by their packs.

 - Creature:
    - Model:
        - bodyParts: The root node of the body parts
        - status: The sum of all the parts, showing net stats and abiltities, temporary statuses, and calculated values like move speed
        - Tags: Describes the creature based on the structure of its body part graph, ecosystem job, or traits e.g. bipedal, scavenger, and cunning
    - Simulation: 
        - tasks: a set of actions that have costs and requirments but can produce results
        - values: a set of incentives and restrictions on the set of actions that change the utility of costs and results
        - priorities: needs like hunger, thirst, and rest that need to be sustained through tasks, if the priority isnt met, enqueue a task to the task stack
        - TaskStack: a stack of tasks, queued by priorities and carried out during simulation ticks, tasks can recursively generate new tasks to help satisfy unmet conditions
        - Reactions: Conditions to check every few simulation ticks that can interrupt and redirect tasks in the task stack in response to sensory input. E.g. if near a predator stop everything and queue a flee type task
    - View:
        - location, size, orientation etc...:
        - style: how to synthesize all the bodyparts together into a single creature

Loaded primitive data, not procedural:
 - Traits: Qualitatively describes a creature
    - Tags: Describes the trait, i.e. strength or weakness and how: mobility, offence, defence, etc... 
    - Constraint: a set of constraints either to be applied to itself, the population, or the inidividual e.g. include or exclude other traits, min or max pack size, or minimum or maximum stat values. 

 - Bodypart: Quantitatively describe a part of a creature
    - Durability: How much damage the part can take
    - Abilities: What the part does for the creature
    - Tags: Describes the part so it can be filtered by constraints
    - Part Graph: A gridlike inventory with type restricted slots indicating attached parts or organs

 - Abilities: Either actively or passively alters a creatures status, i.e. affecting their temporary condition or needs
    - Name, Description, Unique ID, Icon
    - conditions: A set of constraints to be satisfied before using the ability
    - prediction: What the expected outcome of the ability is
    - evaluation: Perform the ability (like triggering a root motion animation)

The simulation implemenation will be further defined at a later date.

 - Tasks:

 - Values:

 - Priorities:

- Tag:
    - Name, Description, Unique ID, Icon

