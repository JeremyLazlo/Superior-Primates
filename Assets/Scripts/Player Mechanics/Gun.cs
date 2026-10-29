using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public float range = 20f;
    public float verticalRange = 5f;
    public float fireRate = 1f;
    public float damage = 2f;
    public AudioSource shotAudio;

    public LayerMask raycastLayerMask;
    public EnemyManager enemyManager;
    public Animator weaponAnimator;

    [Header("Shotgun")]
    public Camera playerCamera;
    public int bulletsPerShot = 6;

    [Range(0f, 30f)]
    public float spreadAngle = 5f;

    [Header("Bullet Trails")]
    public Transform muzzlePoint;
    public TrailRenderer bulletTrailPrefab;
    public float trailSpeed = 100f;

    private float nextTimeToFire;
    private BoxCollider gunTrigger;
    private bool shootingEnabled = false;
    private bool inventoryAllowsShooting = true;

    private void Awake()
    {
        SetShootingEnabled(false);
    }


    void Start()
    {
        gunTrigger = GetComponent<BoxCollider>();
        if (gunTrigger != null)
        {
            gunTrigger.size = new Vector3(1, verticalRange, range);
            gunTrigger.center = new Vector3(0, 0, range * 0.5f);
        }
    }

    void Update()
    {
        if (!shootingEnabled ||
            !inventoryAllowsShooting ||
            Time.timeScale <= 0f)
            return;

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.isPressed &&
            Time.time >= nextTimeToFire)
        {
            Fire();
        }
    }

    public void SetShootingEnabled(bool enabled)
    {
        shootingEnabled = enabled;

        if (TryGetComponent(out GunAmmo ammo))
            ammo.SetShootingEnabled(enabled);

        Debug.Log(
            $"Gun shooting set to {enabled} on {gameObject.name}",
            this
        );
    }

    public void SetInventoryAllowsShooting(bool allowed)
    {
        inventoryAllowsShooting = allowed;
    }

    void Fire()
    {
        Debug.Log(
        $"Fire() called on {gameObject.name}. " +
        $"Shooting enabled: {shootingEnabled}"
        );

        if (playerCamera == null)
            return;
        // out of ammo
        if (TryGetComponent(out GunAmmo ammo) && !ammo.TryUseRound()) return;

        nextTimeToFire = Time.time + fireRate;

        if (weaponAnimator != null)
            weaponAnimator.SetTrigger("Fire");

        if (shotAudio != null)
        {
            shotAudio.Stop();
            shotAudio.Play();
        }

        Transform aim = playerCamera.transform;

        Vector3 trailStart = muzzlePoint != null
            ? muzzlePoint.position
            : aim.position;

        float spreadRadius =
            Mathf.Tan(spreadAngle * Mathf.Deg2Rad);

        for (int i = 0; i < bulletsPerShot; i++)
        {
            Vector2 spread = Random.insideUnitCircle * spreadRadius;

            Vector3 direction = (
                aim.forward +
                aim.right * spread.x +
                aim.up * spread.y
            ).normalized;

            Vector3 endPoint = aim.position + direction * range;

            if (Physics.Raycast(
                aim.position,
                direction,
                out RaycastHit hit,
                range,
                raycastLayerMask,
                QueryTriggerInteraction.Ignore))
            {
                endPoint = hit.point;

                Enemy enemy = hit.collider.GetComponentInParent<Enemy>();

                if (enemy != null)
                    enemy.TakeDamage(damage);
            }

            if (bulletTrailPrefab != null)
                StartCoroutine(SpawnTrail(trailStart, endPoint));
        }

        if (ammo != null && ammo.currentAmmo == 0)
            ammo.TryStartReload();
    }

    IEnumerator SpawnTrail(Vector3 startPoint, Vector3 endPoint)
    {
        TrailRenderer trail = Instantiate(
            bulletTrailPrefab,
            startPoint,
            Quaternion.identity
        );

        trail.autodestruct = false;
        trail.Clear();
        trail.emitting = true;
        trail.AddPosition(startPoint);

        while (Vector3.Distance(
            trail.transform.position, endPoint) > 0.001f)
        {
            trail.transform.position = Vector3.MoveTowards(
                trail.transform.position,
                endPoint,
                Mathf.Max(trailSpeed, 1f) * Time.deltaTime
            );

            trail.AddPosition(trail.transform.position);
            yield return null;
        }

        trail.emitting = false;
        Destroy(trail.gameObject, trail.time);
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.transform.GetComponent<Enemy>();

        if (enemy && enemyManager != null)
        {
            enemyManager.AddEnemy(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.transform.GetComponent<Enemy>();

        if (enemy && enemyManager != null)
        {
            enemyManager.RemoveEnemy(enemy);
        }
    }
}
