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
    public TMP_Dropdown targetFpsDropdown;
    public Toggle vsyncToggle;
    public Slider fovSlider;
    public TextMeshProUGUI fovText;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;
    public Slider lookSensitivitySlider;
    public TextMeshProUGUI lookText;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle cheatsToggle;

    [Header("Controls Objects")]
    public GameObject controlsPanel;

    bool controlsToggle = false;

    bool optionsToggle = false;

    [Header("Stats Text Objects")]
    public GameObject statsPanel;
    public TextMeshProUGUI roundsText;
    public TextMeshProUGUI nightmareRoundsText;
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
        
        nightmareRoundsText.text = string.Format("Highest Nightmare Round: {0}", stats.highestNightmareRound);

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
        // Init VSync Setting
        int vsync = PlayerPrefs.GetInt("VSyncOption", 1);
        ToggleVSync(vsync == 1 ? true : false);

        // Init Graphics
        int graphics = PlayerPrefs.GetInt("GraphicsOption", -1);
        if (graphics == -1)
        {
            graphics = 3;  // default value for graphics (ultra)
        }
        OnGraphicsChange(graphics);

        // Init Difficulty
        int difficulty = PlayerPrefs.GetInt("DifficultyOption", 0);
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

        // Init Target FPS
        int targetFPS = PlayerPrefs.GetInt("TargetFPS", 1);
        OnTargetFpsChange(targetFPS);

        // Init Look Sensitivity
        int lookSen = PlayerPrefs.GetInt("LookSensitivity", 5);
        OnLookSensitivityChange(lookSen);

        // Init Fullscreen
        int fullscreen = PlayerPrefs.GetInt("FullscreenOption", 1);
        ToggleFullscreen(fullscreen == 1 ? true : false);

        // Init Resolution
        int resolution = PlayerPrefs.GetInt("ResolutionOption", 1);
        OnResolutionChange(resolution);

        // Init Developer Cheats
        int cheats = PlayerPrefs.GetInt("DeveloperCheats", 0);
        ToggleDeveloperCheats(cheats == 0 ? false : true);
    }

    public void OnGraphicsChange(int graphics)
    {
        // Set quality
        QualitySettings.SetQualityLevel(graphics);

        // Make sure VSync is unchanged
        ToggleVSync(vsyncToggle.isOn);

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

    public void OnTargetFpsChange(int option)
    {
        // Update dropdown
        targetFpsDropdown.value = option;

        // Save setting
        PlayerPrefs.SetInt("TargetFPS", option);
        PlayerPrefs.Save();

        // Set Target FPS
        switch (option)
        {
            case 0:
                Application.targetFrameRate = 30;
                break;
            case 2:
                Application.targetFrameRate = 120;
                break;
            case 3:
                Application.targetFrameRate = -1;
                break;
            case 1:
            default:
                Application.targetFrameRate = 60;
                break;
        }
    }

    public void ToggleVSync(bool state)
    {
        // Update Checkbox
        vsyncToggle.isOn = state;

        // Save setting
        PlayerPrefs.SetInt("VSyncOption", state ? 1 : 0);
        PlayerPrefs.Save();

        // Toggle VSync
        QualitySettings.vSyncCount = state ? 1 : 0;

        // Toggle Target FPS Dropdown
        if (state)
            OnTargetFpsChange(1);
        targetFpsDropdown.interactable = !state;
    }

    public void OnLookSensitivityChange(Single value)
    {
        // Update Slider
        lookSensitivitySlider.value = value;
        lookText.SetText(value.ToString());

        // Save New Setting
        PlayerPrefs.SetInt("LookSensitivity", (int)value);
        PlayerPrefs.Save();

        // Apply New Sensitivity
        PlayerMovement.mouseSensitivity = (float)value / 5f;
    }

    public void OnResolutionChange(int res)
    {
        // Update Dropdown
        resolutionDropdown.value = res;

        // Save Setting
        PlayerPrefs.SetInt("ResolutionOption", res);
        PlayerPrefs.Save();

        int width, height;

        switch (res)
        {
            case 0:
                width = 2560;
                height = 1440;
                break;
            case 2:
                width = 1680;
                height = 1050;
                break;
            case 3:
                width = 1600;
                height = 900;
                break;
            case 4:
                width = 1440;
                height = 900;
                break;
            case 5:
                width = 1366;
                height = 768;
                break;
            case 6:
                width = 1360;
                height = 768;
                break;
            case 7:
                width = 1280;
                height = 720;
                break;
            case 1:
            default:
                width = 1920;
                height = 1080;
                break;
        }

        Screen.SetResolution(width, height, Screen.fullScreen);
    }

    public void ToggleFullscreen(bool state)
    {
        // Update Checkbox
        fullscreenToggle.isOn = state;

        // Save setting
        PlayerPrefs.SetInt("FullscreenOption", state ? 1 : 0);
        PlayerPrefs.Save();

        // Toggle Fullscreen
        Screen.fullScreen = state;
    }

    public void ToggleDeveloperCheats(bool state)
    {
        // Update Checkbox
        cheatsToggle.isOn = state;

        // Save the preference
        PlayerPrefs.SetInt("DeveloperCheats", state == true ? 1 : 0);
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
