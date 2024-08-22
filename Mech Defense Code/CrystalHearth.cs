using UnityEngine;
using UnityEngine.UI;

public class CrystalHearth : MonoBehaviour
{
    [SerializeField] private int ResetHealth;
    [SerializeField] private int health; // Crystal Hearth's initial health
    [SerializeField] private GameObject gameManagerObject; // Reference to the GameManager GameObject
    [SerializeField] private Slider healthBar; // Reference to the UI Slider representing the health bar
    [SerializeField] private GameObject Explosion; // Reference to the GameManager GameObject
    [SerializeField] public GameObject Crystal;


    private GameManager gameManager;

    void Start()
    {
        // Initialize the health bar
        if (healthBar != null)
        {
            healthBar.maxValue = ResetHealth;
            healthBar.value = health;
        }
        else
        {
            Debug.LogError("HealthBar Slider reference is not set.");
        }

        // Initialize the game manager reference
        if (gameManagerObject != null)
        {
            gameManager = (GameManager)gameManagerObject.GetComponent(typeof(GameManager));
            if (gameManager == null)
            {
                Debug.LogError("GameManager component not found on the specified GameObject.");
            }
        }
        else
        {
            Debug.LogError("GameManager GameObject reference is not set.");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log("Crystal Hearth took damage, health: " + health);

        // Update the health bar
        if (healthBar != null)
        {
            healthBar.value = health;
        }

        if (health <= 0)
        {
            OnCrystalDestroyed();
        }
    }

    public void ResetCrystal(int HP)
    {
        if (gameManager != null)
        {
            health = HP;

            // Update the health bar
            if (healthBar != null)
            {
                healthBar.value = health;
                Crystal.SetActive(true);
            }
        }
    }

    private GameObject tempExplosion;
    private void OnCrystalDestroyed()
    {
        Debug.Log("Crystal Hearth destroyed!");
        tempExplosion = Object.Instantiate(Explosion, transform.position, Quaternion.identity);
        Object.Destroy(tempExplosion, 6);

        if (gameManager != null)
        {
            Crystal.SetActive(false);
            gameManager.OnStop(); // Call the OnStop function from the GameManager
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle other triggers if needed
    }
}
