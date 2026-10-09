using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public float range = 20f;
    public float verticalRange = 5f;
    public float fireRate = 1f;
    public float damage = 2f;
    public AudioSource shotAudio;

    private float nextTimeToFire;
    private BoxCollider gunTrigger;

    private bool shootingEnabled = true;

    public LayerMask raycastLayerMask;
    public EnemyManager enemyManager;

    public Animator weaponAnimator; // Reference to the Animator component attached to the weapon

    void Start()
    {
        gunTrigger = GetComponent<BoxCollider>();
        gunTrigger.size = new Vector3(1, verticalRange, range);
        gunTrigger.center = new Vector3(0, 0, range * 0.5f);
    }

    void Update()
    {
        if (!shootingEnabled)
            return;

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.isPressed && Time.time >= nextTimeToFire)
        {
            Fire();
        }
    }

    public void SetShootingEnabled(bool enabled)
    {
        shootingEnabled = enabled;
    }

    void Fire()
    {
        //gun fire animation trigger
        weaponAnimator.SetTrigger("Fire");
        
        //play ShotGun sound effect
        shotAudio.Stop();
        shotAudio.Play();

        foreach (var enemy in enemyManager.enemiesInTrigger)
        {
            var dir = enemy.transform.position - transform.position;

            RaycastHit hit;
            if (Physics.Raycast(
                transform.position,
                dir,
                out hit,
                range * 1.5f,
                raycastLayerMask))
            {
                if (hit.transform == enemy.transform)
                {
                    enemy.TakeDamage(damage);

                    Debug.DrawRay(
                        transform.position,
                        dir,
                        Color.red,
                        1f
                    );
                }
            }
        }

        nextTimeToFire = Time.time + fireRate;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.transform.GetComponent<Enemy>();

        if (enemy)
        {
            enemyManager.AddEnemy(enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.transform.GetComponent<Enemy>();

        if (enemy)
        {
            enemyManager.RemoveEnemy(enemy);
        }
    }
}
