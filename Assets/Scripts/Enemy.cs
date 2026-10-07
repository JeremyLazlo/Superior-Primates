using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    // ===== EXISTING FIELDS (health/death system) =====
    public EnemyManager enemyManager; // Reference to the EnemyManager script, used to remove this enemy from the tracked list when it dies
    private float enemyHealth = 2f;   // Enemy's current health, starts at 2

    [Header("References")]
    private NavMeshAgent agent;   // Unity's built-in pathfinding component - handles moving the enemy around obstacles toward a destination
    private Transform player;     // Cached reference to the player's Transform, found once at Start() using the "Player" tag

    // Detection Settings
    [Header("Detection")]
    public float detectionRange = 10f;  // How close the player needs to be before the enemy notices them and starts chasing (in Idle state)
    public float attackRange = 2f;      // How close the enemy needs to be to the player before it stops chasing and starts attacking
    public float loseTargetRange = 15f; // If the player gets further than this while being chased, the enemy gives up and goes back to Idle

    // Attack Settings
    [Header("Attack")]
    public float attackDamage = 1f;     // How much damage the enemy deals per attack
    public float attackCooldown = 1.5f; // Minimum time (in seconds) between attacks, so the enemy doesn't hit every single frame
    private float lastAttackTime = -999f; // Tracks the last time an attack happened, used to enforce the cooldown above

    // Patrol Settings for different enemy type 
    [Header("Patrol (optional)")]
    public Transform[] patrolPoints;     // Optional list of waypoints the enemy walks between while Idle. Leave empty if you just want the enemy to stand still until it spots the player
    private int currentPatrolIndex = 0;  // Tracks which patrol point the enemy is currently walking toward

    // States of Enemies
    // Defines the three possible states the enemy can be in
    // Idle   = standing still or patrolling, hasn't noticed the player yet
    // Chase  = has spotted the player and is actively moving toward them
    // Attack = close enough to the player to stop moving and deal damage
    private enum State { Idle, Chase, Attack }
    private State currentState = State.Idle; // Enemy always starts in the Idle state when the scene begins

    void Start()
    {
        // Grab the NavMeshAgent component attached to this same GameObject.
        // Moves the enemy along the baked NavMesh
        agent = GetComponent<NavMeshAgent>();

        // Find the player in the scene by looking for the GameObject tagged "Player".
        // Only runs once at the start rather than every frame, since the player object doesn't change.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // If this enemy has patrol points assigned in the Inspector, start walking toward the first one
        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[0].position);
        }
    }

    void Update()
    {
        // Death
        // If health has hit zero (or below), remove this enemy from the manager's list and destroy the GameObject.
        // The "return" after this stops the rest of Update() from running once the enemy is already dead/destroyed.
        if (enemyHealth <= 0)
        {
            enemyManager.RemoveEnemy(this);
            Destroy(gameObject);
            return;
        }

        // Safety check: if we never found a player (e.g. player object missing from scene), don't try to do anything else.
        if (player == null) return;

        // Calculate how far away the player currently is from this enemy.
        // This is recalculated every frame since both the player and enemy can be moving.
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Enemy State Logic
        // Depending on which state we're currently in, run different behavior.
        switch (currentState)
        {
            case State.Idle:
                // While idle, just patrol back and forth (if patrol points exist).
                Patrol();

                // Check if the player has entered detection range - if so, switch to Chase.
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                // Continuously update the NavMeshAgent's destination to the player's current position, so the enemy keeps pathfinding toward them even as they move.
                agent.SetDestination(player.position);

                // If enemy got close enough, switch to Attack state.
                if (distanceToPlayer <= attackRange)
                {
                    currentState = State.Attack;
                }
                // If the player gets far away again (ran away), give up and go back to Idle.
                else if (distanceToPlayer > loseTargetRange)
                {
                    currentState = State.Idle;
                }
                break;

            case State.Attack:
                // Stop moving by setting the destination to the enemy's own current position.
                agent.SetDestination(transform.position);

                // Rotate the enemy to face the player smoothly (visual)
                Vector3 dir = (player.position - transform.position).normalized;
                dir.y = 0; // Ignore vertical difference so the enemy doesn't tilt up/down while rotating
                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
                }

                // Only attack if enough time has passed since the last attack (depends on attackCooldown).
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    AttackPlayer();
                    lastAttackTime = Time.time;
                }

                // If the player moved back out of attack range, go back to chasing them.
                if (distanceToPlayer > attackRange)
                {
                    currentState = State.Chase;
                }
                break;
        }
    }

    // Handles patrol movement while in the Idle state.
    void Patrol()
    {
        if (patrolPoints.Length == 0) return; // No patrol points assigned, so there's nothing to do

        // Check if the agent has basically arrived at its current destination (within 0.5 units)
        // and isn't still calculating a path (pathPending).
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Move to the next patrol point in the list, wrapping back to 0 after the last one.
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    // Called when the enemy is close enough and off cooldown - this is where actual damage to the player happens.
    void AttackPlayer()
    {
        // Temp line -> logs to console for now.
        // TODO: Replace this with a call into the player's health/damage script once built
        Debug.Log("Enemy attacks player for " + attackDamage + " damage!");

        // player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
    }

    // Called by other scripts (e.g. a weapon/bullet script) to reduce this enemy's health when hit.
    public void TakeDamage(float damage)
    {
        enemyHealth -= damage;
    }
}