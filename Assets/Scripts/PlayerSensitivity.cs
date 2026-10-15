using NUnit.Framework.Internal.Commands;
using UnityEngine;

[RequireComponent(typeof(PlayerAim))]
public class PlayerSensitivity : MonoBehaviour
{
    private PlayerAim playerAim;
    private float baseNormalSensitivity;
    private float baseAimSensitivity;

    private void Awake()
    {
        playerAim = GetComponent<PlayerAim>();
        baseNormalSensitivity = playerAim.normalSensitivity;
        baseAimSensitivity = playerAim.aimSensitivity;
    }

    private void OnEnable()
    {
        Apply();
        GameSettings.Changed += Apply;
    }

    private void OnDisable()
    {
        GameSettings.Changed -= Apply;
    }

    private void Apply()
    {
        float multiplier = GameSettings.SensitivityMultiplier;
        playerAim.normalSensitivity = baseNormalSensitivity * multiplier;
        playerAim.aimSensitivity = baseAimSensitivity * multiplier;
    }
}
