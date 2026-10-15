using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The panel holding the pause buttons. Should start disabled.")]
    [SerializeField] private GameObject pausePanel;

    [Tooltip("Optional: parent object holding Crosshaid / weapon sprite, hidden while paused.")]
    [SerializeField] private GameObject hud;

    [Header("Gameplay scripts to switch off while paused")]
    [Tooltip("Drag in: MouseLook (Main Camera), PlayerMove + PlayerAim (Player), Gun (Gun).")]
    [SerializeField] private MonoBehaviour[] gameplayScripts;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";


    public static bool IsPaused { get; private set; }

    private void Start()
    {
        IsPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        ApplyPauseState(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
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

        UnityEditor.EditorApplication.isPlaying = false; // When in Unity Editor
        // Application.Quit();
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

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
