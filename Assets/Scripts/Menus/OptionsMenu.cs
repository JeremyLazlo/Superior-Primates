using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("The options panel this script shows and hides.")]
    [SerializeField] private GameObject optionsPanel;

    [Tooltip("The panel to go back to when Back is pressed (main menu or pause panel).")]
    [SerializeField] private GameObject previousPanel;

    [Header("Controls")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeValueLabel;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityValueLabel;
    [SerializeField] private Toggle fullscreenToggle;

    private void OnEnable()
    {
        RefreshControls();
    }

    public void Open()
    {
        if (previousPanel != null)
        {
            previousPanel.SetActive(false);
        }
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
        RefreshControls();
    }

    public void Back()
    {
        GameSettings.Save();
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
        if (previousPanel != null)
        {
            previousPanel.SetActive(true);
        }
    }

    public void OnVolumeChanged(float value)
    {
        GameSettings.MasterVolume = value;
        UpdateLabels();
    }

    public void OnSensitivityChanged(float value)
    {
        GameSettings.SensitivityMultiplier = value;
        UpdateLabels();
    }

    public void OnFullscreenChanged(bool value)
    {
        GameSettings.Fullscreen = value;
    }

    public void OnResetPressed()
    {
        GameSettings.ResetToDefaults();
        RefreshControls();
    }


    // Helper functions

    private void RefreshControls()
    {
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
    }
}
