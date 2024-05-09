using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature 
{
    public Creature(Constraint[] constraints, Organ[] organs){
        
    }
/**
    I want creatures to be a list of organs all connected together in a graph structure

    Spine: chain of Points to points with thickness, all limbs are relative to a spine
    Limbs: Similar to spine but more unique shape (still point to points but legs might be 3 points with restriction on angle between them)
    Organs: Similar to limbs, except justa single point with a unique shape and likely a specifc function...

    Manipulation Limbs: Head, Arms, Tails
    Manipulation Organs: Mouth, Hands, Tentacles??
    Movement Limbs: Legs, Wings, None (slither / roll??)
    Senses: Ears, Nose, Eyes...

    Basically CSPs are good for configuration problems with discrete states producing a manageable number of state spaces
    Something that considers all infinite orientations or locations isn't well suited since there are too many options

    Use cases of CSPs for procedural generation is in filling rooms with furniture where furniture may be constrained such that they are useable

**/
//List of body nodes that create the spine
public BodyNode[] spine;
//List of organs relative to a node on the spine the do functionality for the creature 
public List<Organ> organs;
    
}
/**
    A node in the spine linked list
**/
public class BodyNode {
    //Creates a node in the spine linked list
    public Vector3 tip;
    public float radius;
    public BodyNode parent;
    public BodyNode(BodyNode parent = null, Vector3? tip = null, float? radius = null) {

    }
}
