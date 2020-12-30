using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    bool paused = false;

    [Header("Scene Transition")]
    public Animator SceneTransitionAnimator;

    private void Awake()
    {
        pauseMenuPanel.SetActive(false);
    }

    private void Start()
    {
        initOptions();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    void TogglePauseMenu()
    {
        // Toggle Pause
        paused = !paused;

        // Toggle Pause Menu Panel
        pauseMenuPanel.SetActive(paused);
        pauseMenuPanel.transform.GetChild(0).gameObject.SetActive(true);
        pauseMenuPanel.transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(!paused);

        // Toggle Cursor
        Cursor.visible = paused;
        if (paused)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;

        // Toggle other scripts in the scene
        foreach (Gun gun in FindObjectsOfType<Gun>())
            gun.enabled = !paused;
        foreach (WallGun wallgun in FindObjectsOfType<WallGun>())
            wallgun.enabled = !paused;

        // Set Time Scale
        if (paused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;

        // Set Audio Listener pause
        AudioListener.pause = paused;
    }

    public void ResumeGame()
    {
        TogglePauseMenu();
    }

    public void ExitGame()
    {
        Gun.ResetVariables();
        FindObjectOfType<WaveManager>().SaveGameStatistics();
        Time.timeScale = 1f;
        AudioListener.pause = false;

        StartCoroutine(LoadLevel(0));
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Gun.ResetVariables();

        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex));
    }

    IEnumerator LoadLevel(int buildIndex)
    {
        SceneTransitionAnimator.SetTrigger("SceneExit");

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(buildIndex);
    }

    #region OPTIONS

    [Header("Options Objects")]
    public TMP_Dropdown graphicsDropdown;
    public TMP_Dropdown targetFpsDropdown;
    public Toggle vsyncToggle;
    public Slider fovSlider;
    public TextMeshProUGUI fovText;
    public TextMeshProUGUI fovTextGameOver;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;
    public TextMeshProUGUI volumeTextGameOver;

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
            graphicsDropdown.value = graphics;
        }
        OnGraphicsChange(graphics);

        // Init FOV
        int fov = PlayerPrefs.GetInt("FovOption", -1);
        if (fov == -1)
        {
            fov = 60;  // default fov value is 60
            fovSlider.value = fov;
            fovText.SetText(fov.ToString());
            fovTextGameOver.SetText(fov.ToString());
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

    public void OnFovChange(Single value)
    {
        // Update UI
        fovSlider.value = value;
        fovText.SetText(value.ToString());
        fovTextGameOver.SetText(value.ToString());

        // Update Camera and other behaviours
        Camera.main.fieldOfView = value;
        PlayerMovement.UpdateFOV((int)value);
        SniperScope.UpdateFOV((int)value);

        // Save settings
        PlayerPrefs.SetInt("FovOption", (int)value);
        PlayerPrefs.Save();
    }

    public void OnVolumeChange(Single value)
    {
        // Update UI
        volumeSlider.value = value;
        volumeText.SetText(value.ToString() + "%");
        volumeTextGameOver.SetText(value.ToString() + "%");

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

    #endregion
}
