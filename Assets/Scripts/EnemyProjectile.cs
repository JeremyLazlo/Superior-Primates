using UnityEngine;

// the projectile a ranged enemy fires, flies in a straight line and damages the player on contact
public class EnemyProjectile : MonoBehaviour
{
    public float speed = 12f;   // units per second, right now its slow enough that the player can dodge it
    public float lifetime = 5f; // goes away on its own if it never hits anything
    public float radius = 0.15f; // how big the projectile is for hit detection

    private Vector3 direction;
    private int damage;

    // called by the enemy right after spawning it
    public void Launch(Vector3 shotDirection, int shotDamage)
    {
        direction = shotDirection.normalized;
        damage = shotDamage;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (direction == Vector3.zero) return;

        float step = speed * Time.deltaTime;

        // check the path we are about to move along so fast shots can't skip through thin walls
        if (Physics.SphereCast(transform.position, radius, direction, out RaycastHit hit, step, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.GetComponentInParent<Enemy>() == null) // projectiles pass through other enemies
            {
                PlayerHealth health = hit.collider.GetComponentInParent<PlayerHealth>();

                if (health != null)
                {
                    health.DamagePlayer(damage);
                    Debug.Log("Projectile hit player for " + damage + " damage!"); // Prints message to console when the projectile hits the player
                }

                Destroy(gameObject); // hit the player or a wall -> projectile is destroyed
                return;
            }
        }

        transform.position += direction * step;
    }
}