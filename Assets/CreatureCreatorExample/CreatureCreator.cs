using System;
using UnityEngine;
/**
    Main Class for the demo, 
    1. Create instances of template organs, resulting in the organ domain
    2. Select creature constraints
    3. Solve creature csp
    ....
    1. Generate set of creatures
    2. Select ecosystem constraints
    3. solve ecosystem csp...?
**/
public class CreatureCreator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //var creature = new Creature();
        
        // Create a ProbabilityInt with a default sampling function
        Sample<int> probInt1 = new Sample<int>(0, 10);
        Debug.Log("Sample 1: " + probInt1.sample());

        // Create a ProbabilityInt with a custom sampling function
        Sample<int> probInt2 = new Sample<int>(0, 10, samplingFunction: Midpoint);
        Debug.Log("Sample 2: " + probInt2.sample());
    }

    private int Midpoint(int min, int max)
    {
        return (max + min) / 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
