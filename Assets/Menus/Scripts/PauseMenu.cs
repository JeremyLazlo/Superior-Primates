using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The panel holding the pause buttons. Should start disabled.")]
    [SerializeField] private GameObject pausePanel;

    [Tooltip("The OptionsPanel prefab (a copy is made when the level starts), or an options panel already in the scene.")]
    [SerializeField] private OptionsMenu optionsMenu;

    [Tooltip("Optional: parent object holding Crosshaid / weapon sprite, hidden while paused.")]
    [SerializeField] private GameObject hud;

    [Header("Gameplay scripts to switch off while paused")]
    [Tooltip("Leave empty to find MouseLook, PlayerMove, PlayerAim, Gun and InventoryToggle automatically, " +
             "or drag in your own list.")]
    [SerializeField] private MonoBehaviour[] gameplayScripts;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";


    public static bool IsPaused { get; private set; }

    private void Start()
    {
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

        ApplyPauseState(false);
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

        // Something else (like the inventory) already froze the game, so don't pause on top of it
        if (!IsPaused && Time.timeScale == 0f)
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

        ApplyPauseState(true);
    }

    public void Resume()
    {
        IsPaused = false;

        if (optionsMenu != null)
        {
            optionsMenu.Close();
        }
        if (pausePanel!= null)
        {
            pausePanel.SetActive(false);
        }
        if (hud != null )
        {
            hud.SetActive(true);
        }

        ApplyPauseState(false);
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

    private void ApplyPauseState(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f; // If paused is true, timeScale is 0f, otherwise it's 1f
        AudioListener.pause = paused;

        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked; // Unlocking cursor from MouseLook / Locking it back up
        Cursor.visible = paused;

        if (gameplayScripts == null)
        {
            return;
        }

        foreach (MonoBehaviour script in gameplayScripts)
        {
            if (script != null)
            {
                script.enabled = !paused;
            }
        }
    }

    private void ClearPauseState()
    {
        IsPaused = false;
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
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
