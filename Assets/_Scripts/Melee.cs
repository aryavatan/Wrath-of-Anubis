using MilkShake;
using System.Collections;
using UnityEngine;

public class Melee : MonoBehaviour
{
    [Header("Melee Attributes")]
    public float rateOfFire = 0.25f;
    public float damage = 30f;
    public ShakePreset cameraShake;

    private float timer = 0f;

    [Header("Melee Audio")]
    public AudioClip swordSlashAudio;
    public AudioClip swordImpactAudio;

    [Range(0f,1f)]
    public float swordSlashVolume = 1f;
    [Range(0f,1f)]
    public float swordImpactVolume = 1f;

    [Header("Particle Effects")]
    public GameObject bloodImpactEffect;

    Animator meleeAnim;
    AudioSource audioSource;
    Transform weaponHolder;

    GameObject activeGun;
    PlayerHealth health;

    private void Awake()
    {
        meleeAnim = GetComponent<Animator>();
        timer = rateOfFire;

        health = FindObjectOfType<PlayerHealth>();

        weaponHolder = transform.GetChild(0).Find("WeaponHolder");

        // Initialize Audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.clip = swordSlashAudio;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2) && ReadyToSwing())
        {
            SwingMeleeWeapon();
        }

        timer += Time.deltaTime;
    }

    void SwingMeleeWeapon()
    {
        timer = 0f;
        meleeAnim.SetTrigger("Melee");
        audioSource.PlayOneShot(swordSlashAudio, swordSlashVolume);
        StartCoroutine(EnableGun());
    }

    bool ReadyToSwing()
    {
        if (timer < rateOfFire)
            return false;

        foreach (Gun gun in weaponHolder.GetComponentsInChildren<Gun>())
        {
            if (gun.isActiveAndEnabled)
            {
                if (gun.IsReloading())
                {
                    gun.CancelReload();
                    FindObjectOfType<GameUI>().CancelReload();
                }

                SniperScope scope = gun.GetComponent<SniperScope>();
                if (scope && scope.IsScoped())
                    return false;

                activeGun = gun.transform.gameObject;
                gun.WaitForMelee();
            }
        }

        return true;
    }

    public void AttackEnemy(Collider enemyCollider)
    {
        enemyCollider.GetComponentInParent<EnemyHealth>().TakeDamage(damage);
        audioSource.PlayOneShot(swordImpactAudio, swordImpactVolume);
        Camera.main.GetComponent<Shaker>().Shake(cameraShake);

        // Instantiate the blood impact effect
        GameObject impact = Instantiate(bloodImpactEffect, enemyCollider.ClosestPoint(transform.position), Quaternion.LookRotation(enemyCollider.ClosestPoint(transform.position)));
        Destroy(impact, 1f);
    }

    IEnumerator EnableGun()
    {
        yield return new WaitForSeconds(0.31f);

        if (health.health > 0)
        {
            activeGun.GetComponent<Gun>().MeleeFinished();
        }

    }
}
