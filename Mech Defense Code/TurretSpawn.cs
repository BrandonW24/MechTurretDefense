using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.SDK;
using UnityEngine.UI;

public class TurretSpawn : MonoBehaviour
{
    [SerializeField] public Transform raycastPoint;
    [SerializeField] public MLGrab grabComponent;
    [SerializeField] public MLClickable UpgradeClickable;
    [SerializeField] public MLClickable BuildClickable;
    [SerializeField] public MLClickable SellClickable;
    [SerializeField] public GameObject UpgradeButtonObject_Parent;
    [SerializeField] public GameObject BuildButtonObject_Parent;

    [SerializeField] public MLClickable UpgradeButton_HP;
    [SerializeField] public MLClickable UpgradeButton_DMG;
    [SerializeField] public MLClickable UpgradeButton_REPAIR;

    [SerializeField] public MLClickable Build_LaserTurret;
    [SerializeField] public MLClickable Build_RepeaterTurret;
    [SerializeField] public MLClickable Build_MissileTurret;


    [SerializeField] public GameObject[] UpgradeButton_children;
    [SerializeField] public GameObject[] BuildButton_children;

    [SerializeField] private Material originalMaterial;
    [SerializeField] private Material highlightMaterial;
    //[SerializeField] private Material highlightSell;
    [SerializeField] public Material[] laserMats;

    [SerializeField] private GameObject laserTurretPrefab; // Prefab for laser turret
    [SerializeField] private GameObject repeaterTurretPrefab; // Prefab for repeater turret
    [SerializeField] private GameObject missileTurretPrefab; // Prefab for missile turret

    [SerializeField] private LineRenderer lineRenderer; // Reference to the LineRenderer component

    [SerializeField] public Text moneytext;
    [SerializeField] public Text ActionText;

    private TurretBase currentTurret;
    private GameObject selectedTurretPrefab; // The currently selected turret prefab

    private MLPlayer player;

    [SerializeField] public int funds = 60;
    [SerializeField] public int costToUpgrade_FR = 10;
    [SerializeField] public int costToUpgrade_HP = 10;
    [SerializeField] public int costToUpgrade_Repair = 5;

    [SerializeField] public int costToBuildLaserTurret = 15;
    [SerializeField] public int costToBuildRepeaterTurret = 15;
    [SerializeField] public int costToBuildMissileTurret = 30;




    void UpgradeMenuSelect()
    {
        if (UpgradeButtonObject_Parent != null)
        {
            for (int i = 0; i < UpgradeButton_children.Length; i++)
            {
                if (!UpgradeButton_children[i].activeInHierarchy)
                {
                    UpgradeButtonObject_Parent.SetActive(true);
                }
            }

            for (int i = 0; i < BuildButton_children.Length; i++)
            {
                if (BuildButton_children[i].activeInHierarchy)
                {
                    BuildButton_children[i].SetActive(false);
                }
            }
        }

        lineRenderer.material = laserMats[2];
        ActionText.text = "Upgrading";
    }

    void Upgrade_Turret_HP()
    {
        if (funds >= costToUpgrade_HP)
        {
            if (currentTurret != null)
            {
                currentTurret.UpgradeHP(10);
                funds -= costToUpgrade_HP;
                moneytext.text = $"${funds}";
            }
        }
        //lineRenderer.material = laserMats[0];
    }

    void UpgradeTurret_DMGFUNC()
    {
        if (funds >= costToUpgrade_FR)
        {
            if (currentTurret != null)
            {
                currentTurret.UpgradeFireRate(.3f);
                funds -= costToUpgrade_FR;
                moneytext.text = $"${funds}";
            }
        }
        else
        {
            ActionText.text = "Insufficient funds";
        }
    }

    void UpgradeButton_REPAIR_FUNC()
    {
        if (funds >= costToUpgrade_Repair)
        {
            if (currentTurret != null)
            {
                currentTurret.RepairTurret(10);
                funds -= costToUpgrade_Repair;
                moneytext.text = $"${funds}";
            }
        }

        else
        {
            ActionText.text = "Insufficient funds";
        }
    }

    void BuildMenuSelect()
    {
        if (BuildButtonObject_Parent != null)
        {
            for (int i = 0; i < UpgradeButton_children.Length; i++)
            {
                if (UpgradeButton_children[i].activeInHierarchy)
                {
                    UpgradeButton_children[i].SetActive(false);
                }
            }

            for (int i = 0; i < BuildButton_children.Length; i++)
            {
                if (!BuildButton_children[i].activeInHierarchy)
                {
                    BuildButton_children[i].SetActive(true);
                }
            }
        }

        lineRenderer.material = laserMats[1];
        ActionText.text = "Building";
    }

    void SellTurret()
    {
        // Implement selling logic here
        lineRenderer.material = laserMats[3];
        ActionText.text = "Selling";

        if (currentTurret != null)
        {
            Object.Destroy(currentTurret.gameObject);
            funds += costToBuildLaserTurret;
            moneytext.text = $"${funds}";
        }
        else
        {
            ActionText.text = "Highlight Turret To Sell";
        }

    }

    void BuildLaserTurret_new()
    {

        if (funds >= costToBuildLaserTurret)
        {
            ActionText.text = "Building Laser Turret";

            selectedTurretPrefab = laserTurretPrefab;
            lineRenderer.material = laserMats[1];
            //player.SetProperty("turretCredit", (int)player.GetProperty("turretCredit") - 20);
            // moneytext.text = (string)player.GetProperty("turretCredit");

            funds -= costToBuildLaserTurret;
            moneytext.text = $"${funds}";

        }

        else
        {
            ActionText.text = "Insufficient funds";
        }
    }

