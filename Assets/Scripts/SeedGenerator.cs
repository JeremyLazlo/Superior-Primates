using UnityEngine;

[CreateAssetMenu(fileName = "SeedGenerator", menuName = "Scriptable Objects/SeedGenerator")]
public class SeedGenerator : ScriptableObject
{
    [SerializeField] private int seed = 4095;   //default seed value (can be changed via serialize field)

    void setSeed(int newSeed)
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
        Debug.Log("Seed = " + seed);
        return seed;
    }
}
