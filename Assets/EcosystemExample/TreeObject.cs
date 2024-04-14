using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Pool;

public class TreeObject : CreatureObject{

    public TreeObject(){
        root = new GameObject(name);
        GenerateTree();
    }
    public void GenerateTree(){
        GenerateName();
        GenerateTrunk();
        GenerateBranches();
        GenerateLeaves();
    }
    //Generates a name from a list of adjectives
    public void GenerateName(){
        //Define all the domains (repeating empty strings is inefficient but allows us to edit the probability one is selected without unique random domain value selection functions)
        object[] sizes = {"Great", "Mini", ""};
        object[] age = {"Ancient", "Old", "Young", "", ""};
        object[] shape = {"Flat", "Tall", "Big", "Large", "", "", ""};
        object[] color = {"Brown", "", "Black", "Red", "", "Pink", "Grey", ""};
        object[] material = {"Firm", "", "Thick", "Hard", "", "Soft", "Light", ""};
        object[] noun = {"Tree", "Wood", "Sapling", "Growth"};
        //Construct the variables
        var vars = new Variable[]{
            new Variable("sizes", sizes),
            new Variable("age", age),
            new Variable("shape", shape),
            new Variable("color", color),
            new Variable("material", material),
            new Variable("noun", noun),
        };
        //Construct the constraints
        var constraints = new Constraint[]{new Constraint(vars, (vals, sys) => 0)};
        
        //Create the CSP and solve it
        Graph nameLayer = new Graph(vars, constraints, this);
        nameLayer.BacktrackingSolve();

        //Output the results
        name = "";
        foreach(Variable var in vars){
            string adj = (string)var.GetValue();
            if(!adj.Equals("")) name += adj + " ";
        } 
        Debug.Log(name);
    }
    //Constructs a trunk bodypart
    public static float MIN_HEIGHT = 0.5f;
    public static float MAX_HEIGHT = 1.5f;
    public static float STEP_SIZE = 0.1f;
    public static float MIN_WIDTH = 0.2f;
    public static float MAX_WIDTH = 1f;
    public void GenerateTrunk(){
        //Generate the domains
        object[] heights = new object[Mathf.CeilToInt((MAX_HEIGHT - MIN_HEIGHT) / STEP_SIZE) + 1];
        for (int i = 0; i < heights.Length; i++) {heights[i] = MIN_HEIGHT + (STEP_SIZE * i);}
        object[] widths = new object[Mathf.CeilToInt((MAX_WIDTH - MIN_WIDTH) / STEP_SIZE) + 1];
        for (int i = 0; i < widths.Length; i++) {widths[i] = MIN_WIDTH + (STEP_SIZE * i);}
        //Create the CSP
        var vars = new Variable[]{new Variable("height", heights), new Variable("width", widths)};
        var cons = new Constraint[]{};
        Graph trunkLayer = new Graph(vars, cons, this);
        trunkLayer.BacktrackingSolve();
        //Translate the solution into an output
        Joint rootJoint = new Joint();
        BodyPart trunk = new BodyPart();
        trunk.primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        //Reparent the trunk and move the relative height so it starts at 0 and grows up...
        trunk.primitive.transform.parent = root.transform;
    }
    public void GenerateBranches(){

    }
    public void GenerateLeaves(){
        
    }

}