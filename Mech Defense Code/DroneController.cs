using UnityEngine;

public class DroneController : MonoBehaviour
{
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float stoppingDistance = 5f;
    [SerializeField] private float attackRange = 10f; // Range within which the drone can attack the Crystal Hearth
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float minShootingInterval = 1f;
    [SerializeField] private float maxShootingInterval = 3f;
    [SerializeField] private float minRepositionInterval = 3f;
    [SerializeField] private float maxRepositionInterval = 6f;
    [SerializeField] private float repositionRadius = 10f;
    [SerializeField] private float repositionSpeed = 2f;
    [SerializeField] private float deAggroRadius = 20f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float laserDuration = 0.5f;
    [SerializeField] public int health = 5;
    [SerializeField] public GameObject explosion;
    [SerializeField] public GameObject laserimpact;
    [SerializeField] public GameObject ammoBoxPrefab; // Add this line
    [SerializeField] public GameObject moneyprefab;

    private Transform crystalHearth;
    private float shootingTimer;
    private float repositionTimer;
    private float nextShootingInterval;
    private float nextRepositionInterval;
    private Vector3 targetPosition;
    private LineRenderer lineRenderer;
    private float laserTimer;

    private GameObject ExplosionHolder;

    void Start()
    {
        targetPosition = transform.position;
        nextShootingInterval = Random.Range(minShootingInterval, maxShootingInterval);
        nextRepositionInterval = Random.Range(minRepositionInterval, maxRepositionInterval);

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;

        // Immediately start chasing the Crystal_Hearth
        SearchForCrystalHearth();
    }

    void Update()
    {
        if (crystalHearth != null)
        {
            ChaseCrystalHearth();
            if (IsWithinAttackRange())
            {
                ShootAtCrystalHearth();
            }
            else
            {
                MoveCloserToCrystalHearth();
            }
        }

        // Handle laser visibility duration
        if (lineRenderer.enabled)
        {
            laserTimer += Time.deltaTime;
            if (laserTimer >= laserDuration)
            {
                lineRenderer.enabled = false;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Damage taken, health: " + health);

        TriggerReposition();

        if (health <= 0)
        {
            // 25% chance to drop an ammo box
            if (Random.value <= 0.25f)
            {
                Instantiate(ammoBoxPrefab, transform.position, Quaternion.identity);
            }
            if(Random.value <= 0.15f)
            {
                Instantiate(moneyprefab, transform.position, Quaternion.identity);
            }

            ExplosionHolder = Instantiate(explosion, gameObject.transform.position, Quaternion.identity);
            Destroy(ExplosionHolder, 1.5f);
            Destroy(gameObject);
        }
    }

    private void ChaseCrystalHearth()
    {
        if (crystalHearth == null) return;

        float distance = Vector3.Distance(transform.position, crystalHearth.position);

        if (distance > deAggroRadius)
        {
            crystalHearth = null;
            Debug.Log("Crystal Hearth out of range, stopping chase...");
            return;
        }

        Vector3 direction = (crystalHearth.position - transform.position).normalized;

        if (distance > stoppingDistance)
        {
            transform.position += direction * chaseSpeed * Time.deltaTime;
        }

        Vector3 lookDirection = crystalHearth.position - bulletSpawnPoint.position;
        bulletSpawnPoint.rotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    private void MoveCloserToCrystalHearth()
    {
        if (crystalHearth == null) return;

        float distance = Vector3.Distance(transform.position, crystalHearth.position);

        if (distance > attackRange)
        {
            Vector3 direction = (crystalHearth.position - transform.position).normalized;
            transform.position += direction * chaseSpeed * Time.deltaTime;

            Vector3 lookDirection = crystalHearth.position - bulletSpawnPoint.position;
            bulletSpawnPoint.rotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private CrystalHearth crystal_H;
    private GameObject hitgameobject;

    private void ShootAtCrystalHearth()
    {
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

                hitgameobject = hit.collider.gameObject;

                if (hit.transform == crystalHearth)
                {
                    Debug.Log("Crystal Hearth hit by laser!");
                    crystal_H = (CrystalHearth)hitgameobject.GetComponent(typeof(CrystalHearth));
                    crystal_H.TakeDamage(1);
                }
            }
        }
    }

    private bool IsWithinAttackRange()
    {
        if (crystalHearth == null) return false;
        return Vector3.Distance(transform.position, crystalHearth.position) <= attackRange;
    }

    private void SearchForCrystalHearth()
    {
        GameObject crystalHearthObject = GameObject.Find("Crystal_Hearth");
        if (crystalHearthObject != null)
        {
            crystalHearth = crystalHearthObject.transform;
            Debug.Log("Crystal Hearth found! Chasing...");
        }
        else
        {
            Debug.LogError("Crystal_Hearth not found in the scene.");
        }
    }

    private void SmoothRotate(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Roaming functionality remains here but is not used
    private void RepositionDrone()
    {
        repositionTimer += Time.deltaTime;

        if (repositionTimer >= nextRepositionInterval)
        {
            TriggerReposition();
        }

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
        } while (crystalHearth != null && Vector3.Distance(randomDirection, crystalHearth.position) < stoppingDistance);

        targetPosition = randomDirection;
    }
}
