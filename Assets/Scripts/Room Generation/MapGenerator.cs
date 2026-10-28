using UnityEngine;
using System.Collections.Generic;

//Responsible for generating the entire map
public class MapGenerator : MonoBehaviour
{
    public int numberOfLevelsGenerated = 4;
    public SeedGenerator seedGen;
    private LevelPlacer levelGen;
    private int seed;
    //private Queue<GameObject> levelsPlaced = new Queue<GameObject>();

    // Generate seed and place levels
    void Start()
    {
        levelGen = this.GetComponent<LevelPlacer>();
        seedGen.setSeed(4095);
        seed = seedGen.getSeed();
        //seedGen.generateNewSeed();
        if(numberOfLevelsGenerated <= 0)
        {
            //Invalid input for numberOfLevelsGenerated; use default value
            numberOfLevelsGenerated = 4;
        }
        GenerateLevels(numberOfLevelsGenerated);
        spawnAtStart(); //teleport player to start
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateLevels(int numLevels)
    {
        //places levels in world
        Debug.Log("Using seed " + seed);
        Random.InitState(seed);
        int levelIndex = 0; //index for levelList
        Vector3 endPos = levelGen.spawnStart();  //spawn intro room
        Debug.Log(endPos);
        for(int i=0; i<numLevels; i++)
        {
            //Choose random level in levelList
            levelIndex = Random.Range(0, levelGen.getNumLevels());
            Debug.Log("Level index " + levelIndex);
            endPos = levelGen.spawnLevel(levelIndex, endPos);
        }
        
        Debug.Log(endPos);
    }

    //teleports player to starting point of Intro Rooms
    void spawnAtStart()
    {
        GameObject player = GameObject.Find("Player");
        player.transform.position = levelGen.getSpawnLocation() + new Vector3(0, 1.75f, 0);
    }
}
