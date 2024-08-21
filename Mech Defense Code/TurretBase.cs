using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class TurretBase : MonoBehaviour
{
    [Header("Turret Settings")]
    [SerializeField] public float attackRange = 20f;  // Range within which the turret can target drones
    [SerializeField] public float fireRate = 1f;      // Time between shots
    [SerializeField] public int turretHP = 100;       // Turret health
    [SerializeField] public GameObject bulletPrefab;  // The bullet prefab to shoot
    [SerializeField] public Transform gunMount;       // The part of the turret that swivels to aim
    [SerializeField] public Transform firePoint;      // The point from which bullets are fired
    [SerializeField] public GameObject MuzzleFire;    // Muzzle fire effect
    [SerializeField] private Slider healthBar; // Reference to the UI Slider representing the health bar
    [SerializeField] public GameObject deathExplosion;
    [SerializeField] public GameObject UpgradeHPEffect;
    [SerializeField] public GameObject UpgradeFireRateEffect;
    [SerializeField] public GameObject RepairEffect;
   // [SerializeField] public GameObject TurretGun;

    private float fireTimer;
    private Transform targetDrone;
    private SphereCollider detectionCollider;
    private GameManager gameManager;
    private float checkInterval = 0.5f; // Interval between target checks
    private float checkTimer = 0f;      // Timer for tracking target check intervals

    private int maxHP;  // Store the maximum HP for the turret
    private Vector3 gunOriginLocalPosition;
    private float randomRotationSpeed = 1f; // Speed of random rotation
    private float retractionSpeed = 5f; // Speed of gun retraction
    private float randomRotationOffset;
    private GameObject temp_effect;


    void Start()
    {
        Debug.Log("Turret start");
        randomRotationOffset = Random.Range(0f, 360f);

        // Set up the SphereCollider as a trigger
     //   detectionCollider = GetComponent<SphereCollider>();
      //  detectionCollider.isTrigger = true;
      //  detectionCollider.radius = attackRange;
      //  gunOriginLocalPosition = TurretGun.transform.localPosition;

        maxHP = turretHP;  // Initialize maxHP with the starting HP

        // Initialize the health bar
        if (healthBar != null)
        {
            healthBar.maxValue = maxHP;
            healthBar.value = turretHP;
        }
        else
        {
            Debug.LogError("HealthBar Slider reference is not set.");
        }
    }

    void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            UpdateTargetDrone();
        }

        if (targetDrone != null)
        {
            RotateTowardsTarget();
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                fireTimer = 0f;
                Shoot();
            }
        }
        else
        {
            RandomlyRotateTurret();
        }
    }

    private void UpdateTargetDrone()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        float shortestDistance = float.MaxValue;

        foreach (Collider collider in hitColliders)
        {
            if (collider.name.Contains("Drone"))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    targetDrone = collider.transform;
                }
            }
        }

        if (shortestDistance == float.MaxValue)
        {
            targetDrone = null;
        }
    }

    private void RotateTowardsTarget()
    {
        if (targetDrone == null) return;

        Vector3 direction = (targetDrone.position - gunMount.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        gunMount.rotation = Quaternion.Slerp(gunMount.rotation, lookRotation, Time.deltaTime * 5f); // Adjust rotation speed as needed
        firePoint.rotation = Quaternion.Slerp(firePoint.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        BulletController bulletcontrollerscript = (BulletController)bullet.GetComponent(typeof(BulletController));

        GameObject temp_muzzlefire = Instantiate(MuzzleFire, firePoint.position, firePoint.rotation);
        Destroy(temp_muzzlefire, 1f);

        // Start coroutine to handle gun retraction
       // StartCoroutine(RetractAndReturnGun());

        if (bulletcontrollerscript != null)
        {
            bulletcontrollerscript.SetOwnerCollider(GetComponent<Collider>());
        }
    }

    private void RandomlyRotateTurret()
    {
        float randomYRotation = Mathf.Sin(Time.time * randomRotationSpeed + randomRotationOffset) * 90f;
        Quaternion randomRotation = Quaternion.Euler(0f, randomYRotation, 0f);
        gunMount.rotation = Quaternion.Slerp(gunMount.rotation, randomRotation, Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        turretHP -= damage;

        healthBar.value = turretHP;

        if (turretHP <= 0)
        {
            Vector3 offset = new Vector3(0, 3, 0);
            Quaternion rotationConst = new Quaternion(-90, 0, 0,0);
            GameObject temp_explosion = Object.Instantiate(deathExplosion, transform.position + offset, rotationConst);
            Destroy(temp_explosion, 3);
            Destroy(gameObject);
        }
    }

    public void UpgradeHP(int hpIncrease)
    {
        maxHP += hpIncrease;    // Increase the maximum HP
        turretHP += hpIncrease; // Also increase current HP by the same amount
        Debug.Log($"Turret HP upgraded by {hpIncrease}. New Max HP: {maxHP}");

        Vector3 offset = new Vector3(0, 1, 0);
        Quaternion rotationConst = new Quaternion(90, 0, 0, 0);
        temp_effect = Object.Instantiate(UpgradeHPEffect, transform.position + offset, rotationConst);
        Destroy(temp_effect, 2);

        healthBar.maxValue = maxHP;
        healthBar.value = turretHP;
    }

    public void UpgradeFireRate(float rateIncrease)
    {
        fireRate -= rateIncrease; // Decrease the fire rate time (shoots faster)
        Vector3 offset = new Vector3(0, 1, 0);
        Quaternion rotationConst = new Quaternion(90, 0, 0, 0);
        temp_effect = Object.Instantiate(UpgradeFireRateEffect, transform.position + offset, rotationConst);
        Destroy(temp_effect, 2);

        if (fireRate < 0.1f)
        {
            fireRate = 0.1f; // Cap the fire rate so it doesn't go too fast
        }
        Debug.Log($"Turret fire rate upgraded. New Fire Rate: {fireRate}");
    }

    public void RepairTurret(int repairAmount)
    {
        turretHP += repairAmount;

        healthBar.value = turretHP;
        Vector3 offset = new Vector3(0, 1, 0);
        Quaternion rotationConst = new Quaternion(90, 0, 0, 0);
        temp_effect = Object.Instantiate(RepairEffect, transform.position + offset, rotationConst);
        Destroy(temp_effect, 2);

        if (turretHP > maxHP)
        {
            turretHP = maxHP; // Ensure HP does not exceed max HP

        }
        Debug.Log($"Turret repaired by {repairAmount}. Current HP: {turretHP}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Drone"))
        {
            UpdateTargetDrone();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (targetDrone == other.transform)
        {
            UpdateTargetDrone();
        }
    }
}
