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
    public Image sniperScope;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI killCounter;

    float reloadTime;
    float time;
    bool forceCrosshairOff = false;

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

        crosshair.SetActive(forceCrosshairOff ? false : !(Input.GetButton("Fire2")));
    }

    public void SetCrosshairSize(int size)
    {
        crosshair.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
    }

    public void Reload(float reloadTime, int clip, int ammoInventory)
    {
        this.reloadTime = reloadTime;
        time = 0f;
        crosshair.SetActive(false);

        string ammo = string.Format("{0}/{1}", clip, ammoInventory);
        StartCoroutine(ReloadAmmoCounter(ammo, reloadTime));
    }

    public void UpdateAmmoCounter(string ammo)
    {
        AmmoCounter.text = ammo;
    }

    IEnumerator ReloadAmmoCounter(string text, float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateAmmoCounter(text);
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

    public void SetSniperScope(bool state)
    {
        sniperScope.gameObject.SetActive(state);
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

    public void SetReloadTooltip(bool state)
    {
        reloadTooltip.SetActive(state);
    }

    public void SetWaveCounter(int wave)
    {
        WaveCounter.text = string.Format("Round: {0}", wave);
    }

    public void SetEndlessModeTimer(float seconds)
    {
        string time = "";

        int minutes = (int)seconds / 60;
        if (minutes > 0)
        {
            time = minutes.ToString();
            seconds = seconds % 60;
            time += ":"; 
        }

        time += seconds.ToString("F2");

        WaveCounter.text = time;
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
