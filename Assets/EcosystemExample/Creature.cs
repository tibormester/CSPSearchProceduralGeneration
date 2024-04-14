using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CreatureObject : ProceduralObject{
    public string name;
    public GameObject root;
    public List<BodyPart> bodyParts;

}
public class BodyPart{
    //Describes the bodypart
    public string name;
    public string[] tags;
    //Gives the location of the body part's relative to the previous joint or creature in the frame of the mesh
    public Vector3 root;
    public CreatureObject creature;
    //Joint objects describe the physical position of attached bodyparts
    public Dictionary<Joint, BodyPart> joints;
    public Color color;
    public GameObject primitive;

    public bool attach(Joint joint, BodyPart part){
        if(joint.valid(part)){

            return true;
        } else return false;
    }
}
public class Joint{
    //Something to describe the joint, name + filters for tags, checks blacklist first and if a list is null the filter isnt applied
    string name;
    string[] whitelist;
    string[] blacklist;
    BodyPart body;
    BodyPart limb;
    //Where on the parent object to act locally from
    Vector3 root = Vector3.zero;
    //Where on the child object to act on
    Vector3 tail = Vector3.zero;
    //How should the tail be rotated relative to the root orienation
    Quaternion orientation = Quaternion.identity;
    public bool valid(BodyPart part){
        if(part == null)return false;
        if (blacklist != null && blacklist.Length > 0) foreach(string tag in part.tags){
            if (blacklist.Contains(tag)) return false;
        }
        if(whitelist == null) return true;
        foreach(string tag in part.tags) if(whitelist.Contains(tag))return true;
        return false;
    }
}

