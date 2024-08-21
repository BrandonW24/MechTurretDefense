using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private GameObject hiteffect;

    private Collider turretCollider;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
    }

    public void SetOwnerCollider(Collider ownerCollider)
    {
        turretCollider = ownerCollider;
        Physics.IgnoreCollision(GetComponent<Collider>(), turretCollider);
    }


    private DroneController hitscript;
    private TurretBase turretscript;
    private DroneController_Bullet bulletDrones;
    private void OnTriggerEnter(Collider other)
    {
        if (other != turretCollider)  // Check to avoid hitting the turret
        {
            // Implement damage logic here
            if (other.name.Contains("Drone"))
            {
                // Deal damage to the drone
                hitscript = (DroneController)other.GetComponent(typeof(DroneController));

                temp_hiteffect = Object.Instantiate(hiteffect, transform.position, Quaternion.identity);
                Object.Destroy(temp_hiteffect, 3);

                if (hitscript != null)
                {
                    hitscript.TakeDamage(damageAmount);
                    Destroy(gameObject);
                }
                else if(hitscript == null)
                {
                    bulletDrones = (DroneController_Bullet)other.GetComponent(typeof(DroneController_Bullet));
                    if (bulletDrones != null)
                    {
                        bulletDrones.TakeDamage(damageAmount);
                        Destroy(gameObject);
                    }

                }

                // Display the laser using LineRenderer
                // StartCoroutine(FireLaser(hit.point));
            }else if(other.name.Contains("Bullet") & other.name.Contains("Drone"))
            {
                bulletDrones = (DroneController_Bullet)other.GetComponent(typeof(DroneController_Bullet));
                if (bulletDrones != null)
                {
                    bulletDrones.TakeDamage(damageAmount);
                    Destroy(gameObject);
                }

            }

           // Destroy(gameObject);
        }
    }


    private GameObject temp_hiteffect;

    private void OnCollisionEnter(Collision collision)
    {
        

        if (collision.gameObject.name.Contains("Drone"))
        {
            // Deal damage to the drone
            hitscript = (DroneController)collision.gameObject.GetComponent(typeof(DroneController));
            if (hitscript != null)
            {
                temp_hiteffect = Object.Instantiate(hiteffect, transform.position, Quaternion.identity);
                Object.Destroy(temp_hiteffect, 3);
                hitscript.TakeDamage(damageAmount);
                Destroy(gameObject);
            }

            // Display the laser using LineRenderer
            // StartCoroutine(FireLaser(hit.point));
        }

        Destroy(gameObject);
    }
}
