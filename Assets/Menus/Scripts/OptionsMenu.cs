using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("The options panel this script shows and hides. Leave empty to use this object.")]
    [SerializeField] private GameObject optionsPanel;

    [Tooltip("The panel to go back to when Back is pressed (main menu or pause panel). " +
             "MainMenu and PauseMenu set this for you when they open Options.")]
    [SerializeField] private GameObject previousPanel;

    [Header("Controls")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeValueLabel;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityValueLabel;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Text fullscreenValueLabel;

    [Tooltip("Turns the menu fog, grain, flickering light and title glitches on or off.")]
    [SerializeField] private Toggle horrorEffectsToggle;
    [SerializeField] private TMP_Text horrorEffectsValueLabel;

    private bool refreshing;

    private GameObject Panel => optionsPanel != null ? optionsPanel : gameObject;

    public bool IsOpen => Panel.activeSelf;

    public static OptionsMenu CreateIfPrefab(OptionsMenu menu, GameObject fromPanel)
    {
        if (menu == null || menu.gameObject.scene.IsValid())
            return menu;

        Transform canvas = fromPanel != null ? fromPanel.transform.parent : null;
        OptionsMenu instance = Instantiate(menu, canvas, false);
        instance.name = menu.name;
        instance.previousPanel = fromPanel;
        instance.Panel.SetActive(false);
        return instance;
    }

    private void OnEnable()
    {
        RefreshControls();
    }

    // Opens Options and remembers which panel to return to
    public void Open(GameObject fromPanel)
    {
        previousPanel = fromPanel;
        Open();
    }

    public void Open()
    {
        if (previousPanel != null)
        {
            previousPanel.SetActive(false);
        }

        Panel.SetActive(true);
        RefreshControls();
    }

    // Hides Options without showing anything else
    public void Close()
    {
        Panel.SetActive(false);
    }

    public void Back()
    {
        GameSettings.Save();
        Panel.SetActive(false);

        if (previousPanel != null)
        {
            previousPanel.SetActive(true);
        }
    }

    public void OnVolumeChanged(float value)
    {
        if (refreshing)
            return;

        GameSettings.MasterVolume = value;
        UpdateLabels();
    }

    public void OnSensitivityChanged(float value)
    {
        if (refreshing)
            return;

        GameSettings.SensitivityMultiplier = value;
        UpdateLabels();
    }

    public void OnFullscreenChanged(bool value)
    {
        if (refreshing)
            return;

        GameSettings.Fullscreen = value;
        UpdateLabels();
    }

    public void OnHorrorEffectsChanged(bool value)
    {
        if (refreshing)
            return;

        GameSettings.HorrorEffects = value;
        UpdateLabels();
    }

    public void OnResetPressed()
    {
        GameSettings.ResetToDefaults();
        RefreshControls();
    }


    // Helper functions

    private void RefreshControls()
    {
        refreshing = true;

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = GameSettings.MinSensitivity;
            sensitivitySlider.maxValue = GameSettings.MaxSensitivity;
            sensitivitySlider.SetValueWithoutNotify(GameSettings.SensitivityMultiplier);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
        }

        if (horrorEffectsToggle != null)
        {
            horrorEffectsToggle.SetIsOnWithoutNotify(GameSettings.HorrorEffects);
        }

        refreshing = false;
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        if (volumeValueLabel != null)
        {
            volumeValueLabel.text = Mathf.RoundToInt(GameSettings.MasterVolume * 100f) + "%";
        }

        if (sensitivityValueLabel != null)
        {
            sensitivityValueLabel.text = GameSettings.SensitivityMultiplier.ToString("0.00") + "x";
        }

        if (fullscreenValueLabel != null)
        {
            fullscreenValueLabel.text = GameSettings.Fullscreen ? "On" : "Off";
        }

        if (horrorEffectsValueLabel != null)
        {
            horrorEffectsValueLabel.text = GameSettings.HorrorEffects ? "On" : "Off";
        }
    }
}
