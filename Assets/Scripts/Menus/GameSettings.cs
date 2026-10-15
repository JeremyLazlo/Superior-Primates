using System;
using UnityEngine;

public class GameSettings
{
    private const string VolumeKey = "settings.masterVolume";
    private const string SensitivityKey = "settings.sensitivity";
    private const string FullscreenKey = "settings.fullscreen";

    public static event Action Changed;

    private static float masterVolume = 1f;
    private static float sensitivityMultiplier = 1f;
    private static bool fullscreen = true;

    public const float MinSensitivity = 0.25f;
    public const float MaxSensitivity = 3f;

    public static float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = Mathf.Clamp01(value);
            AudioListener.volume = masterVolume;
            PlayerPrefs.SetFloat(VolumeKey, masterVolume);
            Changed?.Invoke();
        }
    }

    public static float SensitivityMultiplier
    {
        get => sensitivityMultiplier;
        set
        {
            sensitivityMultiplier = Mathf.Clamp(value, MinSensitivity, MaxSensitivity);
            PlayerPrefs.SetFloat(SensitivityKey, sensitivityMultiplier);
            Changed?.Invoke();
        }
    }

    public static bool Fullscreen
    {
        get => fullscreen;
        set
        {
            fullscreen = value;
            Screen.fullScreen = fullscreen;
            PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
            Changed?.Invoke();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Load()
    {
        masterVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        sensitivityMultiplier = PlayerPrefs.GetFloat(SensitivityKey, 1f);
        fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;

        AudioListener.volume = masterVolume;
        Screen.fullScreen = fullscreen;
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void ResetToDefaults()
    {
        MasterVolume = 1f;
        SensitivityMultiplier = 1f;
        Fullscreen = true;
        Save();
    }
}
