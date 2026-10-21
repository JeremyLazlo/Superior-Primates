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
    private Vector3 spawnPosition; // remembered original postition so enemies with no patrol points can walk back
    private Quaternion spawnRotation; // the way we were facing at spawn, we turn back to this once we're home

    [Header("Detection")]
    public float detectionRange = 10f;  // how close player has to get before we notice them
    public float attackRange = 2f;      // how close before we stop and swing
    public float loseTargetRange = 15f; // if player gets this far away mid chase we give up (a little more than detectionRange)
    public float viewAngle = 110f;      // full width of the vision cone in degrees, the player is only noticed inside it
    public float closeSenseRange = 2.5f; // very close behind us still counts, so you can't just stand at their back
    public float eyeHeight = 0.5f;      // how far above the enemy's center the sight ray starts
    public LayerMask sightMask = ~0;    // what blocks sight, everything by default (trigger colliders are ignored)

    [Header("Speed")]
    public float patrolSpeed = 1.5f; // slow walk while we haven't noticed the player
    public float chaseSpeed = 2.5f;  // speed once we're going after them
    public float investigateSpeed = 2f; // a bit quicker than patrol when checking where we last saw them

    [Header("Search")]
    public float searchDuration = 4f;   // seconds spent looking around before giving up
    public float searchTurnSpeed = 90f; // turning speed while looking around

    [Header("Attack")]
    public int attackDamage = 10; // PlayerHealth.DamagePlayer takes an int
    public float attackCooldown = 1.5f; // seconds between hits so we're not attacking every frame
    private float lastAttackTime = -999f;

    [Header("Patrol (optional)")]
    public Transform[] patrolPoints; // leave empty if this enemy should just stand still until it spots you
    private int currentPatrolIndex = 0;

    [Header("Debug")]
    public bool showStateLabel = true; // shows the current state above each enemy while testing, turn off for a clean look

    // state machine for enemy behavior
    // idle = patrolling or walking back to spawn, hasn't noticed the player yet
    // chase = we can see the player and we're going after them
    // attack = close enough to actually hit them
    // investigate = lost sight, walking to the last place we saw them
    // search = got there, looking around for a few seconds before giving up
    private enum State { Idle, Chase, Attack, Investigate, Search }
    private State currentState = State.Idle;

    private Vector3 lastKnownPosition; // where we last saw or heard the player
    private float searchTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed; // start out slow, speed up once we spot the player (chase mode)
        spawnPosition = transform.position; // remember where we started
        spawnRotation = transform.rotation; // and which way we were facing

        // enemies spawned from a prefab can't have this assigned in the Inspector, so find it ourselves
        if (enemyManager == null) enemyManager = FindAnyObjectByType<EnemyManager>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>(); // grab it once here too
        }

        if (patrolPoints.Length > 0 && agent.isOnNavMesh)
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
        if (!agent.isOnNavMesh) return; // not on a NavMesh yet (for example a freshly spawned area), nothing to do

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                Patrol();

                if (CanSeePlayer())
                {
                    StartChase(); // spotted them, pick up the pace
                }
                break;

            case State.Chase:
                // Dont need cone when chasing, just a clear path and that the player not too far
                bool stillSees = distanceToPlayer <= loseTargetRange && HasClearPath();

                if (stillSees)
                {
                    lastKnownPosition = player.position; // keep updating where they are so enemy can chase them if player runs behind a wall
                    agent.SetDestination(player.position); // keep updating target every frame since player moves

                    if (distanceToPlayer <= attackRange)
                    {
                        currentState = State.Attack;
                        lastAttackTime = Time.time; // first swing waits a full cooldown, gives the player a beat
                    }
                }
                else
                {
                    StartInvestigate(lastKnownPosition); // lost player, go check where we last saw them
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

                // no hitting through walls, and chase again if they back off
                if (!HasClearPath() || distanceToPlayer > attackRange)
                {
                    currentState = State.Chase;
                    break;
                }

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    AttackPlayer();
                    lastAttackTime = Time.time;
                }
                break;

            case State.Investigate:
                if (CanSeePlayer())
                {
                    StartChase(); // found player again
                    break;
                }

                // arrived at the last known spot, start looking around
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    currentState = State.Search;
                    searchTimer = 0f;
                    agent.ResetPath();
                }
                break;

            case State.Search:
                transform.Rotate(0f, searchTurnSpeed * Time.deltaTime, 0f); // look around
                searchTimer += Time.deltaTime;

                if (CanSeePlayer())
                {
                    StartChase();
                }
                else if (searchTimer >= searchDuration)
                {
                    currentState = State.Idle; // gave up, go back to patrolling
                    agent.speed = patrolSpeed; // and slow back down
                }
                break;
        }
    }

    void StartChase()
    {
        currentState = State.Chase;
        agent.speed = chaseSpeed; // Initiate the chase speed (faster than patrol)
        lastKnownPosition = player.position; // so if we get shot from out of sight we still know where to look
    }

    void StartInvestigate(Vector3 position)
    {
        lastKnownPosition = position;
        currentState = State.Investigate;
        agent.speed = investigateSpeed;
        agent.SetDestination(position);
    }

    // inside the vision cone (or very close), close enough, and nothing in the way
    bool CanSeePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > detectionRange) return false;

        if (distance > closeSenseRange)
        {
            Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z); // ignore height for the angle
            if (Vector3.Angle(transform.forward, flat) > viewAngle * 0.5f) return false;
        }

        return HasClearPath();
    }

    // shoots a ray at the player, if it hits anything that isn't the player then a wall is in the way
    bool HasClearPath()
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 toTarget = player.position - eye;

        if (Physics.Raycast(eye, toTarget.normalized, out RaycastHit hit, toTarget.magnitude, sightMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform.IsChildOf(player);
        }

        return true; // nothing between player and enemy
    }

    // not used yet, a gunshot script could call this so enemies go check out the sound
    public void HearNoise(Vector3 position)
    {
        if (!agent.isOnNavMesh) return;

        if (currentState == State.Idle || currentState == State.Search)
        {
            StartInvestigate(position);
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0)
        {
            agent.SetDestination(spawnPosition); // no route, so head back to where we started

            // once we're home, slowly turn back to face the way we started
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, spawnRotation, 90f * Time.deltaTime);
            }
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

        // getting shot tells us where the player is, so go after them unless we already are
        if (player != null && agent != null && agent.isOnNavMesh && currentState != State.Chase && currentState != State.Attack)
        {
            StartChase();
        }
    }

    // draws the ranges and the vision cone in the Scene view when this enemy is selected, for tuning and debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // yellow = notices you
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); // red = hits you
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange); // gray = gives up chasing

        Vector3 left = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward;
        Gizmos.color = Color.cyan; // cyan = edges of the vision cone
        Gizmos.DrawRay(transform.position, left * detectionRange);
        Gizmos.DrawRay(transform.position, right * detectionRange);

        Gizmos.color = Color.green; // green = the way the enemy is facing
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }

    // shows what each enemy is doing above its head, only while the game is running, for testing and tuning
    void OnGUI()
    {
        if (!showStateLabel || Camera.main == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.6f);
        if (screenPos.z <= 0f || screenPos.z > 25f) return; // behind the camera or too far away

        GUI.Label(new Rect(screenPos.x - 40f, Screen.height - screenPos.y - 10f, 120f, 20f), currentState.ToString());
    }
}