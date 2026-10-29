using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Escape pauses the level: freezes time, shows the pause panel and switches off the gameplay scripts.
public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The panel holding the pause buttons. Should start disabled.")]
    [SerializeField] private GameObject pausePanel;

    [Tooltip("The OptionsPanel prefab (a copy is made when the level starts), or an options panel already in the scene.")]
    [SerializeField] private OptionsMenu optionsMenu;

    [Tooltip("Optional: object holding the crosshair / weapon sprite, hidden while paused (e.g. PlayerUI).")]
    [SerializeField] private GameObject hud;

    [Header("Gameplay scripts to switch off while paused")]
    [Tooltip("Leave empty to find MouseLook, PlayerMove, PlayerAim, Gun and InventoryToggle automatically, " +
             "or drag in your own list.")]
    [SerializeField] private MonoBehaviour[] gameplayScripts;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public static bool IsPaused { get; private set; }

    // True while anything is holding gameplay frozen: the pause panel, a story popup, ...
    public static bool IsGameplayFrozen => freezeHolders.Count > 0;

    private static PauseMenu inCharge;

    // Everything currently holding gameplay frozen (this menu while it's open, an open DialoguePopup, ...)
    private static readonly HashSet<object> freezeHolders = new HashSet<object>();

    // Which gameplay scripts were switched on when the game was paused, so Resume only turns those back on
    private bool[] wasEnabled;

    // True while ApplyPauseState(true) is in effect
    private bool frozen;

    // Set when something else (like the open inventory) had already stopped time when the freeze started,
    // so unfreezing hands the game back still stopped, with the cursor free, instead of starting play
    private bool stoppedBeforeFreeze;

    private void Awake()
    {
        bool stale = inCharge == null || inCharge.gameObject.scene != gameObject.scene;
        bool betterSetUp = !stale && inCharge.pausePanel == null && pausePanel != null;
        if (stale || betterSetUp)
        {
            inCharge = this;
        }
    }

    private void Start()
    {
        if (inCharge != this)
        {
            Debug.LogWarning("There are two PauseMenu scripts in this scene. Using the one on '" + inCharge.name +
                             "'. Delete the one on '" + name + "'.", this);
            enabled = false;
            return;
        }

        IsPaused = false;

        if (gameplayScripts == null || gameplayScripts.Length == 0)
        {
            gameplayScripts = FindGameplayScripts();
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        optionsMenu = OptionsMenu.CreateIfPrefab(optionsMenu, pausePanel);
        if (optionsMenu != null)
        {
            optionsMenu.Close();
        }

        // Forget anything still "holding" the game from a level that was unloaded mid-freeze
        freezeHolders.RemoveWhere(IsGone);

        if (IsGameplayFrozen)
        {
            RefreshPauseState(); // something (like a popup) froze gameplay before this menu was ready
        }
        else
        {
            ApplyPauseState(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        // Escape inside Options goes back to the pause buttons instead of closing everything
        if (IsPaused && optionsMenu != null && optionsMenu.IsOpen)
        {
            optionsMenu.Back();
            return;
        }

        // Something else (like the inventory or a story popup) already froze the game, so don't pause on top of it
        if (!IsPaused && (Time.timeScale == 0f || IsGameplayFrozen))
        {
            return;
        }

        TogglePause();
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        IsPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (hud != null)
        {
            hud.SetActive(false);
        }

        FreezeGameplay(this);
    }

    public void Resume()
    {
        IsPaused = false;

        if (optionsMenu != null)
        {
            optionsMenu.Close();
        }
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        if (hud != null)
        {
            hud.SetActive(true);
        }

        UnfreezeGameplay(this); // play only starts again if nothing else (like a popup) is still holding the game
    }

    public void OpenOptions()
    {
        if (optionsMenu != null)
        {
            optionsMenu.Open(pausePanel); // Back in Options returns to the pause panel
        }
    }

    public void LoadMainMenu()
    {
        ClearPauseState();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartLevel()
    {
        ClearPauseState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        ClearPauseState();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // When in Unity Editor
#else
        Application.Quit();
#endif
    }

    // ---------------------------------------------------------------- shared gameplay freeze

    // Freezes gameplay exactly like pausing does (time stopped, sound paused, cursor freed, player scripts off),
    // but without showing the pause panel. Pass the object asking (usually "this") and call
    // UnfreezeGameplay with the same object when it's done. Asking twice with the same object counts once.
    public static void FreezeGameplay(object holder)
    {
        if (holder == null || !freezeHolders.Add(holder))
        {
            return;
        }

        if (inCharge != null)
        {
            inCharge.RefreshPauseState();
        }
        else if (FindAnyObjectByType<PauseMenu>() == null)
        {
            Debug.LogWarning("Gameplay can't be frozen because there's no PauseMenu in this scene. " +
                             "Drag the PauseMenu prefab (Assets/Menus/Prefabs) into the level.");
        }
    }

    // Lets go of a freeze started with FreezeGameplay. Play only starts again once nothing else is holding it.
    public static void UnfreezeGameplay(object holder)
    {
        if (holder == null || !freezeHolders.Remove(holder))
        {
            return;
        }

        if (inCharge != null)
        {
            inCharge.RefreshPauseState();
        }
    }

    // Frozen while anything holds the game, playable once nothing does
    private void RefreshPauseState()
    {
        bool freeze = IsGameplayFrozen;
        if (freeze != frozen)
        {
            ApplyPauseState(freeze);
        }
    }

    private void ApplyPauseState(bool paused)
    {
        if (paused && !frozen)
        {
            stoppedBeforeFreeze = Time.timeScale == 0f; // e.g. the inventory is open
        }
        frozen = paused;

        // Back to normal play, unless something else had already stopped the game before the freeze
        bool play = !paused && !stoppedBeforeFreeze;
        if (!paused)
        {
            stoppedBeforeFreeze = false;
        }

        Time.timeScale = play ? 1f : 0f; // Frozen (or handed back stopped) is 0f, normal play is 1f
        AudioListener.pause = paused;

        Cursor.lockState = play ? CursorLockMode.Locked : CursorLockMode.None; // Unlocking cursor from MouseLook / Locking it back up
        Cursor.visible = !play;

        // A popup can freeze gameplay before Start has run, so find the scripts here if needed
        if (gameplayScripts == null || gameplayScripts.Length == 0)
        {
            gameplayScripts = FindGameplayScripts();
        }

        if (paused)
        {
            // Remember what was on, then switch everything off
            wasEnabled = new bool[gameplayScripts.Length];
            for (int i = 0; i < gameplayScripts.Length; i++)
            {
                if (gameplayScripts[i] != null)
                {
                    wasEnabled[i] = gameplayScripts[i].enabled;
                    gameplayScripts[i].enabled = false;
                }
            }
        }
        else if (wasEnabled != null)
        {
            // Only switch back on what was on before, so scripts another teammate turned off stay off
            for (int i = 0; i < gameplayScripts.Length; i++)
            {
                if (gameplayScripts[i] != null && wasEnabled[i])
                {
                    gameplayScripts[i].enabled = true;
                }
            }
            wasEnabled = null;
        }
    }

    private void ClearPauseState()
    {
        IsPaused = false;
        freezeHolders.Clear(); // leaving the level, so nothing should keep holding the game
        frozen = false;
        stoppedBeforeFreeze = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Used when Gameplay Scripts is left empty (e.g. when the PauseMenu prefab is dropped into a level)
    private static MonoBehaviour[] FindGameplayScripts()
    {
        return new MonoBehaviour[]
        {
            FindAnyObjectByType<MouseLook>(),
            FindAnyObjectByType<PlayerMove>(),
            FindAnyObjectByType<PlayerAim>(),
            FindAnyObjectByType<Gun>(),
            FindAnyObjectByType<InventoryToggle>(),
        };
    }

    private void OnDestroy()
    {
        if (inCharge != this)
        {
            return; // a switched-off duplicate: leave the game's pause state alone
        }

        inCharge = null;
        freezeHolders.Clear();
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    // A holder that was destroyed (for example along with its level) without letting go
    private static bool IsGone(object holder)
    {
        return holder == null || (holder is UnityEngine.Object unityObject && unityObject == null);
    }

    // Static fields survive between Play sessions when domain reload is turned off (Enter Play Mode Options)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetFreezeHolders()
    {
        freezeHolders.Clear();
    }
}
