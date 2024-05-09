using System;
using System.Collections.Generic;
using UnityEngine;

public class TreeObject : CreatureObject{

    public BodyPart trunk;
    public List<BodyPart> branches;

    public TreeObject(){
        
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
        root = new GameObject(name);
    }
    //Constructs a trunk bodypart
    public static float MIN_HEIGHT = 0.5f;
    public static float MAX_HEIGHT = 1.5f;
    public static float STEP_SIZE = 0.1f;
    public static float MIN_WIDTH = 0.2f;
    public static float MAX_WIDTH = 1f;
    public static int MIN_BRANCHES = 1;
    public static int MAX_BRANCHES = 4;
    public void GenerateTrunk(){
        //Generate the domains
        object[] heights = new object[Mathf.CeilToInt((MAX_HEIGHT - MIN_HEIGHT) / STEP_SIZE) + 1];
        for (int i = 0; i < heights.Length; i++) {heights[i] = MIN_HEIGHT + (STEP_SIZE * i);}
        object[] widths = new object[Mathf.CeilToInt((MAX_WIDTH - MIN_WIDTH) / STEP_SIZE) + 1];
        for (int i = 0; i < widths.Length; i++) {widths[i] = MIN_WIDTH + (STEP_SIZE * i);}
        object[] branches = new object[1 + MAX_BRANCHES - MIN_BRANCHES];
        for (int i = 0; i < branches.Length; i++) {branches[i] = MIN_BRANCHES + i;}
        //Create the CSP
        var vars = new Variable[]{new Variable("height", heights), new Variable("width", widths),new Variable("branches", branches)};
        var cons = new Constraint[]{};
        Graph trunkLayer = new Graph(vars, cons, this);
        trunkLayer.BacktrackingSolve();
        //Translate the solution into an output
        Joint rootJoint = new Joint("Root");
        BodyPart trunk = new BodyPart();
        rootJoint.limb = trunk;
        trunk.primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.primitive.GetComponent<MeshRenderer>().material.color = new Color(0.347f, 0.165f, 0.165f);
        //Reparent the trunk and move the relative height so it starts at 0 and grows up...
        trunk.primitive.transform.parent = root.transform;
        float height = (float)vars[0].GetValue();
        float width = (float)vars[1].GetValue();
        int branch = (int)vars[2].GetValue();

        trunk.primitive.transform.localPosition = new Vector3(0, height, 0);
        trunk.primitive.transform.localScale = new Vector3(width / 2f, height, width / 2f);
        System.Random rand = new System.Random();
        for(int i =0; i < branch; i++){
            //Generate a joint for each branch and add it to the trunk at a spot somehwere along the circumfrence at a height above halfway with orientation pointing away (and maybe up?)
            Joint joint = new Joint("Branch " + i, trunk);
            Vector3 circumfrence = CalculateCirclePoint(width/2f, (float)rand.Next(0, 100) * 0.02f * (float)Math.PI);
            joint.root = circumfrence + new Vector3(0, height * (((float)rand.Next(5, 10)) /10f), 0);
            joint.tail = new Vector3(0,0,0);
            joint.orientation = Quaternion.LookRotation(circumfrence + new Vector3(0, height * (((float)rand.Next(-1, 4)) /10f), 0), Vector3.up);
            trunk.joints.Add(joint,  null);
        }
    }
    public static float MIN_LENGTH = 0.5f;
    public static float MAX_LENGTH = 1.5f;
    public static float MIN_RADIUS = 0.1f;
    public static float MAX_RADIUS = 0.4f;
    public void GenerateBranches(){
        object[] lengths = new object[Mathf.CeilToInt((MAX_LENGTH - MIN_LENGTH) / STEP_SIZE) + 1];
        for (int i = 0; i < lengths.Length; i++) {lengths[i] = MIN_LENGTH + (STEP_SIZE * i);}
        object[] radii = new object[Mathf.CeilToInt((MAX_RADIUS - MIN_RADIUS) / STEP_SIZE) + 1];
        for (int i = 0; i < radii.Length; i++) {radii[i] = MIN_RADIUS + (STEP_SIZE * i);}
        //Create the CSP
        var vars = new Variable[]{new Variable("lengths", lengths), new Variable("widths", radii)};
        var cons = new Constraint[]{};
        Graph branchLayer = new Graph(vars, cons, this);

        foreach(Joint joint in trunk.joints.Keys){        
            branchLayer.BacktrackingSolve();
            BodyPart branch = new BodyPart();
            branch.primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            branch.primitive.GetComponent<MeshRenderer>().material.color = new Color(0.347f, 0.165f, 0.165f);
            //Reparent the trunk and move the relative height so it starts at 0 and grows up...
            branch.primitive.transform.parent = root.transform;
            float height = (float)vars[0].GetValue();
            float width = (float)vars[1].GetValue();
        }
    }
    public void GenerateLeaves(){
        
    }
    public Vector3 CalculateCirclePoint(float radius, float angleRadians) {

        // Calculate the x and y coordinates of the point
        float x = radius * Mathf.Cos(angleRadians);
        float z = radius * Mathf.Sin(angleRadians);

        // Return the Vector3 point
        return new Vector3(x, 0f, z);
    }

}