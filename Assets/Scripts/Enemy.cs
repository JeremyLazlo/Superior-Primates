using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyManager enemyManager; // Reference to the EnemyManager script
    private float enemyHealth = 2f; // Enemy's health
    void Start()
    {
        
    }

    void Update()
    {
        if(enemyHealth <= 0) // Check if the enemy's health is zero or below
        {
            enemyManager.RemoveEnemy(this); // Remove the enemy from the list in EnemyManager
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage) // Method to apply damage to the enemy
    {
        enemyHealth -= damage; // Reduce the enemy's health by the damage amount
    }
}
