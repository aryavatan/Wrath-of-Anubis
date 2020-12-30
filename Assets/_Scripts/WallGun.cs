using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGun : MonoBehaviour
{
    [Header("Wall Gun Details")]
    public GameObject gun;
    public int cost;

    [Header("Asset Dependencies")]
    public Transform WeaponHolder;
    public AudioClip InsufficientFundsAudio;

    int ammoCost;
    bool playerInRange = false;
    GameUI ui;
    AudioSource moneySpentAudioSource;
    float insufficientFundsAudioVolume = 0.4f;

    private void Awake()
    {
        ammoCost = cost / 2;
        ui = FindObjectOfType<GameUI>();
        moneySpentAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                PurchaseFromWall();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInRange = true;

            // Check if player has this gun
            bool playerHasThisGun = false;
            foreach (Transform playerGun in WeaponHolder.transform)
            {
                if (playerGun.name.Contains(gun.name))
                {
                    playerHasThisGun = true;
                    break;
                }
            }

            ui.SetWallgunTooltip(true, cost, playerHasThisGun);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInRange = false;
            ui.SetWallgunTooltip(false);
        }
    }

    private void PurchaseFromWall()
    {
        // Check if player already has this gun
        foreach (Transform playerGun in WeaponHolder.transform)
        {
            if (playerGun.name.Contains(gun.name))
            {
                // Check if weapon needs ammo
                bool fullAmmo = playerGun.GetComponent<Gun>().IsAmmoFull();

                // If player has this gun and is not at full ammo, refill their ammo with the ammo cost
                if (!fullAmmo && ui.SubtractMoney(ammoCost))
                {
                    playerGun.GetComponent<Gun>().RefillAmmo();
                    moneySpentAudioSource.Play();
                }
                else
                {
                    // Else play the insufficient funds audio clip
                    moneySpentAudioSource.PlayOneShot(InsufficientFundsAudio, insufficientFundsAudioVolume);
                }

                // Return so player doesn't repurchase the weapon
                return;
            }
        }

        // Purchase the weapon
        if (ui.SubtractMoney(cost))
        {
            moneySpentAudioSource.Play();

            // If Player has empty holster space, add this gun to their inventory
            if (WeaponHolder.childCount < Gun.HolsterCount)
            {
                Gun currentGun = FindObjectOfType<Gun>();
                Transform newGun = Instantiate(gun, WeaponHolder).transform;
                newGun.gameObject.SetActive(false);

                currentGun.SwitchWeapon(true, newGun.GetSiblingIndex());

                // Update UI tooltip to show ammo cost
                ui.SetWallgunTooltip(true, cost, true);
            }
            else
            {
                // Else swap the current gun for this gun
                foreach (Gun gunInInventory in WeaponHolder.GetComponentsInChildren<Gun>())
                {
                    if (gunInInventory.isActiveAndEnabled)
                    {
                        if (gunInInventory.IsReloading())
                            ui.CancelReloadUI();

                        Destroy(gunInInventory.gameObject);
                        Instantiate(gun, WeaponHolder);

                        // Update UI tooltip to show ammo cost
                        ui.SetWallgunTooltip(true, cost, true);

                        break;
                    }
                }

                Gun currentGun = FindObjectOfType<Gun>();
                if (currentGun.IsReloading())
                    ui.CancelReloadUI();

                Destroy(currentGun.gameObject);
                Instantiate(gun, WeaponHolder);

                // Update UI tooltip to show ammo cost
                ui.SetWallgunTooltip(true, cost, true);
            }
        }
        else
        {
            // Else play the insufficient funds audio clip
            moneySpentAudioSource.PlayOneShot(InsufficientFundsAudio, insufficientFundsAudioVolume);
        }
    }
}
