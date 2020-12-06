using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Play Objects")]
    public GameObject playPanel;
    public Animator sceneTransitionAnimator;

    bool playToggle = false;

    [Header("Options Objects")]
    public GameObject optionsPanel;
    public TMP_Dropdown graphicsDropdown;
    public TMP_Dropdown difficultyDropdown;
    public Slider fovSlider;
    public TextMeshProUGUI fovText;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;

    [Header("Controls Objects")]
    public GameObject controlsPanel;

    bool controlsToggle = false;

    bool optionsToggle = false;

    [Header("Stats Text Objects")]
    public GameObject statsPanel;
    public TextMeshProUGUI roundsText;
    public TextMeshProUGUI killsPerGameText;
    public TextMeshProUGUI totalKillsText;

    bool statsToggle = false;

    private void Start()
    {
        initOptions();
        GetUpdatedStats();
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameAfterTransition());
    }

    IEnumerator QuitGameAfterTransition()
    {
        sceneTransitionAnimator.SetTrigger("SceneExit");

        yield return new WaitForSeconds(0.5f);

        Application.Quit();
    }

    #region PLAY

    public void Play()
    {
        if (optionsToggle)
            ToggleOptionsPanel();
        if (controlsToggle)
            ToggleControlsPanel();
        if (statsToggle)
            ToggleStatsPanel();

        playToggle = !playToggle;
        playPanel.SetActive(playToggle);
    }

    public void LoadDesertLevel()
    {
        StartCoroutine(LoadLevel(2));
    }

    public void LoadPrototypeLevel()
    {
        StartCoroutine(LoadLevel(1));
    }

    IEnumerator LoadLevel(int buildIndex)
    {
        sceneTransitionAnimator.SetTrigger("SceneExit");

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(buildIndex);
    }

    #endregion

    #region STATS

    public void ToggleStatsPanel()
    {
        if (playToggle)
            Play();
        if (controlsToggle)
            ToggleControlsPanel();
        if (optionsToggle)
            ToggleOptionsPanel();
        statsToggle = !statsToggle;
        statsPanel.SetActive(statsToggle);
    }

    void GetUpdatedStats()
    {
        PlayerStatistics stats = PlayerStatistics.Load();

        roundsText.text = string.Format("Highest Round: {0}", stats.highestRound);

        killsPerGameText.text = string.Format("Most Kills in Game: {0}", stats.highestKillsPerGame);

        totalKillsText.text = string.Format("Total Kills: {0}", stats.totalKills);
    }

    public void ResetStats()
    {
        PlayerStatistics.ResetStats();
        GetUpdatedStats();
    }

    #endregion

    #region OPTIONS

    public void ToggleOptionsPanel()
    {
        if (playToggle)
            Play();
        if (controlsToggle)
            ToggleControlsPanel();
        if (statsToggle)
            ToggleStatsPanel();
        optionsToggle = !optionsToggle;
        optionsPanel.SetActive(optionsToggle);

    }

    void initOptions()
    {
        // Init Graphics
        int graphics = PlayerPrefs.GetInt("GraphicsOption", -1);
        if (graphics == -1)
        {
            graphics = 3;  // default value for graphics (ultra)
        }
        OnGraphicsChange(graphics);

        // Init Difficulty
        int difficulty = PlayerPrefs.GetInt("DifficultyOption", -1);
        if (difficulty == -1)
        {
            difficulty = 1;  // default value for difficulty (elite gamer)
        }
        OnDifficultyChange(difficulty);

        // Init FOV
        int fov = PlayerPrefs.GetInt("FovOption", -1);
        if (fov == -1)
        {
            fov = 60;  // default fov value is 60
        }
        OnFovChange(fov);

        // Init Volume
        int volume = PlayerPrefs.GetInt("VolumeOption", -1);
        if (volume == -1)
        {
            volume = 100;
        }
        OnVolumeChange(volume);
    }

    public void OnGraphicsChange(int graphics)
    {
        // Set quality
        QualitySettings.SetQualityLevel(graphics);

        // Update dropdown
        graphicsDropdown.value = graphics;

        // Save setting
        PlayerPrefs.SetInt("GraphicsOption", graphics);
        PlayerPrefs.Save();
    }

    public void OnDifficultyChange(int difficulty)
    {
        // Update dropdown
        difficultyDropdown.value = difficulty;
        
        // Save setting
        PlayerPrefs.SetInt("DifficultyOption", difficulty);
        PlayerPrefs.Save();
    }

    public void OnFovChange(Single value)
    {
        // Update UI
        fovSlider.value = value;
        fovText.SetText(value.ToString());

        // Save settings
        PlayerPrefs.SetInt("FovOption", (int)value);
        PlayerPrefs.Save();
    }

    public void OnVolumeChange(Single value)
    {
        // Update UI
        volumeSlider.value = value;
        volumeText.SetText(value.ToString() + "%");

        // Set volume
        AudioListener.volume = (float)value / 100f;

        // Save settings
        PlayerPrefs.SetInt("VolumeOption", (int)value);
        PlayerPrefs.Save();
    }

    #endregion

    #region CONTROLS

    public void ToggleControlsPanel()
    {
        if (playToggle)
            Play();
        if (optionsToggle)
            ToggleOptionsPanel();
        if (statsToggle)
            ToggleStatsPanel();

        controlsToggle = !controlsToggle;
        controlsPanel.SetActive(controlsToggle);
    }

    #endregion
}
