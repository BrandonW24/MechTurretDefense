using UnityEngine;

public class DroneController_Bullet : MonoBehaviour
{
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float stoppingDistance = 5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float minShootingInterval = 1f;
    [SerializeField] private float maxShootingInterval = 3f;
    [SerializeField] private float minRepositionInterval = 3f;
    [SerializeField] private float maxRepositionInterval = 6f;
    [SerializeField] private float repositionRadius = 10f;
    [SerializeField] private float repositionSpeed = 2f;
    [SerializeField] private float searchRadius = 15f; // Range within which the drone can detect the turret
    [SerializeField] private float deAggroRadius = 20f; // Range within which the drone will stop chasing the turret
    [SerializeField] private float rotationSpeed = 2f; // Speed of rotation
    [SerializeField] private float attackRange = 10f; // Range within which the drone can attack the turret
    [SerializeField] private int health = 5;
    [SerializeField] public GameObject explosion;
    [SerializeField] public GameObject laserimpact;
    [SerializeField] private float laserDuration = 0.5f;
    [SerializeField] public GameObject muzzleFlare;


    private Transform mech;
    private float shootingTimer;
    private float repositionTimer;
    private float nextShootingInterval;
    private float nextRepositionInterval;
    private Vector3 targetPosition;
    private bool isChasingMech;
    private LineRenderer lineRenderer;
    private float laserTimer;
    private GameObject ExplosionHolder;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        targetPosition = transform.position;
        nextShootingInterval = Random.Range(minShootingInterval, maxShootingInterval);
        nextRepositionInterval = Random.Range(minRepositionInterval, maxRepositionInterval);
        isChasingMech = false; // Initially not chasing
    }

    private bool IsWithinAttackRange()
    {
        if (mech == null) return false;
        return Vector3.Distance(transform.position, mech.position) <= attackRange;
    }

    void Update()
    {
        if (mech == null || !mech.gameObject.activeInHierarchy)
        {
            SearchForMech();
            Roam();
        }
        else if (isChasingMech)
        {
            if (IsWithinAttackRange())
            {
                ChaseMech();
                ShootAtMech();
            }
            else
            {
                ChaseMech();
            }

            // ShootAtMech();
        }

        if (lineRenderer.enabled)
        {
            laserTimer += Time.deltaTime;
            if (laserTimer >= laserDuration)
            {
                lineRenderer.enabled = false;
            }
        }

        RepositionDrone();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log(gameObject.name + " Damage taken, health: " + health);

        // Trigger an immediate reposition on damage
      //  TriggerReposition();

        if (health <= 0)
        {
            ExplosionHolder = Instantiate(explosion, gameObject.transform.position, Quaternion.identity);
            Destroy(ExplosionHolder, 1.5f);
            Destroy(gameObject);
        }
    }

    private void ChaseMech()
    {
        float distance = Vector3.Distance(transform.position, mech.position);

        if (distance > deAggroRadius)
        {
            // Mech is out of range, stop chasing and resume roaming
            mech = null;
            isChasingMech = false;
            Debug.Log("Mech out of range, resuming roam...");
            return;
        }

        // Rotate the whole drone towards the mech
        Vector3 direction = (mech.position - transform.position).normalized;
        SmoothRotate(direction);

        if (distance > stoppingDistance)
        {
            transform.position += direction * chaseSpeed * Time.deltaTime;
        }

        // Rotate the bullet spawn point to face the mech
        Vector3 lookDirection = mech.position - bulletSpawnPoint.position;
        bulletSpawnPoint.rotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.LookRotation(lookDirection);

    }

    private TurretBase turret;
    private GameObject hitgameobject;
    private GameObject MuzzleFlash;

    private void ShootAtMech()
    {
        float distance = Vector3.Distance(transform.position, mech.position);
        shootingTimer += Time.deltaTime;

        if (shootingTimer >= nextShootingInterval)
        {
            shootingTimer = 0f;
            nextShootingInterval = Random.Range(minShootingInterval, maxShootingInterval);

            RaycastHit hit;
            if (Physics.Raycast(bulletSpawnPoint.position, bulletSpawnPoint.forward, out hit))
            {
                lineRenderer.SetPosition(0, bulletSpawnPoint.position);
                lineRenderer.SetPosition(1, hit.point);
                lineRenderer.enabled = true;
                laserTimer = 0f;

                ExplosionHolder = Object.Instantiate(laserimpact, hit.point, Quaternion.identity);
                Object.Destroy(ExplosionHolder, 2);

                MuzzleFlash = Object.Instantiate(muzzleFlare, bulletSpawnPoint.transform.position, Quaternion.identity);
                Object.Destroy(MuzzleFlash, 2);


                hitgameobject = hit.collider.gameObject;

                if (hitgameobject.name.Contains("Turret"))
                {
                    Debug.Log("Crystal Hearth hit by laser!");
                    turret = (TurretBase)hitgameobject.GetComponent(typeof(TurretBase));
                    turret.TakeDamage(1);
                }
            }
        }
    }

    private void RepositionDrone()
    {
        repositionTimer += Time.deltaTime;

        if (repositionTimer >= nextRepositionInterval)
        {
            TriggerReposition();
        }

        // Move towards the target position smoothly
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position = Vector3.Lerp(transform.position, targetPosition, repositionSpeed * Time.deltaTime);
        SmoothRotate(direction);
    }

    private void TriggerReposition()
    {
        repositionTimer = 0f;
        nextRepositionInterval = Random.Range(minRepositionInterval, maxRepositionInterval);

        Vector3 randomDirection;

        do
        {
            randomDirection = Random.insideUnitSphere * repositionRadius;
            randomDirection += transform.position;
            // Allow the drone to move in all directions, including up and down
        } while (mech != null && Vector3.Distance(randomDirection, mech.position) < stoppingDistance);

        targetPosition = randomDirection;
    }

    private void Roam()
    {
        // Roam to random positions if mech is not assigned
        repositionTimer += Time.deltaTime;

        if (repositionTimer >= nextRepositionInterval)
        {
            TriggerReposition();
        }

        // Move towards the target position smoothly
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position = Vector3.Lerp(transform.position, targetPosition, repositionSpeed * Time.deltaTime);
        SmoothRotate(direction);
    }

    private void SearchForMech()
    {
        // Perform a sphere cast to find objects within the search radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, searchRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.name.Contains("Turret"))
            {
                mech = hitCollider.transform;
                isChasingMech = true;
                Debug.Log("Mech found! Chasing...");
                break;
            }
        }
    }

    private void SmoothRotate(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
          //  transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
