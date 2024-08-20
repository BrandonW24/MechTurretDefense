using System.Collections;
using UnityEngine;
using ML.SDK;
using UnityEngine.UI;


public class StationSettler : MonoBehaviour
{
    [SerializeField]
    public MLStation station;
    public GameObject LookPos;
    private MLPlayer player;
    private bool underLocalPlayerControl;
    private UserInput input;
    private int direction;

    private float throttle;
    private float breaks;
    private float speedSMA;

    private const float DM_KEY_TRESHOLD = 0.1f;
    private const float THROTTLE_FACTOR = 0.02f;
    private const float NEAR_ZERO = 0.01f;
    private const float MIN_SPEED_TO_CHANGE_DIRECTION = 0.1f;
    private const float MAX_REVERSE_THROTTLE = -1f;
    private const float STEERING_FACTOR = 0.05f;
    private const float MOVE_SPEED = 1.5f;
    private const float UPRIGHT_LERP_SPEED = 2f;

    private float steering;

    public Animator mechAni;

    private GameObject avatarObj;
    public GameObject spinepiece;

    private Transform playerCamera;

    public GameObject[] weaponVisuals;

    private Rigidbody rb;
    private Quaternion initialRotation;
    private bool isMoving;

    private Vector3 initialLookPosOffset;

    public float maxGrappleRange = 50f;

    public AudioSource reloadSound;
    public AudioSource footstep;

    public Transform[] shootingOriginPosition;
    public GameObject secondHand;
    private bool secondWeapon;
    private DualWielding secondHandScript;


    void OnSeated()
    {
        Debug.Log("Player has been seated ");
        if (player != null)
        {
            Debug.Log("Player sitting in seat : " + player.NickName);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision entered");
        if (other.gameObject.name.Contains("w_") & ifWeaponHeld == false)
        {
            if (other.gameObject.name.Contains("Bolt"))
            {
                Debug.Log("Boltgun found");
                weaponVisuals[0].SetActive(true);
                ifWeaponHeld = true;
                raycastOrigin = shootingOriginPosition[0];

                if (weaponVisuals[1].activeInHierarchy)
                {
                    weaponVisuals[1].SetActive(false);
                    raycastOrigin = shootingOriginPosition[0];
                }
            }
            else if (other.gameObject.name.Contains("Laser"))
            {
                Debug.Log("Laser canon found");
                weaponVisuals[1].SetActive(true);
                ifWeaponHeld = true;
                raycastOrigin = shootingOriginPosition[1];

                if (weaponVisuals[0].activeInHierarchy)
                {
                    weaponVisuals[0].SetActive(false);
                    raycastOrigin = shootingOriginPosition[1];
                }

            }
        }

        if(other.gameObject.name.Contains("CleanWeapon") & ifWeaponHeld != false)
        {

            for(int i = 0; i < weaponVisuals.Length; i++)
            {
                weaponVisuals[i].SetActive(false);
            }
            ifWeaponHeld = false;

        }


    }


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        StartCoroutine(InitializePlayer());
        station.OnPlayerSeated.AddListener(OnSeated);
        initialRotation = transform.rotation;
        secondHandScript = (DualWielding)secondHand.GetComponent(typeof(DualWielding));

        if (AmmoBar != null)
        {
            AmmoBar.maxValue = ammo_max;
            AmmoBar.value = ammo;

            ClipsBar.maxValue = maxClips;
            ClipsBar.value = clips;
        }
        else
        {
            Debug.LogError("HealthBar Slider reference is not set.");
        }

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
                Debug.Log("Searching for Player Complete");
                avatarObj = player.AvatarTrackedObject;
            }

