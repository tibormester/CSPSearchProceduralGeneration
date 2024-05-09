using UnityEngine;
using System;
using System.Collections.Generic;

// Define a base class for components of the organ represnting data used for determing the organ's functions

public class Organ : Entity{
    public string name;
    public Creature creature;
    public BodyNode parent;
}
// Define a component representing a limb
public class PhysicalComponent : Component{

    private GameObject obj;
    public Sample<float> size = new Sample<float>(0.5f, 2f);
    public Sample<int> shape = new Sample<int>(0, 5);
    public GameObject drawOrgan(){
        if (obj == null){
            obj = GameObject.CreatePrimitive((PrimitiveType)shape.Value);
        }return obj;
    }    
    public Sample<int> r = new Sample<int>(0, 255);
    public Sample<int> b = new Sample<int>(0, 255);
    public Sample<int> g = new Sample<int>(0, 255);
    public Color Color(){
        return new Color(r.Value, b.Value, g.Value);
    }
}
public class LimbComponent : Component {
    public float Length { get; set; }
    public int NumberOfSegments { get; set; }
    // Add more properties as needed
}

// Define a component representing sensory abilities
public class SensoryComponent : Component {
    public float SightRange { get; set; }
    public float HearingRange { get; set; }
    // Add more properties as needed
}

// Define a component representing offensive capabilities
public class OffensiveComponent : Component {
    public float AttackPower { get; set; }
    public float AttackRange { get; set; }
    // Add more properties as needed
}

// Define a component representing defensive capabilities
public class DefensiveComponent : Component {
    public float Armor { get; set; }
    // Add more properties as needed
}

// Define a component representing metabolic functions
public class MetabolicComponent : Component {
    public float DigestiveEfficiency { get; set; }
    // Add more properties as needed
}

public class Program {
    public static void Main(string[] args) {
        // Create an entity representing a creature
        Entity creature = new Entity();

        // Add various components to the creature
        creature.Components.Add(new LimbComponent {
            Length = 1.2f,
            NumberOfSegments = 3
        });

        creature.Components.Add(new SensoryComponent {
            SightRange = 20.0f,
            HearingRange = 15.0f
        });

        creature.Components.Add(new OffensiveComponent {
            AttackPower = 10.0f,
            AttackRange = 2.0f
        });

        creature.Components.Add(new DefensiveComponent {
            Armor = 5.0f
        });

        creature.Components.Add(new MetabolicComponent {
            DigestiveEfficiency = 0.8f
        });

        // Access and use components as needed
        foreach (var component in creature.Components) {
            if (component is LimbComponent limb) {
                Console.WriteLine($"Limb: {((Organ)component.Entity).name}, Length: {limb.Length}, Segments: {limb.NumberOfSegments}");
            } else if (component is SensoryComponent sensory) {
                Console.WriteLine($"Sensory Abilities: Sight Range: {sensory.SightRange}, Hearing Range: {sensory.HearingRange}");
            } else if (component is OffensiveComponent offensive) {
                Console.WriteLine($"Offensive Capabilities: Attack Power: {offensive.AttackPower}, Attack Range: {offensive.AttackRange}");
            } else if (component is DefensiveComponent defensive) {
                Console.WriteLine($"Defensive Capabilities: Armor: {defensive.Armor}");
            } else if (component is MetabolicComponent metabolic) {
                Console.WriteLine($"Metabolic Functions: Digestive Efficiency: {metabolic.DigestiveEfficiency}");
            }
            // Add more conditions for other component types as needed
        }
    }
}

/**
basilisk

tree hierarchy from brain out? physical root..?

Torso Organ : Movement Component (slither) : Limb Component (many segments?)....
Fang Organ : Offensive Compoenent (Touch, penetration, venom...)
etc....

**/