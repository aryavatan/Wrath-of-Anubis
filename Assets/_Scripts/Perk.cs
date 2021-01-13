using UnityEngine;

public class Perk : MonoBehaviour
{
    public PerkType perk;
    public int cost;
    public AudioClip powerUpAudio;
    public AudioClip insufficientFundsAudio;
    public bool perkEnabled = false;

    string perkName;
    string perkDescription;
    GameUI ui;
    AudioSource audioSource;
    Animator anim;
    bool playerInRange = false;
    float insufficientFundsAudioVolume = 0.4f;

    // Player References
    PlayerHealth playerHealth;

    private void Awake()
    {
        ui = FindObjectOfType<GameUI>();
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        playerHealth = FindObjectOfType<PlayerHealth>();
    }

    private void Start()
    {
        SetPerkName();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !perkEnabled)
        {
            PurchasePerk();
        }
    }

    /// <summary>
    /// Sets this objects perk name and description
    /// </summary>
    void SetPerkName()
    {
        switch (perk)
        {
            case PerkType.INCREASED_HEALTH:
                perkName = "Armor";
                perkDescription = "(Increased Health)";
                break;
            case PerkType.INCREASED_DAMAGE:
                perkName = "Commando";
                perkDescription = "(Increased Damage)";
                break;
            case PerkType.SLIGHT_OF_HAND:
                perkName = "Slight of Hand";
                perkDescription = "(Faster Reload and Aiming)";
                break;
            case PerkType.THIRD_GUN:
                perkName = "Extra Holster";
                perkDescription = "(Third Gun Slot)";
                break;
        }
    }

    /// <summary>
    /// Call this method to purchase the perk
    /// </summary>
    void PurchasePerk()
    {
        // First check if player can afford to open the door
        if (ui.SubtractMoney(cost))
        {
            // Play money spent audio
            audioSource.Play();

            playerInRange = false;
            ui.SetPerkTooltip(false);

            // Apply the perk to the player
            EnablePerkEffect();

            // Clean up other unneeded components
            //StartCoroutine(CleanUp());
        }
        else
        {
            audioSource.PlayOneShot(insufficientFundsAudio, insufficientFundsAudioVolume);
        }
    }

    /// <summary>
    /// This method will enable the respective perk effect(s) for this perk
    /// </summary>
    public void EnablePerkEffect(bool playSound = true)
    {
        FindObjectOfType<PerkManager>().AddPerk(perk, ui);

        if (playSound)
            audioSource.PlayOneShot(powerUpAudio);

        perkEnabled = true;

        switch (perk)
        {
            case PerkType.INCREASED_HEALTH:
                EnableIncreasedHealthPerk(100);
                break;
            case PerkType.INCREASED_DAMAGE:
                EnableIncreasedDamagePerk(1f);
                break;
            case PerkType.SLIGHT_OF_HAND:
                EnableSlightOfHandPerk(0.5f, 0.3f);
                break;
            case PerkType.THIRD_GUN:
                EnableThirdGunPerk();
                break;
        }

        // Turn off the trigger box collider
        GetComponent<BoxCollider>().enabled = false;

        // Trigger Perk Purchase Animation
        anim.SetTrigger("Purchase");
    }

    /// <summary>
    /// Enables the Increased Health Perk Effects
    /// </summary>
    void EnableIncreasedHealthPerk(int increase)
    {
        playerHealth.getHealthSlider().maxValue += increase;
        
        playerHealth.maxHealth += increase;
        playerHealth.health += increase;
    }

    /// <summary>
    /// Enables the Increased Damage Perk Effects
    /// </summary>
    /// <param name="percentageIncrease">The percentage of damage increase applied to all weapons</param>
    void EnableIncreasedDamagePerk(float percentageIncrease)
    {
        Gun.damagePerk = 1f + percentageIncrease;

        // Apply the damage buff to the weapon currently in hand
        FindObjectOfType<Gun>().damage *= Gun.damagePerk;
    }

    /// <summary>
    /// Enables the Third Gun Perk Effect
    /// </summary>
    void EnableThirdGunPerk()
    {
        Gun.HolsterCount = 3;
    }

    /// <summary>
    /// Enables the Slight of Hand Perk Effects
    /// </summary>
    /// <param name="reloadPercentageDecrease">The percentage of time which is reduced from all weapon reloads</param>
    /// <param name="AimPercentageDecrease">The percentage of time which is reduced from ADS</param>
    void EnableSlightOfHandPerk(float reloadPercentageDecrease, float AimPercentageDecrease)
    {
        Gun.reloadMultiplier -= reloadPercentageDecrease;
        Gun.AdsMultiplier -= AimPercentageDecrease;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !perkEnabled)
        {
            playerInRange = true;
            ui.SetPerkTooltip(true, cost, perkName, perkDescription);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            ui.SetPerkTooltip(false);
        }
    }
}

public enum PerkType
{
    INCREASED_HEALTH,
    INCREASED_DAMAGE,
    SLIGHT_OF_HAND,
    THIRD_GUN
}