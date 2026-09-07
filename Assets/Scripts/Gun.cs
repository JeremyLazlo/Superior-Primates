using UnityEngine;
using UnityEngine.InputSystem; // Import the InputSystem namespace to handle input from the mouse

public class Gun : MonoBehaviour
{
    public float range = 20f;
    public float verticalRange = 5f;
    public float fireRate = 1f;
    public float damage = 2f;
    
    private float nextTimeToFire;
    private BoxCollider gunTrigger;

    public LayerMask raycastLayerMask; // Layer mask to filter the raycast to only hit enemies
    public EnemyManager enemyManager; // Reference to the EnemyManager script

    void Start()
    {
        gunTrigger = GetComponent<BoxCollider>(); // Get the BoxCollider component attached to the gun
        gunTrigger.size = new Vector3(1, verticalRange, range); // Set the size of the BoxCollider based on the range and verticalRange values
        gunTrigger.center = new Vector3(0, 0, range * 0.5f); // Set the center of the BoxCollider to be at half the verticalRange and half the range
    }

    void Update()
    {
        if (Mouse.current == null) // Check if the mouse is available
            return;

        if (Mouse.current.leftButton.isPressed && Time.time >= nextTimeToFire) // Check if the left mouse button is pressed and if the current time is greater than or equal to the next time to fire
        {
            Fire();
        }
    }

    void Fire()
    {
        //play ShotGun sound effect
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().Play();

        foreach (var enemy in enemyManager.enemiesInTrigger) // Loop through all enemies in the list from EnemyManager
        {
            var dir = enemy.transform.position - transform.position; // Calculate the direction from the gun to the enemy
            
            RaycastHit hit;
            if(Physics.Raycast(transform.position, dir, out hit, range * 1.5f, raycastLayerMask))
            {
                if(hit.transform == enemy.transform) // Check if the raycast hit the enemy
                {
                    // Apply damage to the enemy
                    enemy.TakeDamage(damage); // Call the TakeDamage method on the enemy, applying the specified damage

                    // Draw a debug ray for visualization
                    Debug.DrawRay(transform.position, dir, Color.red, 1f);
                }
            }

        }

        nextTimeToFire = Time.time + fireRate; // Update the next time to fire based on the fire rate
    }

    private void OnTriggerEnter(Collider other) // Called when another collider enters the trigger collider attached to the gun
    {
        Enemy enemy = other.transform.GetComponent<Enemy>();

        if (enemy)
        {
            enemyManager.AddEnemy(enemy); // Add the enemy to the list in EnemyManager
        }
    }

    private void OnTriggerExit(Collider other) // Called when another collider exits the trigger collider attached to the gun
    {
        Enemy enemy = other.transform.GetComponent<Enemy>();
        if (enemy)
        {
            enemyManager.RemoveEnemy(enemy); // Remove the enemy from the list in EnemyManager
        }
    }
}