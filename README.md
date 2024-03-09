
**

## Procedural City Simulation by Constraint Satisfaction Problems
**CMSC421 Final Project**
**

## Current Implementation and Changes

 - Changed CSP to work with a list of ints that reference static arrays of variables and domain objects. Cuts down on performance issues significantly.


## **Proposal**

**Project Overview:** The project aims to develop a procedural city generation and simulation for a game environment using Constraint Satisfaction Problem (CSP) techniques. 

**Key Features:**

-   General CSP Solver: Implement a backtracking algorithm that works for a set of generic Variables, Domains, and Constraints. Incorporate optimizations and alterations as necessary.
    
-   Dynamic Constraint Alteration: Design a specialized CSP that utilize local consistency and incorporates minimax optimizations to travel from an initial state produced from an old set of constraints to a new valid solution space satisfying updated constraints. This attempts to simulate structuraly changes to the city

- Sequential Problems and Solutions: Abstract properties of the city like the building composition is determined by the inital CSP solution. These properties are used in the following CSP to determine locations for the buildings. Iteratively more layers can be incorporated to increase the level of detail of the city. 
    
-   Unity Integration: Use the 3D Unity engine to visualize the results of the generator as well as to provide an environment for interfacing with dynamic constraint simulation. 
    


-   Save Load & Tag System: Design the domain so that objects can be stored in a Json format containing tags to inform the CSP about which constraints to apply and what variables to pass the constraint. There will be a dictionary file with templates for objects as well as the output of the CSP creating an output file with each instanced copy of the object and its location.
    
-   Dynamic Constraint Alteration: Implement a mechanism for dynamically altering constraints, allowing for the simulation of settlement changes over time. Develop an intermediary CSP to transition between different goal states while minimizing differences in object positions. The cost function can be weighted so that larger landmarks are more permanent.


**Conclusion:** The project will provide a foundation for the development of a simulated city system using CSP techniques that enables a game world to feel alive without tracking individual objects; the updates act on the set of constraints and the domain instead of the solution. This should allow detailed large scale worlds with computational cost mostly associated with the number of constraints, since the CSP generation will be done infrequently the number of objects should be irrelevant for performance.

Future expansion on this project is to create a version of this generator with more breadth and depth. Nesting these generators so that the parent generator creates child objects that are constraints and domains for the object’s generator means that a game could simulate a vibrant world with the finest level of detail while only paying the cost of updating details in the currently simulated reference frame.

## Implementation

**Proposed Layers of Problems**
- Building Composition: Based off of variables like city size, population size, and industry sizes, generate a list of buildings such that the city has sufficient housing and employment, without exceeding the size limit.
- Road Placement: Based off the city size, create a list of points. These points will be placed randomly satisfying loose symetry constraints (how loose should also be tied to the order attribute). Place roads connecting these points with straitness and angle of intersection related to the city's order attribute
- Building Placement: Assign industries to districts tied to the points assigned by the roadplacement. Next place buildings along roads weighted that they are closer to their appropriate district
- People Generation: People need only be generated as needed, a person will have a job building and a house building in proximity. Given a set of buildings, return a set of people.
- People Placement: People do tasks either for work, chores at their house, etc... Assign a task from the list of possible tasks and assign a location somwhere along the progression of the task.

