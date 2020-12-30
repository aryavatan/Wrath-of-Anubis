using MilkShake;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Weapon Attributes")]
    public float baseDamage = 10f;
    public float damage = 10f;
    public float range = 100f;
    public float timeBetweenBullets = 0.15f;
    public int accuracy = 30;
    public bool automatic = false;
    public float impactForce = 30f;
    public float reloadTime = 2.21f;
    [Range(0f, 1f)]
    public float reloadVolume = 1f;

    public int clipSize;
    public int maxAmmoCapacity;
    
    protected int ammoInClip;
    private int ammoInInventory;
    private float reloadTimer;

    [Header("Recoil")]
    public ShakePreset cameraShake;
    public float verticleRecoil = 0f;
    public float horizontalRecoil = 0f;
    PlayerMovement playerMovement;

    [Header("Scene Dependencies")]
    public Camera fpsCamera;

    [Header("Assets")]
    public AudioClip EmptyClipAudio;
    public AudioClip ReloadAudio;
    public AudioClip FastReloadAudio;
    public AudioClip WeaponSwitchAudio;
    public ParticleSystem MuzzleFlash;
    public GameObject bloodImpactEffect;
    public GameObject worldImpactEffect;

    [Header("Optional")]
    public LineRenderer gunLine;
    public Transform gunBarrel;
    float effectsDisplayTime = 0.1f;

    Animator anim;
    protected AudioSource GunshotAudioSource;
    protected float timer;
    GameUI ui;

    #region Static Variables
    
    // Static Variables
    public static float damagePerk = 1f;
    public static int HolsterCount = 2;
    public static float reloadMultiplier = 1f;
    public static float AdsMultiplier = 1f;

    public static void ResetVariables()
    {
        damagePerk = 1f;
        HolsterCount = 2;
        reloadMultiplier = 1f;
        AdsMultiplier = 1f;
    }

    #endregion

    private void Awake()
    {
        GunshotAudioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        ammoInClip = clipSize;
        ammoInInventory = maxAmmoCapacity;
        reloadTimer = reloadTime;

        fpsCamera = transform.parent.parent.GetChild(0).GetComponent<Camera>();

        timer = timeBetweenBullets;

        playerMovement = FindObjectOfType<PlayerMovement>();
        ui = FindObjectOfType<GameUI>();
    }

    private void OnEnable()
    {
        ui.SetCrosshairSize(accuracy);

        // If player has gotten the damage perk, apply the additional damage
        damage = baseDamage * damagePerk;
    }

    private void OnDisable()
    {
        anim.SetBool("Aim", false);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (isMeleeAttacking)
        {
            if (automatic) MuzzleFlash.Stop();
            return;
        }

        reloadTimer += Time.deltaTime;

        bool automaticFire = automatic && ammoInClip > 0 && Input.GetButton("Fire1");
        bool fire = Input.GetButtonDown("Fire1");
        bool reloading = reloadTimer < (reloadTime * reloadMultiplier);
        
        if (timer >= timeBetweenBullets && (fire || automaticFire) && !reloading)
        {
            // Check if ammo in clip
            if (ammoInClip > 0)
            {
                Shoot();
                playerMovement.Recoil(verticleRecoil, horizontalRecoil);
            }
            else
            {
                GunshotAudioSource.PlayOneShot(EmptyClipAudio);
                if (ammoInInventory > 0)
                    Reload();
            }
        }
        else if (!fire && timer >= timeBetweenBullets)
        {
            if (automatic) MuzzleFlash.Stop();
        }

        if (timer > timeBetweenBullets * effectsDisplayTime)
        {
            DisableEffects();
        }

        if (ammoInClip < clipSize && !reloading && Input.GetKeyDown(KeyCode.R))
        {
            if (ammoInInventory == 0)
            {
                GunshotAudioSource.PlayOneShot(EmptyClipAudio);
            }
            else Reload();
        }

        if (!reloading)
        {
            SwitchWeapon();
            UpdateAmmoIndicator();
            AimDownSights();
        }

        // Changing weapons manually using number keys
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown("[1]"))
            SwitchWeapon(true, 0);
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown("[2]"))
            SwitchWeapon(true, 1);
        else if (HolsterCount == 3 && (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown("[3]")))
            SwitchWeapon(true, 2);
    }

    public virtual void Shoot()
    {
        GunshotAudioSource.Play();
        MuzzleFlash.Play();

        ammoInClip--;
        if (ammoInClip == 0 && automatic)
        {
            GunshotAudioSource.PlayOneShot(EmptyClipAudio);
        }

        RaycastHit hit;
        Vector3 origin = fpsCamera.transform.position;
        Vector3 direction = fpsCamera.transform.forward;
        if (Physics.Raycast(origin, direction, out hit, range))
        {
            EnemyHealth enemy = hit.transform.GetComponentInParent<EnemyHealth>();

            if(enemy != null)
            {
                // Apply damage to enemy, if headshot apply double damage
                bool lethalAttack =  enemy.TakeDamage(damage);
                if (hit.transform.name.Contains("Head"))
                    lethalAttack = enemy.TakeDamage(damage);

                // Instantiate the blood impact effect
                GameObject impact = Instantiate(bloodImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 1f);

                if(hit.rigidbody != null || lethalAttack)
                {
                    hit.rigidbody.AddForce(-hit.normal * impactForce);
                }
            }
            else
            {
                GameObject impact = Instantiate(worldImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 1f);
            }
        }
        
        Camera.main.GetComponent<Shaker>().Shake(cameraShake);

        DrawGunLine(hit, origin, direction);
        timer = 0f;
    }

    void UpdateAmmoIndicator()
    {
        if (IsReloading())
            return;

        string ammo = string.Format("{0}/{1}", ammoInClip, ammoInInventory);
        ui.UpdateAmmoCounter(ammo);

        if (ammoInClip == 0 && ammoInInventory > 0 && !ui.IsWallgunActive() && !ui.IsGateActive() && !ui.IsPerkActive())
            ui.SetReloadTooltip(true);
        else
            ui.SetReloadTooltip(false);
        
    }

    void Reload()
    {
        GunshotAudioSource.Stop();
        if (automatic) MuzzleFlash.Stop();

        reloadTimer = 0f;

        StartCoroutine(UpdateAmmoAfterReload());

        if (reloadMultiplier != 1f)
        {
            anim.speed = 1f / reloadMultiplier;
            anim.SetBool("FastReload", true);
            GunshotAudioSource.PlayOneShot(FastReloadAudio, reloadVolume);
        }
        else
        {
            GunshotAudioSource.PlayOneShot(ReloadAudio, reloadVolume);
        }
        
        anim.SetTrigger("Reload");
        
        ui.Reload(reloadTime * reloadMultiplier, ammoInClip, ammoInInventory, clipSize);
        ui.SetReloadTooltip(false);
    }

    IEnumerator UpdateAmmoAfterReload()
    {
        yield return new WaitForSeconds(reloadTime * reloadMultiplier);

        anim.speed = 1f;

        ammoInInventory += ammoInClip;
        ammoInClip = 0;

        if (ammoInInventory >= clipSize)
        {
            ammoInInventory -= clipSize;
            ammoInClip = clipSize;
        }
        else
        {
            ammoInClip = ammoInInventory;
            ammoInInventory = 0;
        }

        UpdateAmmoIndicator();
    }

    public bool IsReloading()
    {
        return reloadTimer < (reloadTime * reloadMultiplier);
    }

    public void CancelReload()
    {
        anim.speed = 1f;
        reloadTimer = reloadTime;
    }

    void AimDownSights()
    {
        if ((Input.GetButton("Fire2") || Input.GetButtonDown("Fire2")) && !IsReloading())
        {
            anim.speed = 1f / AdsMultiplier;

            anim.SetBool("Aim", true);

            // Slow down player's max speed when aiming
            FindObjectOfType<PlayerMovement>().movementMultiplier = 0.4f;
        }
        else
        {
            anim.SetBool("Aim", false);

            // Reset player's max speed when not aiming
            FindObjectOfType<PlayerMovement>().movementMultiplier = 1f;
        }
    }

    public void SwitchWeapon(bool forceSwitch = false, int newGunIndex = -1)
    {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");

        if (scroll != 0 || forceSwitch)
        {
            // Block weapon switching mid-reload
            if (reloadTimer < (reloadTime * reloadMultiplier)) return;

            int gunCount = transform.parent.childCount;
            if (gunCount > 1)
            {
                int currentGunIndex = transform.GetSiblingIndex();
                int nextWeapon;

                if (forceSwitch)
                {
                    if (newGunIndex > gunCount - 1 || newGunIndex == currentGunIndex)
                        return;
                    nextWeapon = newGunIndex;
                }
                else if (scroll > 0)
                    nextWeapon = currentGunIndex + 1 > gunCount - 1 ? 0 : currentGunIndex + 1;
                else
                    nextWeapon = currentGunIndex - 1 < 0 ? gunCount - 1 : currentGunIndex - 1;

                anim.SetTrigger("Holster");

                for (int i = 0; i < gunCount; i++)
                {
                    if (i == nextWeapon)
                    {
                        Transform obj = transform.parent.GetChild(i);
                        obj.gameObject.SetActive(true);
                        obj.GetComponent<Gun>().DrawWeapon();
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                }
                
            }
        }
    }

    // Plays the weapon draw audio clip
    public void DrawWeapon()
    {
        GunshotAudioSource.PlayOneShot(WeaponSwitchAudio, 0.5f);
    }

    // Refills the player's ammo in inventory (NOT their clip ammo)
    public void RefillAmmo()
    {
        ammoInInventory = maxAmmoCapacity;
    }

    // Checks if the ammo in inventory is equal to the max ammo capacity
    public bool IsAmmoFull()
    {
        return (ammoInInventory == maxAmmoCapacity);
    }

    // Handles the gun line, if one is provided for this gun
    void DrawGunLine(RaycastHit shootHit, Vector3 origin, Vector3 direction)
    {
        if (gunLine)
        {
            gunLine.enabled = true;
            gunLine.SetPosition(0, gunBarrel.position);

            if (shootHit.point != new Vector3(0f,0f,0f))
            {
                gunLine.startWidth = 0.05f;
                gunLine.endWidth = 0.05f;
                gunLine.SetPosition(1, shootHit.point);
            }
            else
            {
                gunLine.startWidth = 0.05f;
                gunLine.endWidth = 0.05f;
                gunLine.SetPosition(1, origin + direction * range);
            }
        }
    }

    void DisableEffects()
    {
        if (gunLine)
        {
            gunLine.enabled = false;
        }
    }

    bool isMeleeAttacking = false;
    public void WaitForMelee()
    {
        isMeleeAttacking = true;
        foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>())
        {
            mesh.enabled = false;
        }
    }
    public void MeleeFinished()
    {
        foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>())
        {
            mesh.enabled = true;
        }
        isMeleeAttacking = false;
    }
}
