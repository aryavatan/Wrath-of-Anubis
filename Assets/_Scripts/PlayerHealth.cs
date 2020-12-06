using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100;
    public bool healthRegeneration = false;
    public int regenRate = 2;
    public float regenCooldown = 5f;
    bool dead = false;

    Slider healthSlider;
    float regenTimer;

    AudioSource playerHurtAudioSource;
    public AudioClip playerDeathAudio;

    [Header("Player Hurt Audio Clips")]
    [Range(0f, 1f)]
    public float playerHurtVolume = 1f;
    public AudioClip ImpactSound;
    public AudioClip[] playerHurtSounds;

    private void Awake()
    {
        healthSlider = FindObjectOfType<Slider>();
        playerHurtAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        CheckDifficulty();
    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = health;
        if (healthRegeneration && regenTimer > regenCooldown && health < 100 && !dead)
        {
            health += regenRate * Time.deltaTime;
            if (health > 100) health = 100;
        }

        regenTimer += Time.deltaTime;
    }

    public void TakeDamage(float damage)
    {
        if (!dead)
        {
            health -= damage;
            PlayPlayerHurtAudio();
            regenTimer = 0f;

            if (health <= 0)
            {
                Die();
            }
        }
    }

    // Player death
    private void Die()
    {
        dead = true;
        GetComponent<PlayerMovement>().enabled = false;
        
        Gun[] guns = FindObjectsOfType<Gun>();
        foreach (Gun gun in guns)
        {
            gun.gameObject.SetActive(false);
        }

        FindObjectOfType<GameUI>().GameOverUI();
        FindObjectOfType<WaveManager>().SaveGameStatistics();
        playerHurtAudioSource.PlayOneShot(playerDeathAudio);

        foreach (EnemyMovement enemy in FindObjectsOfType<EnemyMovement>())
        {
            enemy.PlayerDied();
            enemy.GetComponent<EnemyAttack>().Die();
        }
    }

    void CheckDifficulty()
    {
        int difficulty = PlayerPrefs.GetInt("DifficultyOption", -1);
        if (difficulty == -1)
        {
            difficulty = 1;  // default value is elite gamer
            PlayerPrefs.SetInt("DifficultyOption", difficulty);
            PlayerPrefs.Save();
        }

        if (difficulty == 1)
            healthRegeneration = false;
        else
            healthRegeneration = true;
    }

    private int hurtIndex = 0;
    void PlayPlayerHurtAudio()
    {
        playerHurtAudioSource.PlayOneShot(ImpactSound, playerHurtVolume);

        if (++hurtIndex == playerHurtSounds.Length)
        {
            hurtIndex = 0;
        }

        playerHurtAudioSource.PlayOneShot(playerHurtSounds[hurtIndex], playerHurtVolume);
    }
}
