using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth;
    private int health;

    public int maxArmor;
    private int armor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
        armor = maxArmor; //for test puposes bc player will start with 0
    }

    // Update is called once per frame
    void Update()
    {
        // temporary test func
        if (Keyboard.current.rightShiftKey.wasPressedThisFrame)
        {
            DamagePlayer(30);
            Debug.Log("Player took damage!");
        }
    }

    public void DamagePlayer(int damage)
    {
        // if player har armor, damage armor first
        if(armor > 0)
        {
            if(armor >= damage)
            {
                armor -= damage;
            }
            else if(armor < damage)
            {
                int remainingDamage = damage - armor;
                armor = 0;
                health -= remainingDamage;
            }
        }
        else
        {
            health -= damage;
        }

        // Check if player is dead
        if (health <= 0)
        {
            Debug.Log("Player is dead!");
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }
    }
}
