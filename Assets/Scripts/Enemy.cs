using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public EnemyManager enemyManager; // Reference to the EnemyManager script
    private float enemyHealth = 2f; // Enemy's health

    [Header("References")]
    private NavMeshAgent agent;
    private Transform player; // grabbed once in Start, no need to look it up every frame

    [Header("Detection")]
    public float detectionRange = 10f;  // how close player has to get before we notice them
    public float attackRange = 2f;      // how close before we stop and swing
    public float loseTargetRange = 15f; // if player gets this far away mid chase we give up

    [Header("Attack")]
    public float attackDamage = 1f;
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

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
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
            enemyManager.RemoveEnemy(this); // Remove the enemy from the list in EnemyManager
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
                }
                break;

            case State.Chase:
                agent.SetDestination(player.position); // keep updating target every frame since player moves

                if (distanceToPlayer <= attackRange)
                {
                    currentState = State.Attack;
                }
                else if (distanceToPlayer > loseTargetRange)
                {
                    currentState = State.Idle; // lost them, go back to patrolling
                }
                break;

            case State.Attack:
                agent.SetDestination(transform.position); // stop moving

                // face the player while attacking, just looks better
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
        if (patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void AttackPlayer()
    {
        // just a log for now until player health exists
        Debug.Log("Enemy attacks player for " + attackDamage + " damage!");

        // player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
    }

    public void TakeDamage(float damage) // Method to apply damage to the enemy
    {
        enemyHealth -= damage; // Reduce the enemy's health by the damage amount
    }
}