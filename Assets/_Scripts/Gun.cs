using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Weapon Attributes")]
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
    public float verticleRecoil = 0f;
    public float horizontalRecoil = 0f;
    PlayerMovement playerMovement;

    [Header("Scene Dependencies")]
    public Camera fpsCamera;

    [Header("Assets")]
    public AudioClip EmptyClipAudio;
    public AudioClip ReloadAudio;
    public AudioClip WeaponSwitchAudio;
    public ParticleSystem MuzzleFlash;
    public GameObject bloodImpactEffect;
    public GameObject worldImpactEffect;

    Animator anim;
    protected AudioSource GunshotAudioSource;
    protected float timer;
    GameUI ui;

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
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        reloadTimer += Time.deltaTime;

        bool automaticFire = automatic && ammoInClip > 0 && Input.GetButton("Fire1");
        bool fire = Input.GetButtonDown("Fire1");
        bool reloading = reloadTimer < reloadTime;
        
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
            }
        }
        else if (!fire && timer >= timeBetweenBullets)
        {
            if (automatic) MuzzleFlash.Stop();
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
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
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

        timer = 0f;
    }

    void UpdateAmmoIndicator()
    {
        string ammo = string.Format("{0}/{1}", ammoInClip, ammoInInventory);
        ui.UpdateAmmoCounter(ammo);

        if (ammoInClip == 0 && !ui.IsWallgunActive())
            ui.SetReloadTooltip(true);
        else
            ui.SetReloadTooltip(false);
        
    }

    void Reload()
    {
        GunshotAudioSource.Stop();
        if (automatic) MuzzleFlash.Stop();

        reloadTimer = 0f;
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

        anim.SetTrigger("Reload");
        GunshotAudioSource.PlayOneShot(ReloadAudio, reloadVolume);
        ui.Reload(reloadTime, ammoInClip, ammoInInventory);
        ui.SetReloadTooltip(false);
    }

    public bool IsReloading()
    {
        return reloadTimer < reloadTime;
    }

    void AimDownSights()
    {
        if ((Input.GetButton("Fire2") || Input.GetButtonDown("Fire2")) && !IsReloading())
        {
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

    public void SwitchWeapon(bool forceSwitch = false)
    {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll != 0 || forceSwitch)
        {
            // Block weapon switching mid-reload
            if (reloadTimer < reloadTime) return;

            int gunCount = transform.parent.childCount;
            if (gunCount > 1)
            {
                anim.SetTrigger("Holster");

                for (int i = 0; i < gunCount; i++)
                {
                    Transform obj = transform.parent.GetChild(i);
                    obj.gameObject.SetActive(true);
                    obj.GetComponent<Gun>().DrawWeapon();
                }
                gameObject.SetActive(false);
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
}
