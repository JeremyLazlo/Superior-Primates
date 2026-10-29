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

    [Header("Ranged Enemy Settings")]
    public bool isRanged = false;        // tick this to make a ranged enemy
    public float shootRange = 9f;        // ranged enemies stop and shoot once they are inside this distance
    public float tooCloseRange = 4f;     // ranged enemies back away if the player gets closer than this
    public float retreatDistance = 4f;   // how far they try to back off each time
    public GameObject projectilePrefab;  // needs the EnemyProjectile script, if empty the shot just hits instantly
    public Transform firePoint;          // optional spot the shot comes from, uses the enemy's eye position if empty

    [Header("Sounds (optional)")]
    public AudioClip alertSound;         // plays when the enemy spots the player and starts chasing
    public AudioClip investigateSound;   // plays when the enemy loses the player and goes to check the last spot
    public AudioClip idleSound;          // groans every so often while the enemy hasn't noticed the player
    public AudioClip attackSound;        // plays on every swing, or every shot for ranged enemies
    public AudioClip hurtSound;          // plays when the enemy gets shot
    public AudioClip deathSound;         // plays when the enemy dies
    public float idleSoundMinDelay = 6f;   // shortest wait between idle sounds
    public float idleSoundMaxDelay = 14f;  // longest wait between idle sounds
    public float soundMaxDistance = 20f;   // how far away the player can still hear this enemy
    private AudioSource audioSource;
    private float nextIdleSoundTime;

    // FOOTSTEPS
    public AudioClip footstepClip;         // one single step sound, played again every time the enemy has walked stepDistance
    public float stepDistance = 1.2f;      // how far the enemy walks between steps, lower = faster steps
    public float footstepVolume = 0.5f;    // quieter than the player's steps so you hear them as a warning
    private float distanceSinceLastStep;

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

        // sounds come from an AudioSource on the enemy, add one if the prefab doesn't have it yet
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // fully 3D so you can tell where the enemy is from the sound
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = soundMaxDistance;
        nextIdleSoundTime = Time.time + Random.Range(idleSoundMinDelay, idleSoundMaxDelay);

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
            if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, transform.position); // plays at the spot, the enemy's own AudioSource is about to be destroyed
            Destroy(gameObject);
            return;
        }

        if (player == null) return; // bail if we never found the player for some reason
        if (!agent.isOnNavMesh) return; // not on a NavMesh yet (for example a freshly spawned area), nothing to do

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        PlayFootsteps(); // FOOTSTEPS

        switch (currentState)
        {
            case State.Idle:
                Patrol();
                MaybePlayIdleSound();

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

                    if (isRanged)
                    {
                        ChaseRanged(distanceToPlayer); // ranged enemies keep their distance instead of running up to the player
                    }
                    else
                    {
                        agent.SetDestination(player.position); // keep updating target every frame since player moves

                        if (distanceToPlayer <= attackRange)
                        {
                            currentState = State.Attack;
                            lastAttackTime = Time.time; // first swing waits a full cooldown, gives the player a beat
                        }
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

                // no hitting through walls, and chase again if they back off (ranged enemies use their shoot range instead)
                float maxAttackDistance = isRanged ? shootRange : attackRange;
                if (!HasClearPath() || distanceToPlayer > maxAttackDistance)
                {
                    currentState = State.Chase;
                    break;
                }

                // ranged enemies try to back away if the player gets too close, if they can't (cornered) they keep shooting
                if (isRanged && distanceToPlayer < tooCloseRange && TryRetreat())
                {
                    currentState = State.Chase;
                    break;
                }

                // ranged enemies only fire once they are actually turned toward the player
                bool facingPlayer = Vector3.Angle(transform.forward, dir) < 20f;

                if (Time.time >= lastAttackTime + attackCooldown && (!isRanged || facingPlayer))
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
                MaybePlayIdleSound();
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

    // ranged chase: back away if too close, close the gap if too far, otherwise stop and shoot
    void ChaseRanged(float distanceToPlayer)
    {
        if (distanceToPlayer < tooCloseRange)
        {
            if (!TryRetreat())
            {
                currentState = State.Attack; // nowhere to run, so fight back
                lastAttackTime = Time.time;
            }
        }
        else if (distanceToPlayer > shootRange)
        {
            agent.SetDestination(player.position); // too far to shoot, get closer
        }
        else
        {
            currentState = State.Attack; // in the sweet spot, stop and shoot
            lastAttackTime = Time.time;
        }
    }

    // walks directly away from the player, returns false if there is no walkable spot to back up to
    bool TryRetreat()
    {
        Vector3 away = transform.position - player.position;
        away.y = 0f;

        if (away == Vector3.zero) return false;

        Vector3 target = transform.position + away.normalized * retreatDistance;

        if (NavMesh.SamplePosition(target, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
            return true;
        }

        return false;
    }

    // fires a projectile at where the player is right now, so moving makes you dodge it
    void FireRanged()
    {
        Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * eyeHeight + transform.forward * 0.6f;
        Vector3 direction = player.position - origin;

        if (projectilePrefab != null)
        {
            GameObject shot = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction));
            EnemyProjectile projectile = shot.GetComponent<EnemyProjectile>();
            if (projectile != null) projectile.Launch(direction, attackDamage);
        }
        else if (playerHealth != null)
        {
            playerHealth.DamagePlayer(attackDamage); // no prefab assigned yet, so just hit instantly
        }
    }

    // plays a sound from the enemy, does nothing if no clip was assigned
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.pitch = 1f; // FOOTSTEPS: footsteps nudge the pitch a little, this puts it back for the other sounds
            audioSource.PlayOneShot(clip);
        }
    }

    // FOOTSTEPS
    // plays one step sound each time the enemy has walked stepDistance, it uses the agent's real speed so it goes quiet when the enemy stops
    void PlayFootsteps()
    {
        if (footstepClip == null || audioSource == null) return;

        distanceSinceLastStep += agent.velocity.magnitude * Time.deltaTime;

        if (distanceSinceLastStep >= stepDistance)
        {
            distanceSinceLastStep = 0f;
            audioSource.pitch = Random.Range(0.9f, 1.1f); // small random change so every step doesn't sound identical
            audioSource.PlayOneShot(footstepClip, footstepVolume);
        }
    }

    // random groans while the enemy hasn't noticed the player, keeps them from being silent
    void MaybePlayIdleSound()
    {
        if (idleSound == null || Time.time < nextIdleSoundTime) return;

        PlaySound(idleSound);
        nextIdleSoundTime = Time.time + Random.Range(idleSoundMinDelay, idleSoundMaxDelay);
    }

    void StartChase()
    {
        if (currentState != State.Chase && currentState != State.Attack) PlaySound(alertSound); // only when we first spot them, not every time we go back into chase

        currentState = State.Chase;
        agent.speed = chaseSpeed; // Initiate the chase speed (faster than patrol)
        lastKnownPosition = player.position; // so if we get shot from out of sight we still know where to look
    }

    void StartInvestigate(Vector3 position)
    {
        lastKnownPosition = position;
        currentState = State.Investigate;
        PlaySound(investigateSound);
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
        PlaySound(attackSound); // swing or shot sound, the same for melee and ranged

        if (isRanged)
        {
            FireRanged();
            return;
        }

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
        if (enemyHealth > 0) PlaySound(hurtSound); // the death sound covers the killing shot

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

        if (isRanged)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, shootRange); // magenta = starts shooting inside this
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, tooCloseRange); // orange = backs away inside this
        }
    }

    // shows what each enemy is doing above its head, only while the game is running, for testing and tuning
    // void OnGUI()
    // {
    //     if (!showStateLabel || Camera.main == null) return;

    //     Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.6f);
    //     if (screenPos.z <= 0f || screenPos.z > 25f) return; // behind the camera or too far away

    //     GUI.Label(new Rect(screenPos.x - 40f, Screen.height - screenPos.y - 10f, 120f, 20f), currentState.ToString());
    // }
}