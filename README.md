
**

## Procedural City Simulation by Constraint Satisfaction Problems
**CMSC421 Final Project**
**

## Current Implementation and Changes

 - Changed CSP to work with a list of ints that reference static arrays of variables and domain objects. Cuts down on memory issues significantly. 


## **Proposal**

**Project Overview:** The project aims to develop a tool for procedural generation and simulation of complex objects in a game environment using Constraint Satisfaction Problem (CSP) techniques. Procedural generation typically utilizes sequential algorithms that often model real world physics actions, i.e. using tectonic plates in terrain generation. By utilizing a CSP instead of creating an object sequentially, we bruteforce search the space of all possible objects until we find one that looks like what we want. 

This project will implement thses techniques in C# and using the Unity engine to interface with the user. Our specific implementation will generate a city as a layout of buildings paired with a set of residents and employees. The simulation of the city will have buildings and people change over time in response to continusous variables like economic stimulus or discrete changes like natural disasters.

We hope that our implementation of these techniques will provide analysis into the significance of the advantages of this approach and the effectiveness of game environment unique optimizations.


**Key Features:**

- Complexity: The CSP model is advantageous since the underlying algorithms are independent of the set of end constraints that define an object. Instead of encoding an algorithm that result in an object, we define constraints that our object satisfies, decreasing code complexity during development. 

- Evolution: Another advantage is that not only can the developer manually craft content by creating the set of constraints the bound it, but can automate this process with genetic algorithms combining similar objects or utilizing gradients between sets of constraints. Especially for exploration themed games, this would be a valuable way to add an imense amount of themetically appropriate variety while limiting code and asset complexity. 

- Simulation: Evolution of a object into itself is simulation. By using a defined object as initial state for it's own CSP with updated constraints, we can bias the solution to one the resembles fewer differences. Creating the illusion of simulation. This process can be improved with weights assigning the importance of permanance to specific variables or values, and using those weights to determine ordering. For example, a city founded around a culturally important building is likely to retain a historical district in the old style despite the rest of the city evolving away. 

- Optimizaitons: Unfortunately the cost of solving a CSP is dependent on the size of the solution space (number of values ^ number of variables) and the number of constraints. Where a sequential algorithm is likely to have runtime only associated with the number of variables. We believe this problem can be avoided almost in its entirely. It is common for a video game to only simulate and render what is immediately in front of the player, omitting and occluding the rest. For our CSP we can implement this broad technique in a handful of ways. 

- Layers (Structure): By structuring objects into discrete layers, we can separate the generation into the sum of sequentional more managable CSPs, i.e. buildings layer, roads layer, people layer, jobs layer, etc... The result of precceding CSPs inform the following CSPs, so although they must be solved in sequence, each layer's csp searches a much smaller space than the state space of the entire complex object.

- Partial Determination (Filtering): Additionally, the layers themselves can be optimized. Complex objects are likely to not fit entirely within the render distance. So by only partially determining the values needed on screen, we can postpone most computations and save on memory. I.e. For a city, the people only need to be generated when the buildings they live an work in are rendered. And if not all the buildings are ever visited then this layer will never be completely determined. Additionally, the player wont remember most people they meet, so any insignificant people can be omitted from permanent memory. 

- Relaxed Problem (Arc Consistency): To implement partial determination, we need to enforce consistency which scales in time complexity with the number of constraints. However, we can limit this increased run time by only enforcing consistency to a certain depth. Although this would result in ultimately inconsistent objects, they would consist of several locally consistent fragements, ideally with a sufficient depth it would be difficult for a player to notice.

- Least constraining (Ordering): It is especially important to reduce the amount of inconsistencies that arise among fragments to select the least constraining values during partial determinatino. However, the calculation for this may be expensive to compute completely so a heuristic could be used and a termination depth.

    
**Project Specific Implementation Features**

-   Unity Integration: Use the 3D Unity engine to visualize the results of the generator as well as to provide an environment for interfacing with dynamic constraint simulation. 


-   Save Load & Tag System: Design the domain so that objects can be stored in a Json format containing tags to inform the CSP about which constraints to apply and what variables to pass the constraint. There will be a dictionary file with templates for objects as well as the output of the CSP creating an output file with each instanced copy of the object and its location.
    

**Conclusion:** The project will provide a foundation for the development of diverse sets of assets utilizing CSP techniques, enabling immersive and coherent game worlds with increased variety at reduced design complexity. Additionally, simulation of the world can be optimized by represention through evolution of the system objects instead of individual actions of the actors in the system. The dynamic simulation of the appropriate level of detail will enable an infinite levels of detail for large scale worlds.

A lot of these techniques for this project are only useful for games. We are constrained by having our generation not hinder the user's frame rate, but benefit that our goal is more broad. A game seeks to immerse a player in it's world, and let create feelings. As long as little details and inconsistencies are too minor to be noticed, the resulting world will have the desired effect. As such our CSP techniques need only be good 'enough.' For real world generation applications (planning systems), CSPs are still extremely useful. However, more effort would be put into generating a set of solution states and conducting analysis across the set, since time would be less important than optimality of the solition.

## Implementation

**Proposed Layers of Problems**
- Building Composition: Based off of variables like city size, population size, and industry sizes, generate a list of buildings such that the city has sufficient housing and employment, without exceeding the size limit.
- Road Placement: Based off the city size, create a list of points. These points will be placed randomly satisfying loose symetry constraints (how loose should also be tied to the order attribute). Place roads connecting these points with straitness and angle of intersection related to the city's order attribute
- Building Placement: Assign industries to districts tied to the points assigned by the roadplacement. Next place buildings along roads weighted that they are closer to their appropriate district
- People Generation: People need only be generated as needed, a person will have a job building and a house building in proximity. Given a set of buildings, return a set of people.
- People Placement: People do tasks either for work, chores at their house, etc... Assign a task from the list of possible tasks and assign a location somwhere along the progression of the task.

