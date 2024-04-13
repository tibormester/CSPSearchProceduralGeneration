using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeciesObject : ProceduralObject
{
    /**
    Given a species from the ecosystem object, construct an example... 
    
    a Species has a bodytype - basically a graph of limbs and organs describing their layout
    a species has traits like: move speed....
    **/
    //Stuff for the species object to decide
    public object[] bodySize = new object[4]{0.8f, 1f, 1.2f, 2f};
    public object[] camouflage = new string[3]{"spots", "stripes", "dark"};
    public object[] limbs = new string[7]{"Head", "Arm", "Claw", "Foot", "Hand", "Leg", "Abdomen"};
    public object[] teethShapes = new string[2]{"Triangle", "Square"};
}
