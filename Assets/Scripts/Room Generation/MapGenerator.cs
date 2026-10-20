using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private LevelPlacer levelGen;
    public SeedGenerator seedGen;
    private int seed;

    // Generate seed and place levels
    void Start()
    {
        levelGen = this.GetComponent<LevelPlacer>();
        seed = seedGen.generateNewSeed();
        GenerateLevels(4);
        //spawnAtStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateLevels(int numLevels)
    {
        Debug.Log("Using seed " + seed);
        levelGen.GenerateLevels(numLevels, seed);
    }

    void spawnAtStart()
    {
        GameObject player = GameObject.Find("Player");
        player.transform.position = levelGen.getSpawnLocation() + new Vector3(0, 1.75f, 0);
    }
}
