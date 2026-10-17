using System;
using UnityEngine;

public class GameSettings
{
    private const string VolumeKey = "settings.masterVolume";
    private const string SensitivityKey = "settings.sensitivity.v2";
    private const string FullscreenKey = "settings.fullscreen";
    private const string HorrorEffectsKey = "settings.horrorEffects";

    public static event Action Changed;

    public const float DefaultVolume = 1f;
    public const float DefaultSensitivity = 1f;
    public const bool DefaultFullscreen = true;
    public const bool DefaultHorrorEffects = true;

    private static float masterVolume = DefaultVolume;
    private static float sensitivityMultiplier = DefaultSensitivity;
    private static bool fullscreen = DefaultFullscreen;
    private static bool horrorEffects = DefaultHorrorEffects;

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

    // Menu fog, grain, flickering light and title glitches (MenuAtmosphere, TextFlicker, MenuButtonFX)
    public static bool HorrorEffects
    {
        get => horrorEffects;
        set
        {
            horrorEffects = value;
            PlayerPrefs.SetInt(HorrorEffectsKey, horrorEffects ? 1 : 0);
            Changed?.Invoke();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Load()
    {
        masterVolume = PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);
        sensitivityMultiplier = Mathf.Clamp(PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity), MinSensitivity, MaxSensitivity);
        fullscreen = PlayerPrefs.GetInt(FullscreenKey, DefaultFullscreen ? 1 : 0) == 1;
        horrorEffects = PlayerPrefs.GetInt(HorrorEffectsKey, DefaultHorrorEffects ? 1 : 0) == 1;

        AudioListener.volume = masterVolume;
        Screen.fullScreen = fullscreen;
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void ResetToDefaults()
    {
        MasterVolume = DefaultVolume;
        SensitivityMultiplier = DefaultSensitivity;
        Fullscreen = DefaultFullscreen;
        HorrorEffects = DefaultHorrorEffects;
        Save();
    }
}
