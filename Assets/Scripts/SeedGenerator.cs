using UnityEngine;

//See list of comments at end of file for some specific seeds
[CreateAssetMenu(fileName = "SeedGenerator", menuName = "Scriptable Objects/SeedGenerator")]
public class SeedGenerator : ScriptableObject
{
    [SerializeField] private int seed = 4095;   //default seed value (can be changed via serialize field)

    public void setSeed(int newSeed)
    {
        seed = newSeed;
    }
    public int getSeed()
    {
        return seed;
    }
    public int generateNewSeed()
    {
        seed = Random.Range(0, 1000000);
        //Debug.Log("Seed = " + seed);
        return seed;
    }
}

/*
Some Seeds
4095 (generates the three unique levels used in the beginning)
*/