            GameObject mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (mainCameraObject != null)
            {
                playerCamera = mainCameraObject.transform;
                Debug.Log("Player camera found: " + playerCamera.position);
            }
            else
            {
                Debug.LogWarning("Waiting for player to be initialized...");
            }
        }
        Debug.Log("Waiting ended");
    }

    public void AddAmmo(int amount)
    {
        if (clips < maxClips)
        {
            clips += amount;
            Debug.Log("Ammo added: " + amount + ". Total Ammo: " + ammo);
            ClipsBar.value = clips;
        }
    }

    IEnumerator WaitForReload()
    {

        if (!reloadSound.isPlaying)
        {
            reloadSound.Play();
        }

        isReloading = true;
        yield return new WaitForSeconds(2f);
        mechAni.SetBool("shoot", false);
        mechAni.SetBool("reload", false);
        mechAni.SetBool("idle", true);

        ammo = ammo_max; // Reload ammo (reset to full clip)
        AmmoBar.value = ammo;
        clips--;
        ClipsBar.value = clips;
        isReloading = false;

        Debug.Log("Reloading Done");

    }

    IEnumerator WaitForReload_Left()
    {

        if (!reloadSound.isPlaying)
        {
            reloadSound.Play();
        }

        isReloading_left = true;
        yield return new WaitForSeconds(2f);
        mechAni.SetBool("shoot", false);
        mechAni.SetBool("reload", false);
        mechAni.SetBool("idle", true);

        ammo = 30; // Reload ammo (reset to full clip)
        AmmoBar.value = ammo;
        clips--;
        ClipsBar.value = clips;
        isReloading_left = false;

        Debug.Log("Reloading Done");

    }

    void Update()
    {
        if (station.IsOccupied)
        {
            player = station.GetPlayer();
            if (player.IsLocal)
            {
                underLocalPlayerControl = true;
                input = station.GetInput();
                if (direction == 0)
                {
                    direction = 1;
                }

                if (MassiveLoopClient.IsInDesktopMode)
                {
                    HandleDesktopModeInput(input);
                }
                else
                {
                    HandleVRModeInput(input);
                }

                if (playerCamera != null)
                {
                    // Updated LookPos based on playerCamera's direction
                    LookPos.transform.position = playerCamera.position + playerCamera.forward * 14.0f; // Adjust distance as needed

                    float angularDifferenceBetweenPortalRotations = Quaternion.Angle(playerCamera.rotation, LookPos.transform.rotation);
                    Quaternion portalRotationalDifference = Quaternion.AngleAxis(angularDifferenceBetweenPortalRotations, Vector3.up);

                }
            }
            else if (!player.IsLocal)
            {
                underLocalPlayerControl = false;
            }
        }
        else if (!station.IsOccupied)
        {
            underLocalPlayerControl = false;
        }

    }


    public int ammo = 30; // Bullets per clip
    public int clips = 5; // Total clips available
    public int maxClips = 5;
    public float reloadTime = 2.0f; // Time taken to reload
    public int ammo_max;
    private bool isFiring = false;
    private bool ads = false; // Example for aiming down sights toggle
    private bool isReloading = false; // Flag to check if reloading
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private AudioSource Gun_audioSource;
    public AudioSource PassedinAudio;
    public AudioClip audi0;

    public float BulletDistance = 100;
    public GameObject[] MuzzleFire;
    //public MLGrab mlgrabObject;
    public GameObject[] impactEle;
    public bool isAutomatic = false;
    public float fireRate = 0.1f; // Time between shots in automatic mode
    public AudioClip[] SoundEffects;
    public Transform raycastOrigin; // Transform for custom raycast origin
    private bool ifWeaponHeld = false;
    private LineRenderer lineRenderer;
    private GameObject hitgameobject;
    private bool isFiring_Left = false;
    [SerializeField] private Slider AmmoBar; // Reference to the UI Slider representing the health bar
    [SerializeField] private Slider ClipsBar; // Reference to the UI Slider representing the health bar



    IEnumerator AutomaticFire()
    {
        mechAni.SetBool("shoot", true);
        mechAni.SetBool("idle", false);
        while (isFiring)
        {
            FireBullet();
            yield return new WaitForSeconds(fireRate);
        }
    }

    IEnumerator AutomaticFire_Left()
    {
        mechAni.SetBool("shoot", true);
        mechAni.SetBool("idle", false);
        while (isFiring_Left)
        {
            FireBulletLeft();
            yield return new WaitForSeconds(fireRate);
        }
    }

    void PlayRandomFireSound()
    {
        Debug.Log("Play random fire sound evoked");
        if (SoundEffects != null && SoundEffects.Length >= 4)
        {
            int randomIndex = Random.Range(0, 3); // Select a random index between 0 and 3

            Gun_audioSource.clip = SoundEffects[randomIndex];
            //Won't work
            Gun_audioSource.Play();

            //PlayOneShot does not appear to work either?
            // Gun_audioSource.PlayOneShot(Gun_audioSource.clip);

            Debug.Log("Play random fire sound should have played");
            Debug.Log("Random clip = " + SoundEffects[randomIndex]);
            //  audioSource.PlayOneShot(SoundEffects[randomIndex]);
        }
        else
        {
            Debug.LogWarning("SoundEffects array is not properly set. Ensure it has at least 4 elements.");
        }
    }


    private DroneController hitscript;

    void FireBullet()
    {
        if (ammo > 0)
        {
            ammo--;
            AmmoBar.value = ammo;

            RaycastHit hit;
            var ray = new Ray(raycastOrigin ? raycastOrigin.position : transform.position, raycastOrigin ? raycastOrigin.forward : transform.forward);
            Debug.Log("Raycast fired");
            //    PlayRandomFireSound();

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Getting effect");
                //Check which weapon is being held

                if (weaponVisuals[0].activeInHierarchy)
                {
                    //  Debug.Log("Instantiating boltgun effect");
                    var effectInstance = Instantiate(impactEle[0], hit.point, Quaternion.identity);
                    effectInstance.transform.LookAt(hit.point + hit.normal);
                    Destroy(effectInstance, 20);

                    hitgameobject = hit.collider.gameObject;

                    effectInstance.transform.parent = hitgameobject.transform;

                    var MuzzleFireInstance = Instantiate(MuzzleFire[0], raycastOrigin.position, transform.rotation);
                    Destroy(MuzzleFireInstance, 4);


                    if (hitgameobject.name.Contains("Drone"))
                    {
                        Debug.Log("Weapon found drone, attempting to deal damage");
                        hitscript = (DroneController)hitgameobject.GetComponent(typeof(DroneController));
                        hitscript.TakeDamage(1);
                    }


                }
                //Laser Canon
                else if (weaponVisuals[1].activeInHierarchy)
                {
                    // Debug.Log("Instantiating laser effect");
                    var effectInstance = Instantiate(impactEle[1], hit.point, Quaternion.identity);
                    effectInstance.transform.LookAt(hit.point + hit.normal);
                    Destroy(effectInstance, 20);

                    var MuzzleFireInstance = Instantiate(MuzzleFire[1], raycastOrigin.position, transform.rotation);
                    Destroy(MuzzleFireInstance, 4);

                    hitgameobject = hit.collider.gameObject;


                    lineRenderer = (LineRenderer)weaponVisuals[1].GetComponent(typeof(LineRenderer));
                    lineRenderer.SetPosition(0, raycastOrigin.position);
                    lineRenderer.enabled = true;


                    lineRenderer.SetPosition(1, hit.point);
                    StartCoroutine(LazerReset());


                    if (hitgameobject.name.Contains("Drone"))
                    {
                        Debug.Log("Weapon found drone, attempting to deal damage");
                        hitscript = (DroneController)hitgameobject.GetComponent(typeof(DroneController));
                        hitscript.TakeDamage(1);
                    }

                }
            }
            else
            {

                Debug.Log("Getting effect");
                //Check which weapon is being held

                if (weaponVisuals[0].activeInHierarchy)
                {
                    //  Debug.Log("Instantiating boltgun effect");
                    //   var effectInstance = Instantiate(impactEle[0], hit.point, Quaternion.identity);
                    //   effectInstance.transform.LookAt(hit.point + hit.normal);
                    //    Destroy(effectInstance, 20);

                    var MuzzleFireInstance = Instantiate(MuzzleFire[0], raycastOrigin.position, transform.rotation);
                    Destroy(MuzzleFireInstance, 4);

                }
                //Laser Canon
                else if (weaponVisuals[1].activeInHierarchy)
                {
                    // Debug.Log("Instantiating laser effect");
                    //    var effectInstance = Instantiate(impactEle[1], hit.point, Quaternion.identity);
                    //    effectInstance.transform.LookAt(hit.point + hit.normal);
                    //    Destroy(effectInstance, 20);

                    var MuzzleFireInstance = Instantiate(MuzzleFire[1], raycastOrigin.position, transform.rotation);
                    Destroy(MuzzleFireInstance, 4);

                    lineRenderer = (LineRenderer)weaponVisuals[1].GetComponent(typeof(LineRenderer));
                    lineRenderer.SetPosition(0, raycastOrigin.position);
                    lineRenderer.enabled = true;


                    lineRenderer.SetPosition(1, LookPos.transform.position);
                    StartCoroutine(LazerReset());

                }

            }


            // ApplyRecoil();

        }
        else if (clips > 0)
        {
            Debug.Log("Reloading");
            mechAni.SetBool("shoot", false);
            mechAni.SetBool("reload", true);
            mechAni.SetBool("idle", false);

            StartCoroutine(WaitForReload());
        }
    }


    public Transform raycastOrigin_Left;
    private bool isReloading_left = false;
    public GameObject[] weaponVisuals_Left;


    void FireBulletLeft()
    {
        if (ammo > 0)
        {
            ammo--;
            AmmoBar.value = ammo;

            RaycastHit hit;
            var ray = new Ray(raycastOrigin_Left ? raycastOrigin_Left.position : transform.position, raycastOrigin_Left ? raycastOrigin_Left.forward : transform.forward);
            Debug.Log("Raycast fired");
            //    PlayRandomFireSound();

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Getting effect");
                //Check which weapon is being held

                if (weaponVisuals[0].activeInHierarchy)
                {
                    //  Debug.Log("Instantiating boltgun effect");
                    var effectInstance = Instantiate(impactEle[0], hit.point, Quaternion.identity);
                    effectInstance.transform.LookAt(hit.point + hit.normal);
                    Destroy(effectInstance, 20);

                    hitgameobject = hit.collider.gameObject;

                    effectInstance.transform.parent = hitgameobject.transform;

                    var MuzzleFireInstance = Instantiate(MuzzleFire[0], raycastOrigin_Left.position, transform.rotation);
                    Destroy(MuzzleFireInstance, 4);


                    if (hitgameobject.name.Contains("Drone"))
                    {
                        Debug.Log("Weapon found drone, attempting to deal damage");
                        hitscript = (DroneController)hitgameobject.GetComponent(typeof(DroneController));
                        hitscript.TakeDamage(1);
                    }


                }
                /*
                //Laser Canon
                else if (weaponVisuals[1].activeInHierarchy)
                {
                    // Debug.Log("Instantiating laser effect");
                    var effectInstance = Instantiate(impactEle[1], hit.point, Quaternion.identity);
                    effectInstance.transform.LookAt(hit.point + hit.normal);
                    Destroy(effectInstance, 20);

                    var MuzzleFireInstance = Instantiate(MuzzleFire[1], raycastOrigin_Left.position, transform.rotation);
                    Destroy(MuzzleFireInstance, 4);

                    hitgameobject = hit.collider.gameObject;


                    lineRenderer = (LineRenderer)weaponVisuals[1].GetComponent(typeof(LineRenderer));
                    lineRenderer.SetPosition(0, raycastOrigin_Left.position);
                    lineRenderer.enabled = true;


                    lineRenderer.SetPosition(1, hit.point);
                    StartCoroutine(LazerReset());


                    if (hitgameobject.name.Contains("Drone"))
                    {
                        Debug.Log("Weapon found drone, attempting to deal damage");
                        hitscript = (DroneController)hitgameobject.GetComponent(typeof(DroneController));
                        hitscript.TakeDamage();
                    }

                }*/
            }
            else
            {

                //Miss attack
              //  Debug.Log("Getting effect");
                //Check which weapon is being held

                if (weaponVisuals[0].activeInHierarchy)
                {
                    //  Debug.Log("Instantiating boltgun effect");
                    //   var effectInstance = Instantiate(impactEle[0], hit.point, Quaternion.identity);
                    //   effectInstance.transform.LookAt(hit.point + hit.normal);
                    //    Destroy(effectInstance, 20);

                    var MuzzleFireInstance = Instantiate(MuzzleFire[0], raycastOrigin_Left.position, transform.rotation);
                    Destroy(MuzzleFireInstance, 4);

                }

                /*
                //Laser Canon miss attack
                else if (weaponVisuals[1].activeInHierarchy)
                {
                    // Debug.Log("Instantiating laser effect");
                    //    var effectInstance = Instantiate(impactEle[1], hit.point, Quaternion.identity);
                    //    effectInstance.transform.LookAt(hit.point + hit.normal);
                    //    Destroy(effectInstance, 20);

                    var MuzzleFireInstance = Instantiate(MuzzleFire[1], raycastOrigin.position, transform.rotation);
                    Destroy(MuzzleFireInstance, 4);

                    lineRenderer = (LineRenderer)weaponVisuals[1].GetComponent(typeof(LineRenderer));
                    lineRenderer.SetPosition(0, raycastOrigin.position);
                    lineRenderer.enabled = true;


                    lineRenderer.SetPosition(1, LookPos.transform.position);
                    StartCoroutine(LazerReset());

                }*/

            }


            // ApplyRecoil();

        }
        else if (clips > 0)
        {
            Debug.Log("Reloading");
            mechAni.SetBool("shoot", false);
            mechAni.SetBool("reload", true);
            mechAni.SetBool("idle", false);

            StartCoroutine(WaitForReload_Left());
        }
    }

    private void ResetLazer()
    {
        lineRenderer.enabled = false; // Hide the rope when not grappling
    }


    private IEnumerator LazerReset()
    {
        yield return new WaitForSeconds(0.25f);
        ResetLazer();
    }


    void HandleDesktopModeInput(UserInput input)
    {
        isMoving = false;

        if (input.KeyboardMove.y > DM_KEY_TRESHOLD)
        {
            mechAni.SetBool("walkForward", true);
            mechAni.SetBool("walkBackward", false);

            if (!footstep.isPlaying)
            {
                footstep.Play();
            }

            throttle += THROTTLE_FACTOR;
            if (throttle > 1)
                throttle = 1;

            if (direction > 0)
            {
                if (throttle > 0)
                    breaks = 0;
            }
            else
            {
                if (throttle > 0)
                    breaks += THROTTLE_FACTOR;

                if (breaks > 1)
                    breaks = 1;

                if (speedSMA < NEAR_ZERO)
                {
                    throttle = 0;
                    direction = -direction;
                }
            }

            isMoving = true;
        }
        else if (input.KeyboardMove.y < -DM_KEY_TRESHOLD)
        {
            mechAni.SetBool("walkForward", false);
            mechAni.SetBool("walkBackward", true);

            if (!footstep.isPlaying)
            {
                footstep.Play();
            }

            if (direction > 0)
            {
                throttle -= THROTTLE_FACTOR * 2;
                if (throttle < 0)
                    breaks += THROTTLE_FACTOR;

                if (throttle < -1)
                    throttle = -1;

                if (breaks > 1)
                    breaks = 1;

                if (speedSMA < MIN_SPEED_TO_CHANGE_DIRECTION)
                {
                    throttle = 0;
                    direction = -direction;
                }
            }
            else
            {
                throttle -= THROTTLE_FACTOR;
                if (throttle < 0)
                    breaks = 0;

                if (throttle < MAX_REVERSE_THROTTLE)
                    throttle = MAX_REVERSE_THROTTLE;
            }

            isMoving = true;
        }
        else
        {
            throttle -= throttle / 2;
        }

        if (input.KeyboardMove.x > DM_KEY_TRESHOLD)
        {
            mechAni.SetBool("walkLeft", false);
            mechAni.SetBool("walkRight", true);

            if (!footstep.isPlaying)
            {
                footstep.Play();
            }

            steering += STEERING_FACTOR;
            if (steering > 1)
                steering = 1;
            station.transform.Rotate(Vector3.up * 1f * 30 * Time.deltaTime);
            isMoving = true;
        }
        else if (input.KeyboardMove.x < -DM_KEY_TRESHOLD)
        {
            mechAni.SetBool("walkRight", false);
            mechAni.SetBool("walkLeft", true);

            if (!footstep.isPlaying)
            {
                footstep.Play();
            }

            steering -= STEERING_FACTOR;
            if (steering < -1)
                steering = -1;
            station.transform.Rotate(Vector3.up * -1f * 30 * Time.deltaTime);
            isMoving = true;
        }
        else
        {
            steering -= steering / 20;
        }

        if (input.Jump)
        {
            Debug.Log("Jump detected");
        }

        if (input.RightTrigger > DM_KEY_TRESHOLD)
        {
            if (isReloading) return; // Do nothing if reloading
            if (ifWeaponHeld == false) return; //Do Nothing if there is no weapon currently being held.

            if (isAutomatic)
            {
                if (!isFiring)
                {
                    isFiring = true;
                    StartCoroutine(AutomaticFire());
                }
            }
            else
            {
                FireBullet();
            }
        }
        else
        {
            mechAni.SetBool("shoot", false);
            mechAni.SetBool("idle", true);
            if (isAutomatic)
            {
                isFiring = false;
            }
        }

        if (input.LeftTrigger > DM_KEY_TRESHOLD)
        {
            mechAni.SetBool("armswing", true);
            mechAni.SetBool("idle", false);
        }
        else
        {
            mechAni.SetBool("armswing", false);
            mechAni.SetBool("idle", true);
        }

        if (input.CTRL)
        {
            if (ifWeaponHeld == false) return; //Do Nothing if there is no weapon currently being held.
            if (clips < 0) return;
            Debug.Log("Reloading");
            mechAni.SetBool("shoot", false);
            mechAni.SetBool("reload", true);
            mechAni.SetBool("idle", false);

            StartCoroutine(WaitForReload());
        }

        if (input.LeftSprint)
        {
            mechAni.SetBool("stomp", true);
            mechAni.SetBool("idle", false);
        }
        else
        {
            mechAni.SetBool("stomp", false);
            mechAni.SetBool("idle", true);
        }

        if (input.Crouch)
        {
            mechAni.SetBool("stomp", true);
            mechAni.SetBool("idle", false);
        }

        if (!isMoving)
        {
            mechAni.SetBool("walkForward", false);
            mechAni.SetBool("walkBackward", false);
            mechAni.SetBool("walkLeft", false);
            mechAni.SetBool("walkRight", false);
            mechAni.SetBool("idle", true);
        }
        else
        {
            mechAni.SetBool("idle", false);
        }

        MoveStation();
    }

    float steeringTarget = 0;
    void HandleVRModeInput(UserInput input)
    {
        // Implementation for VR mode input handling
        throttle = input.LeftControl.y;

        isMoving = false;
        steeringTarget = input.RightControl.x;
        steering = Mathf.MoveTowards(steering, steeringTarget, 1.5f * Time.deltaTime);

        // Smoothly rotate the mech based on the joystick input
        float rotationSpeed = 60f; // Rotation speed in degrees per second
        if (Mathf.Abs(input.RightControl.x) > DM_KEY_TRESHOLD)
        {
            station.transform.Rotate(Vector3.up * steering * rotationSpeed * Time.deltaTime);
            isMoving = true;
            mechAni.SetBool("walkForward", false);
            mechAni.SetBool("walkBackward", true);
        }
        else
        {
            steering -= steering / 20;
        }

        if (direction > 0)
        {
            if (throttle > 0.0001)
            {
                isMoving = true;
                breaks = 0;

                mechAni.SetBool("walkForward", true);
                mechAni.SetBool("walkBackward", false);

            }
            else
            {
                breaks = Mathf.Abs(throttle);
                throttle = 0;

                if (speedSMA < MIN_SPEED_TO_CHANGE_DIRECTION)
                {
                    direction = -direction;
                }
            }
        }
        else if (direction < 0)
        {
            if (throttle > 0.0001)
            {
                mechAni.SetBool("walkForward", false);
                mechAni.SetBool("walkBackward", true);

                isMoving = true;
                breaks = Mathf.Abs(throttle);
                throttle = 0;
                if (speedSMA > -MIN_SPEED_TO_CHANGE_DIRECTION)
                {
                    direction = -direction;
                }
                else
                {
                    if (throttle < MAX_REVERSE_THROTTLE)
                    {
                        throttle = MAX_REVERSE_THROTTLE;
                    }
                    breaks = 0;
                }
            }

        }

        if (input.RightTrigger > DM_KEY_TRESHOLD)
        {
            Debug.Log("Right VR trigger held");
            if (isReloading) return; // Do nothing if reloading
            if (ifWeaponHeld == false) return; //Do Nothing if there is no weapon currently being held.

            if (isAutomatic)
            {
                if (!isFiring)
                {
                    isFiring = true;
                    StartCoroutine(AutomaticFire());
                }
            }
            else
            {
                FireBullet();
            }
        }
        else
        {
            mechAni.SetBool("shoot", false);
            mechAni.SetBool("idle", true);
            if (isAutomatic)
            {
                isFiring = false;
            }
        }

        //TODO: 

        /*[12:46:48], Error: Object reference not set to an instance of an object.
         at IL Instruction <IL_0131 Ldfld System.Boolean ifWeaponHeld> 
         stack:
         System.Void StationSettler::Update()
        System.Void StationSettler::HandleVRModeInput(ML.SDK.UserInput)
        System.Void StationSettler::Update()
        System.Void StationSettler::HandleVRModeInput(ML.SDK.UserInput)
        System.Void StationSettler::Update()
        System.Void StationSettler::HandleVRModeInput(ML.SDK.UserInput)
        System.Void StationSettler::Update()
        System.Void StationSettler::HandleVRModeInput(ML.SDK.UserInput)
        System.Void StationSettler::Update()

        Fixed. Now I need to make it shoot :)
        */

        if (input.LeftTrigger > DM_KEY_TRESHOLD)
        {
            if (isReloading_left) return; // Do nothing if reloading

            if (secondHandScript.ifWeaponHeld == true)
            {
                //Debug.Log("Second weapon is held");

                if (isAutomatic)
                {
                    if (!isFiring_Left)
                    {
                        isFiring_Left = true;
                        StartCoroutine(AutomaticFire_Left());
                    }
                }
                else
                {
                    FireBulletLeft();
                }

            }
            else if (secondHandScript.ifWeaponHeld == false)
            {
                Debug.Log("SecondWeaponNotHeld");
                return;
            }
        }
        else
        {
            mechAni.SetBool("shoot", false);
            mechAni.SetBool("idle", true);
            if (isAutomatic)
            {
                isFiring_Left = false;
            }
        }

        if (!isMoving)
        {
            mechAni.SetBool("walkForward", false);
            mechAni.SetBool("walkBackward", false);
            mechAni.SetBool("walkLeft", false);
            mechAni.SetBool("walkRight", false);
            mechAni.SetBool("idle", true);
        }
        else
        {
            mechAni.SetBool("idle", false);
        }

    }

    void MoveStation()
    {
        Vector3 moveDirection = station.transform.forward * throttle * MOVE_SPEED * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }

    void FixedUpdate()
    {
        if (underLocalPlayerControl)
        {
            MoveStation();
        }

        /*if (!isMoving)
        {
            Quaternion targetRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, UPRIGHT_LERP_SPEED * Time.deltaTime);
        }*/
    }
}
