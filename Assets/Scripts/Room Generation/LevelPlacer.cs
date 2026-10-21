using System.Collections.Generic;
using UnityEngine;

public class LevelPlacer : MonoBehaviour
{
    [SerializeField] private GameObject[] levelList;
    [SerializeField] private GameObject startingLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //add default values in case of null
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //gets starting position for player to spawn
    public Vector3 getSpawnLocation()
    {
        Vector3 startPos = this.transform.position;
        Transform findSpawnPos = startingLevel.transform.GetChild(0); //start position at (13, 0, 2)
        return startPos + findSpawnPos.position;
    }

    //get number of levels in levelList
    public int getNumLevels()
    {
        return levelList.Length;
    }

    //loads intro area level
    //returns vector of end position of newly generated map
    public Vector3 spawnStart()
    {
        Vector3 startPos = this.transform.position;
        Quaternion levelRot = Quaternion.identity;
        Instantiate(startingLevel, startPos, levelRot);
        //Transform findStartPos = startingLevel.transform.GetChild(0); //start position at (13, 0, 2)
        Transform findEndPos = startingLevel.transform.GetChild(1); //end position of starting level is (35, 0, 44)
        return startPos + findEndPos.position;
    }

    //loads all other levels
    //inputs: number of next level, end position of currently generated map
    //returns vector of end position of newly generated map
    public Vector3 spawnLevel(int levelNum, Vector3 currentEndPos)
    {
        Quaternion levelRot = Quaternion.identity;
        GameObject newLevel = levelList[levelNum];
        Transform findStartPos = newLevel.transform.GetChild(0);
        Transform findEndPos = newLevel.transform.GetChild(1);
        Vector3 newLevelShift = -(findStartPos.position);

        Instantiate(newLevel, (currentEndPos + newLevelShift), levelRot);
        return currentEndPos + newLevelShift + findEndPos.position;
        //add error check to make sure level has a start spot
        //generation spot = end pos of previous level - start spot of next level
    }
}
