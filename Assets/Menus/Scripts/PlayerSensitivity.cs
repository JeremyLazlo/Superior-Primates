using UnityEngine;
using UnityEngine.SceneManagement;

// Applies the Look Sensitivity option to the player's PlayerAim.
// It adds itself to anything with a PlayerAim when a scene loads, so the Player in SPScene
// doesn't need editing. You can still add it to the Player by hand to tweak Base Scale.
[RequireComponent(typeof(PlayerAim))]
public class PlayerSensitivity : MonoBehaviour
{
    [Tooltip("How fast 1.00x feels. PlayerAim's sensitivities are multiplied by this and by the Options slider.")]
    [SerializeField] private float baseScale = 0.1f;

    private PlayerAim playerAim;
    private float baseNormalSensitivity;
    private float baseAimSensitivity;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ListenForSceneLoads()
    {
        SceneManager.sceneLoaded -= AddToPlayers;
        SceneManager.sceneLoaded += AddToPlayers;
    }

    private static void AddToPlayers(Scene scene, LoadSceneMode mode)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (PlayerAim aim in root.GetComponentsInChildren<PlayerAim>(true))
            {
                if (aim.GetComponent<PlayerSensitivity>() == null)
                {
                    aim.gameObject.AddComponent<PlayerSensitivity>();
                }
            }
        }
    }

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
        float multiplier = GameSettings.SensitivityMultiplier * baseScale;
        playerAim.normalSensitivity = baseNormalSensitivity * multiplier;
        playerAim.aimSensitivity = baseAimSensitivity * multiplier;
    }
}
