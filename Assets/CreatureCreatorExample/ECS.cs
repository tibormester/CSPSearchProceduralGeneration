using System;
using System.Collections.Generic;
using System.Linq;

public static class ECS {
    /**
        In a proper application it may be ideal to create an ECS system to manage objects and entities especially at scale
        However, for the demo, we will simply treat entities as objects that have components to avoid the overhead of implementing the ECS architecture
        Typically, we would have entities stored in tables with the ability to quickly find all entities with component of type T, 
            enabling the implemention of systems on the relevant components, 
            avoiding iterating through each entity during a game loop
    **/
}
public class Entity{
    public List<Component> Components { get; } = new List<Component>();
}
public abstract class Component {
    public Entity Entity { get; set; }
}

// Two wrapper classes to handle quantatative and qualitative data:
/**
    Sample given a min and a max value in a range as well as a sampling function can sample a random value in the range

    Subset given a list or similar collection and a min and max length, randomly sample a subset of the list with sufficient length
**/
public class Sample<T> where T : struct, IComparable<T>{
    private T? value;
    public T Value {
        get {
            if (value.HasValue) { return value.Value;} 
            else {return samplingFunction(min, max);}
        }
        set{ this.value = value;}}
    private T min;
    private T max;
    private Func<T, T, T> samplingFunction;

    public Sample(T? value = null, T? min = null, T? max = null, Func<T, T, T> samplingFunction = null) {
        if(value != null)this.value = value;
        if(min != null)this.min = (T)min;
        if(max != null)this.max = (T)max;
        this.samplingFunction = samplingFunction ?? DefaultSamplingFunction;
    }
    public Sample(T min, T max, Func<T, T, T> samplingFunction = null) {
        this.min = min;
        this.max = max;
        this.samplingFunction = samplingFunction ?? DefaultSamplingFunction;
    }

    public T sample() {
        value = samplingFunction(min, max);
        return (T)Value;
    }

    private T DefaultSamplingFunction(T min, T max) {
        // Uniform sampling
        Random random = new Random();
        if (typeof(T) == typeof(int)) {
            int minValue = (int)(object)min;
            int maxValue = (int)(object)max;
            return (T)(object)random.Next(minValue, maxValue + 1);
        } else if (typeof(T) == typeof(double)) {
            double minValue = (double)(object)min;
            double maxValue = (double)(object)max;
            return (T)(object)(random.NextDouble() * (maxValue - minValue) + minValue);
        } else if (typeof(T) == typeof(float)) {
            float minValue = (float)(object)min;
            float maxValue = (float)(object)max;
            return (T)(object)((float)random.NextDouble() * (maxValue - minValue) + minValue);
        }
        // Handle other types if needed
        throw new InvalidOperationException("Unsupported type for sampling.");
    }
}

public class SubSet<T> {
    public List<T> set {
        get {if (set == null){
            set = samplingFunction(collection, minLength, maxLength).ToList();
            return set;
        } else{
            return set;
        }}
        set {set = value;}
    }
    private IList<T> collection;
    private int minLength;
    private int maxLength;
    private bool replacement;
    private Func<IList<T>, int, int, IEnumerable<T>> samplingFunction;

    public SubSet(IList<T> collection, int minLength = 1, int maxLength = 1, bool replacement = false,
                                 Func<IList<T>, int, int, IEnumerable<T>> samplingFunction = null) {
        this.collection = collection;
        this.minLength = minLength;
        this.maxLength = maxLength;
        this.replacement = replacement;
        this.samplingFunction = samplingFunction ?? DefaultSamplingFunction;
    }
    public SubSet(List<T> set, IList<T> collection = null,int minLength = 1, int maxLength = 1, bool replacement = false,
                                 Func<IList<T>, int, int, IEnumerable<T>> samplingFunction = null) {
        this.set = set;
        this.collection = collection;
        this.minLength = minLength;
        this.maxLength = maxLength;
        this.replacement = replacement;
        this.samplingFunction = samplingFunction ?? DefaultSamplingFunction;
    }

    public IEnumerable<T> Sample() {
        set = samplingFunction(collection, minLength, maxLength).ToList();
        return set.AsEnumerable();
    }

    private IEnumerable<T> DefaultSamplingFunction(IList<T> collection, int minLength, int maxLength) {
        Random random = new Random();
        int length = random.Next(minLength, maxLength);
        if (replacement){
            List<T> results = new List<T>();
            while (length > 0){
                results.Add(collection[random.Next(0,collection.Count)]);
            }
            return results.AsEnumerable();
        }else {
            return collection.OrderBy(_ => random.Next()).Take(length);
        }
    }
}
