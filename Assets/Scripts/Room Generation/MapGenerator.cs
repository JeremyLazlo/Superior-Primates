using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;

//Responsible for generating the entire map
public class MapGenerator : MonoBehaviour
{
    public int numberOfLevelsGenerated = 4;
    public SeedGenerator seedGen;
    private LevelPlacer levelGen;
    private int seed;
    //private Queue<GameObject> levelsPlaced = new Queue<GameObject>();

    //Get number of levels to generate
    void Start()
    {
        
        if(numberOfLevelsGenerated <= 0)
        {
            //Invalid input for numberOfLevelsGenerated; use default value
            Debug.LogWarning("Invalid number of levels to be generated, using 4");
            numberOfLevelsGenerated = 4;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
 
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //Generate levels and spawn player at start
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        seedGen.setSeed(4095);
        seed = seedGen.getSeed();
        //seedGen.generateNewSeed();
        levelGen = this.GetComponent<LevelPlacer>();
        GenerateLevels(numberOfLevelsGenerated);
        spawnAtStart(); //teleport player to start
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
        CharacterController controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.transform.position = levelGen.getSpawnLocation() + new Vector3(0, 1.75f, 0);
        controller.enabled = true;
    }
}
