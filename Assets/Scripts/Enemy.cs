using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public EnemyManager enemyManager; // Reference to the EnemyManager script
    private float enemyHealth = 2f; // Enemy's health

    [Header("References")]
    private NavMeshAgent agent;
    private Transform player; // grabbed once in Start, no need to look it up every frame
    private PlayerHealth playerHealth; // lives on the same object as the Player tag
    private Vector3 spawnPosition; // remembered so enemies with no patrol points can walk back

    [Header("Detection")]
    public float detectionRange = 10f;  // how close player has to get before we notice them
    public float attackRange = 2f;      // how close before we stop and swing
    public float loseTargetRange = 15f; // if player gets this far away mid chase we give up (a little more than detectionRange)

    [Header("Speed")]
    public float patrolSpeed = 1.5f; // slow walk while we haven't noticed the player
    public float chaseSpeed = 2.5f;  // speed once we're going after them

    [Header("Attack")]
    public int attackDamage = 10; // PlayerHealth.DamagePlayer takes an int
    public float attackCooldown = 1.5f; // seconds between hits so we're not attacking every frame
    private float lastAttackTime = -999f;

    [Header("Patrol (optional)")]
    public Transform[] patrolPoints; // leave empty if this enemy should just stand still until it spots you
    private int currentPatrolIndex = 0;

    // simple state machine for enemy behavior
    // idle = patrolling or just standing around
    // chase = we see the player and we're going after them
    // attack = close enough to actually hit them
    private enum State { Idle, Chase, Attack }
    private State currentState = State.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed; // start out slow, speed up once we spot the player (chase mode)
        spawnPosition = transform.position; // remember where we started

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>(); // grab it once here too
        }

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[0].position);
        }
    }

    void Update()
    {
        if(enemyHealth <= 0) // Check if the enemy's health is zero or below
        {
            if (enemyManager != null) enemyManager.RemoveEnemy(this); // Remove the enemy from the list in EnemyManager
            Destroy(gameObject);
            return;
        }

        if (player == null) return; // bail if we never found the player for some reason

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                Patrol();

                if (distanceToPlayer <= detectionRange)
                {
                    currentState = State.Chase;
                    agent.speed = chaseSpeed; // Initiate the chase speed (faster than patrol)
                }
                break;

            case State.Chase:
                agent.SetDestination(player.position); // keep updating target every frame since player moves

                if (distanceToPlayer <= attackRange)
                {
                    currentState = State.Attack;
                    lastAttackTime = Time.time; // first swing waits a full cooldown, gives the player a beat
                }
                else if (distanceToPlayer > loseTargetRange)
                {
                    currentState = State.Idle; // lost them, go back to patrolling
                    agent.speed = patrolSpeed; // and slow back down
                }
                break;

            case State.Attack:
                agent.SetDestination(transform.position); // stop moving

                // face the player while attacking
                Vector3 dir = (player.position - transform.position).normalized;
                dir.y = 0;
                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
                }

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    AttackPlayer();
                    lastAttackTime = Time.time;
                }

                if (distanceToPlayer > attackRange)
                {
                    currentState = State.Chase; // player backed off, chase again
                }
                break;
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0)
        {
            agent.SetDestination(spawnPosition); // no route, so head back to where we started
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void AttackPlayer()
    {
        // hits the player through PlayerHealth script
        if (playerHealth != null)
        {
            playerHealth.DamagePlayer(attackDamage);
        }

        Debug.Log("Enemy attacks player for " + attackDamage + " damage!");
    }

    public void TakeDamage(float damage) // Method to apply damage to the enemy
    {
        enemyHealth -= damage; // Reduce the enemy's health by the damage amount
    }

    // draws the ranges in the Scene view when this enemy is selected, handy for tuning
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // yellow = notices you
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); // red = hits you
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange); // gray = gives up chasing
    }
}