using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.SDK;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] public MLClickable Startclickable;
    [SerializeField] public MLClickable Stopclickable;
    [SerializeField] public MLClickable SettingsClickable;

    [SerializeField] private GameObject dronePrefab; // The prefab for the drones
    [SerializeField] private Transform[] spawnPoints; // Array of spawn points
    [SerializeField] private float spawnRadius = 50f; // The radius within which drones will spawn
    [SerializeField] private int initialWaveCount = 3; // Initial number of drones to spawn
    [SerializeField] private float waveInterval = 10f; // Time interval between each wave
    [SerializeField] private Text timerText; // The Text component to display the timer
    [SerializeField] private Text GameStatus; // The Text component to display the game status
    [SerializeField] private GameObject ExplosionPrefab; // The prefab for the explosion, when the drones die.
    [SerializeField] private GameObject CrystalHearth; // The prefab for the explosion, when the drones die.
    [SerializeField] public GameObject TurretAttackerDrone;
    [SerializeField] public int wave_incrementindex = 4;

    private List<GameObject> spawnedDrones = new List<GameObject>();
    private float timer = 0f;
    public bool isGameRunning = false;
    private int waveCount = 0;
    private GameObject temp_Explosion;
    private CrystalHearth crystal_H;
    private MLPlayer player;


    // Called when the Start button is clicked
    public void OnStart()
    {
        UnityEngine.Debug.Log("Started!");
        isGameRunning = true;
        waveCount = initialWaveCount;
        StartCoroutine(SpawnWaves());
        timer = 0;
        GameStatus.text = "Active";
        StartCoroutine(UpdateTimer());

        crystal_H = (CrystalHearth)CrystalHearth.GetComponent(typeof(CrystalHearth));
        crystal_H.ResetCrystal(25);

        crystal_H.Crystal.SetActive(true);

    }

    // Called when the Stop button is clicked
    public void OnStop()
    {
        UnityEngine.Debug.Log("Stopped!");
        isGameRunning = false;
        StopAllCoroutines();
        DestroyAllDrones();
        GameStatus.text = "Inactive";
    }

    // Spawn drones in waves with increasing quantity
    private IEnumerator SpawnWaves()
    {
        while (isGameRunning)
        {
            SpawnDrones(waveCount);
            waveCount+= wave_incrementindex; // Increase the number of drones by wave_incrementindex
            yield return new WaitForSeconds(waveInterval);
        }
    }

    // Spawn a specific number of drones from random spawn points
    private void SpawnDrones(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Choose a random spawn point
            Transform chosenSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Calculate a random position within the radius around the chosen spawn point
            Vector3 randomPosition = chosenSpawnPoint.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                Random.Range(10f, 30f), // Spawn at a random height in the air
                Random.Range(-spawnRadius, spawnRadius)
            );

            // Instantiate the drone and add it to the list
            // 25% chance to drop an ammo box
            if (Random.value <= 0.25f)
            {
                GameObject drone = Instantiate(TurretAttackerDrone, randomPosition, Quaternion.identity);
                spawnedDrones.Add(drone);

            }
            else
            {
                GameObject drone = Instantiate(dronePrefab, randomPosition, Quaternion.identity);
                spawnedDrones.Add(drone);

            }
        }
    }

    // Destroy all spawned drones with explosion effect
    private void DestroyAllDrones()
    {
        foreach (GameObject drone in spawnedDrones)
        {
            if (drone != null)
            {
                temp_Explosion = Instantiate(ExplosionPrefab, drone.transform.position, Quaternion.identity);
                Destroy(temp_Explosion, 2);
                Destroy(drone);
            }
        }
        spawnedDrones.Clear();
    }

    // Update the timer and display it on the Text component
    private IEnumerator UpdateTimer()
    {
        while (isGameRunning)
        {
            timer += Time.deltaTime;
            timerText.text = $"Time: {timer:F2}"; // Display time with 2 decimal places
            yield return null;
        }
    }

    public void OnSettingsToggle()
    {
        UnityEngine.Debug.Log("Settings button pressed!");
    }

    public void OnLaserPointerEnter()
    {
        UnityEngine.Debug.Log("Pointer Entered!");
    }

    public void OnLaserPointerExit()
    {
        UnityEngine.Debug.Log("Pointer Exited!");
    }

    void Start()
    {
        if (Startclickable != null)
        {
            Startclickable.OnClick.AddListener(OnStart);
            Startclickable.OnPointerEnter.AddListener(OnLaserPointerEnter);
            Startclickable.OnPointerExit.AddListener(OnLaserPointerExit);

            Stopclickable.OnClick.AddListener(OnStop);
            Stopclickable.OnPointerEnter.AddListener(OnLaserPointerEnter);
            Stopclickable.OnPointerExit.AddListener(OnLaserPointerExit);

            SettingsClickable.OnClick.AddListener(OnSettingsToggle);
            SettingsClickable.OnPointerEnter.AddListener(OnLaserPointerEnter);
            SettingsClickable.OnPointerExit.AddListener(OnLaserPointerExit);
        }

        StartCoroutine(InitializePlayer());


    }

    IEnumerator InitializePlayer()
    {
        while (player == null)
        {
            Debug.Log("Waiting started");
            yield return new WaitForSeconds(2f);
            player = ML.SDK.MassiveLoopRoom.GetLocalPlayer();
            Debug.Log("Checking player...");

            if (player != null)
            {
              //  Debug.Log("Searching for Player Complete, setting turret credit number.");
              //  player.SetProperty("turretCredit", 100);
            }

        }
        Debug.Log("Waiting ended");
    }

    void Update()
    {
        // Update logic can be added here if needed
    }
}
