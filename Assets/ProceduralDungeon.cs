using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ProceduralDungeon : MonoBehaviour
{
    public int length = 16;
    public int width = 24;
    // Start is called before the first frame update
    public void Start(){
        System.Object[] values = LoadPrefabs();
        List<object> floors = new();
        List<object> walls = new();
        List<object> banners = new();

        List<object> others = new();
        foreach(System.Object o in values){
            Prefab fab = (Prefab)o;
            if(fab.tags.Contains("wall")){
                walls.Add(fab);
            }else if(fab.tags.Contains("banner")){
                banners.Add(fab);
            }else if(fab.tags.Contains("floor")){
                floors.Add(fab);
            }else {
                others.Add(fab);
            }
        } 

        List<Constraint> cons = new();
        Variable[] roomVars = new Variable[length*width];
        //Create a cell to fill with objects
        int index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                roomVars[index] = new Variable(x + ", 0.25," + y, others.ToArray());
                index++;
            }
        }
        Variable[] floorVars = new Variable[length*width/4];
        index = 0;
        for (int x = 0; x < width; x+= 2)
        {
            for (int y = 0; y < length; y+= 2)
            {
                floorVars[index] = new Variable(x + ", 0," + y, floors.ToArray());
                index++;
            }
        }
        Variable[] flatWalls = new Variable[(width /2)];
        Variable[] turnedWalls = new Variable[(length/2)];
        index = 0;
        for (int x = 0; x < width; x+= 4)
        {
            for (int y = 0; y < length; y += length -1){
                flatWalls[index] = new Variable(x + ", 0.1, " + y, walls.ToArray());
                index++;
            }
        }
        index = 0;
        for (int y = 0; y < length; y+= 4)
        {
            for (int x = 0; x < width; x += width -1){
                turnedWalls[index] = new Variable(x + ", 1," + y, walls.ToArray());
                index++;
            }
        }

        ProceduralObject dungeon = new ProceduralObject();
        var graph = new Graph[]{new Graph(roomVars.Concat(floorVars).Concat(flatWalls).ToArray(),cons.ToArray(), dungeon)};
        dungeon.layers = graph;
        var solution = dungeon.layers[0].BacktrackingSolve();
        foreach ((string name, object o) in solution){
                Prefab fab = (Prefab) o;
                string[] parts = name.Split(',');
                float x = float.Parse(parts[0]);
                float y = float.Parse(parts[1]);
                float z = float.Parse(parts[2]);
                if(!fab.tags.Contains("null")){
                    fab.Create().transform.position = new Vector3(x, y, z);
                }
        }
        


    }
    public System.Object[] LoadPrefabs(string pathname = "/DungeonAssets/Resources/fbx/"){
        List<System.Object> prefabs = new();
        string[] files = Directory.GetFiles(Application.dataPath + pathname, "*.fbx", SearchOption.TopDirectoryOnly);
        foreach (string file in files){
            prefabs.Add(new Prefab(Path.GetFileNameWithoutExtension(file)));
        }
        //prefabs.Add(new Prefab("wall_null"));
        //prefabs.Add(new Prefab("floor_null"));
        //prefabs.Add(new Prefab("banner_null"));
        for(int i = 0; i < 150; i++){
            prefabs.Add(new Prefab("obj_null"));
        }

        return prefabs.ToArray();
    }

    public class Prefab {
        public string name;
        public string[] tags;
        public Vector3 spacing;
        public Prefab(string filename){
            this.name = "fbx/" + filename ;
            this.tags = filename.Split('_');
            spacing = new Vector3(2f, 2f, 2f);
            if(tags.Contains("null")){
                spacing = new Vector3(0f, 0f, 0f);
            }
            else if(tags.Contains("banner")){
                spacing = new Vector3(2f, 4f, 1f);
                if(tags.Contains("tripple")){    
                    spacing = new Vector3(4f, 4f, 0.5f);
                }
            }
            else if(tags.Contains("barrel")){
                spacing = new Vector3(1f, 1f, 1f);
                if(tags.Contains("large")){    
                    spacing = new Vector3(2f, 2f, 2f);
                }
                if(tags.Contains("stack")){    
                    spacing = new Vector3(2f, 2f, 1f);
                }
            }
            else if(tags.Contains("barrier")){
                spacing = new Vector3(4f, 1f, 1f);
                if(tags.Contains("half")){    
                    spacing = new Vector3(2f, 1f, 1f);
                }
                if(tags.Contains("corner")){    
                    spacing = new Vector3(2f, 1f, 2f);
                }
            }
            else if(tags.Contains("bed")){
                spacing = new Vector3(2f, 1f, 4f);
                if(tags.Contains("decorated")){    
                    spacing = new Vector3(3f, 1f, 4f);
                }
            }
            else if(tags.Contains("bottle")){
                spacing = new Vector3(1f, 1f, 1f);
            }
            else if(tags.Contains("box")){
                spacing = new Vector3(1f, 1f, 1f);
                if(tags.Contains("large")){    
                    spacing = new Vector3(2f, 2f, 2f);
                }
                if(tags.Contains("stacked")){    
                    spacing = new Vector3(4f, 3f, 4f);
                }
            }
            else if(tags.Contains("candle") || tags.Contains("chair") || tags.Contains("coin")|| tags.Contains("column")|| tags.Contains("plate")|| tags.Contains("key")|| tags.Contains("keyring")|| tags.Contains("trunk")){
                spacing = new Vector3(1f, 1f, 1f);
            }
            else if(tags.Contains("chest")){
                spacing = new Vector3(2f, 1f, 1f);
            }
            else if(tags.Contains("crates") || tags.Contains("keg")){
                spacing = new Vector3(2f, 2f, 2f);
                if(tags.Contains("stacked")){    
                    spacing = new Vector3(2f, 2f, 2f);
                }
            }
            else if(tags.Contains("foundation")){
                spacing = new Vector3(2f, 2f, 2f);
            }
            else if(tags.Contains("floor")){
                spacing = new Vector3(2f, 1f, 2f);
                if(tags.Contains("small")){    
                    spacing = new Vector3(2f, 1f, 2f);
                }if(tags.Contains("extralarge")){
                    spacing = new Vector3(8f, 1f, 8f);
                }
            }else if(tags.Contains("Wall")){
                spacing = new Vector3(4f, 4f, 1f);
                if(tags.Contains("doorway")){    
                    spacing = new Vector3(4f, 4f, 4f);
                }
                if(tags.Contains("corner")){    
                    spacing = new Vector3(2f, 4f, 2f);
                }
            }else if(tags.Contains("barrier")){
                spacing = new Vector3(4f, 1f, 1f);
                if(tags.Contains("half")){    
                    spacing = new Vector3(2f, 1f, 1f);
                }
                if(tags.Contains("corner")){    
                    spacing = new Vector3(2f, 1f, 2f);
                }
            }
        }
        public GameObject obj;
        public GameObject Create(){
            GameObject prefab = Instantiate(Resources.Load<GameObject>(name));
            if (prefab != null){
                Renderer renderer = prefab.GetComponent<Renderer>();
                if (renderer != null){
                    renderer.material = Resources.Load<Material>("Texture/Materials/dungeon_texture");
                }
            } else {
                Debug.Log("Couldn't load the prefab: " + name);
            }
            obj = prefab;
            return prefab;
        } 
    }


    //Floor constraint
    //Wall Constraint
    //No overlap constraint




    // Update is called once per frame
    void Update()
    {
        
    }
}