    void BuildRepeaterTurret_new()
    {
        if (funds >= costToBuildRepeaterTurret)
        {
            ActionText.text = "Building Repeater Turret";

            selectedTurretPrefab = repeaterTurretPrefab;
            lineRenderer.material = laserMats[1];

            //player.SetProperty("turretCredit", (int)player.GetProperty("turretCredit") - 20);
            // moneytext.text = (string)player.GetProperty("turretCredit");

            funds -= costToBuildRepeaterTurret;
            moneytext.text = $"${funds}";


        }

        else
        {
            ActionText.text = "Insufficient funds";
        }
    }

    void Build_MissileTurret_new()
    {
        if (funds >= costToBuildMissileTurret)
        {
            ActionText.text = "Building Repeater Turret";

            selectedTurretPrefab = missileTurretPrefab;
            lineRenderer.material = laserMats[1];

            //player.SetProperty("turretCredit", (int)player.GetProperty("turretCredit") - 20);
            // moneytext.text = (string)player.GetProperty("turretCredit");

            funds -= costToBuildMissileTurret;
            moneytext.text = $"${funds}";


        }

        else
        {
            ActionText.text = "Insufficient funds";
        }
    }

    void EvokeFunction()
    {
        if (selectedTurretPrefab != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(raycastPoint.position, raycastPoint.forward, out hit))
            {
                if (hit.collider != null)
                {
                    // Instantiate the turret a little above the hit point
                    Vector3 spawnPosition = hit.point + new Vector3(0, 1f, 0);
                    GameObject turretInstance = Instantiate(selectedTurretPrefab, spawnPosition, transform.rotation);

                    // Reset the selected turret prefab after placement
                    selectedTurretPrefab = null;

                    lineRenderer.material = laserMats[0];

                    ActionText.text = "idle";
                }
                selectedTurretPrefab = null;
            }
        }
    }

    void Start()
    {

        Debug.Log("Start Turret Editor");

        if (UpgradeClickable != null)
        {
            UpgradeClickable.OnClick.AddListener(UpgradeMenuSelect);
            BuildClickable.OnClick.AddListener(BuildMenuSelect);
            SellClickable.OnClick.AddListener(SellTurret);

            UpgradeButton_HP.OnClick.AddListener(Upgrade_Turret_HP);
            UpgradeButton_DMG.OnClick.AddListener(UpgradeTurret_DMGFUNC);
            UpgradeButton_REPAIR.OnClick.AddListener(UpgradeButton_REPAIR_FUNC);

            Build_LaserTurret.OnClick.AddListener(BuildLaserTurret_new);
            Build_RepeaterTurret.OnClick.AddListener(BuildRepeaterTurret_new);
            Build_MissileTurret.OnClick.AddListener(Build_MissileTurret_new);
        }

        if (grabComponent != null)
        {
            grabComponent.OnPrimaryTriggerDown.AddListener(EvokeFunction);
        }

        // Ensure LineRenderer is properly initialized
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }


        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;

        moneytext.text = $"${funds}";


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
                Debug.Log("Searching for Player Complete.");
                moneytext.text = (string)player.GetProperty("turretCredit");
                ActionText.text = "idle";

                Debug.Log("Checking to see if player money exists");
                if (!player.PropertyExists("turretCredit"))
                {
                    Debug.Log("Player money did not exist. Setting.");
                    player.SetProperty("turretCredit", 100);
                    Debug.Log("player property : " + (string)player.GetProperty("turretCredit"));
                    moneytext.text = (string)player.GetProperty("turretCredit");

                    moneytext.text = $"${funds}";

                }

            }

        }
        Debug.Log("Waiting ended");
    }

    void Update()
    {
        DetectTurret();
    }

    void DetectTurret()
    {
        RaycastHit hit;
        if (Physics.Raycast(raycastPoint.position, raycastPoint.forward, out hit))
        {
            // Ensure the lineRenderer has the correct number of positions
            if (lineRenderer.positionCount < 2)
            {
                lineRenderer.positionCount = 2;
            }

            // Update LineRenderer to show the raycast
            lineRenderer.SetPosition(0, raycastPoint.position);
            lineRenderer.SetPosition(1, hit.point);

            if (hit.collider.name.Contains("Turret"))
            {
                TurretBase turret = (TurretBase)hit.collider.GetComponent(typeof(TurretBase));

                if (turret != null)
                {
                    if (currentTurret != turret)
                    {
                        ResetTurretMaterial();

                        currentTurret = turret;
                        ChangeTurretMaterial(highlightMaterial);
                    }
                }
                else
                {
                    ResetTurretMaterial();
                    currentTurret = null;
                }
            }
            else
            {
                ResetTurretMaterial();
                currentTurret = null;
            }
        }
        else
        {
            // If raycast doesn't hit anything, set the end of the line renderer to a far point
            lineRenderer.SetPosition(0, raycastPoint.position);
            lineRenderer.SetPosition(1, raycastPoint.position + raycastPoint.forward * 100f);
        }

        if (hit.collider != null && hit.collider.name.Contains("Money"))
        {
            funds += Random.Range(20, 30);
            moneytext.text = $"${funds}";
            Destroy(hit.collider.gameObject);
        }
    }

    void ChangeTurretMaterial(Material mat)
    {
        if (currentTurret != null)
        {
            Renderer renderer = currentTurret.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = mat;
            }
        }
    }

    void ResetTurretMaterial()
    {
        if (currentTurret != null)
        {
            Renderer renderer = currentTurret.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = originalMaterial;
            }
        }
    }
}
