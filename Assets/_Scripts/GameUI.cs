using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("In-Game UI")]
    public Image reloadProgress;
    public GameObject crosshair;
    public TextMeshProUGUI WaveCounter;
    public TextMeshProUGUI AmmoCounter;
    public TextMeshProUGUI moneyCounter;
    public GameObject reloadTooltip;
    public GameObject wallGunTooltip;
    public GameObject gateTooltip;
    public GameObject perkTooltip;
    public Image slownessDebuffProgress;
    public GameObject[] perks;
    public Image sniperScope;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI killCounter;

    float reloadTime;
    float time;
    bool forceCrosshairOff = false;

    // Debuff Variables
    float debuffDuration = 0f;
    float debuffTimer = 0f;

    // Update is called once per frame
    void Update()
    {
        if (time < reloadTime)
        {
            time += Time.deltaTime;
            reloadProgress.fillAmount = time / reloadTime;
            forceCrosshairOff = true;
        }
        else
        {
            reloadProgress.fillAmount = 0;
            crosshair.SetActive(true);
            forceCrosshairOff = false;
        }

        if (debuffTimer < debuffDuration)
        {
            debuffTimer += Time.deltaTime;
            slownessDebuffProgress.fillAmount = 1 - (debuffTimer / debuffDuration);
        }
        else
        {
            slownessDebuffProgress.fillAmount = 0;
            slownessDebuffProgress.gameObject.SetActive(false);
        }

        crosshair.SetActive(forceCrosshairOff ? false : !(Input.GetButton("Fire2")));
    }

    public void EnableSlownessDebuffUI(float duration)
    {
        debuffDuration = duration;
        debuffTimer = 0f;
        slownessDebuffProgress.gameObject.SetActive(true);
    }

    public void SetCrosshairSize(int size)
    {
        crosshair.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
    }

    public void Reload(float reloadTime, int clip, int ammoInventory, int clipSize)
    {
        this.reloadTime = reloadTime;
        time = 0f;
        crosshair.SetActive(false);

        StartCoroutine(ReloadAmmoCounter(reloadTime, clip,  ammoInventory,  clipSize));
    }

    private bool cancelReload = false;
    public void CancelReload()
    {
        cancelReload = true;
        CancelReloadUI();
        crosshair.SetActive(true);
    }

    public void UpdateAmmoCounter(string ammo)
    {
        AmmoCounter.text = ammo;
    }

    IEnumerator ReloadAmmoCounter(float delay, int ammoInClip, int ammoInInventory, int clipSize)
    {
        yield return new WaitForSeconds(delay);

        if (!cancelReload)
        {
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
            
            string ammoText = string.Format("{0}/{1}", ammoInClip, ammoInInventory);
            UpdateAmmoCounter(ammoText);
        }
        else
        {
            cancelReload = false;
        }
    }

    public void CancelReloadUI()
    {
        reloadTime = 0f;
    }

    public void AddMoney(int amount)
    {
        int currentMoney = int.Parse(moneyCounter.text);
        currentMoney += amount;
        moneyCounter.text = currentMoney.ToString();
    }

    public bool SubtractMoney(int amount)
    {
        int currentMoney = int.Parse(moneyCounter.text);

        if (currentMoney < amount)
        {
            return false;
        }

        currentMoney -= amount;
        moneyCounter.text = currentMoney.ToString();
        return true;
    }

    public void SetSniperScope(bool state, Sprite customOverlay = null)
    {
        sniperScope.gameObject.SetActive(state);
        if (customOverlay)
            sniperScope.sprite = customOverlay;
    }

    public void SetWallgunTooltip(bool state, int cost = 0, bool ammoPurchase = false)
    {
        if (cost > 0)
        {
            string tooltip = "${0} - {1}";

            if (!ammoPurchase)
            {
                tooltip = string.Format(tooltip, cost, "Buy Weapon");
            }
            else
            {
                tooltip = string.Format(tooltip, cost/2, "Refill Ammo");
            }

            wallGunTooltip.GetComponentInChildren<TextMeshProUGUI>().text = tooltip;
        }

        wallGunTooltip.SetActive(state);
    }

    public bool IsWallgunActive()
    {
        return wallGunTooltip.activeSelf;
    }

    public bool IsGateActive()
    {
        return gateTooltip.activeSelf;
    }

    public bool IsPerkActive()
    {
        return perkTooltip.activeSelf;
    }

    public void SetGateTooltip(bool state, int cost = 1000)
    {
        if (state)
        {
            string tooltip = string.Format("${0} - Open Gate", cost);
            gateTooltip.GetComponentInChildren<TextMeshProUGUI>().text = tooltip;
        }
        gateTooltip.SetActive(state);
    }
    
    public void SetPerkTooltip(bool state, int cost = 0, string name = null, string description = null)
    {
        if (state)
        {
            string tooltip = string.Format("${0} - {1}", cost, name);

            TextMeshProUGUI[] perkText = perkTooltip.GetComponentsInChildren<TextMeshProUGUI>();
            perkText[0].text = tooltip;
            perkText[1].text = description;
        }
        perkTooltip.SetActive(state);
    }

    public void SetReloadTooltip(bool state)
    {
        reloadTooltip.SetActive(state);
    }

    public void SetWaveCounter(int wave)
    {
        WaveCounter.text = string.Format("Round: {0}", wave);
    }

    public void AddPerk(Sprite perkSprite)
    {
        for (int i = 0; i < perks.Length; i++)
        {
            if (!perks[i].activeSelf)
            {
                perks[i].GetComponent<Image>().sprite = perkSprite;
                perks[i].SetActive(true);
                return;
            }
        }
    }

    #region Game Over Code

    public void GameOverUI()
    {
        // Disable In-Game UI and Pause Menu
        crosshair.transform.parent.gameObject.SetActive(false);
        GetComponent<PauseMenu>().enabled = false;

        // Enable Game Over Panel
        gameOverPanel.SetActive(true);

        // Populate Kill Counter
        int kills = FindObjectOfType<WaveManager>().GetPlayerKills();
        killCounter.text = string.Format("Kills: {0}", kills);

        // Enable Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #endregion

}
