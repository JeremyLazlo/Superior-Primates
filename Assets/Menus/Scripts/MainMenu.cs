using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [Header("Scenes")]
    [Tooltip("Must match the scene file name exactly and be in the Build Profile scene list.")]
    [SerializeField] private string gameSceneName = "SPScene";

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;

    [Tooltip("The OptionsPanel prefab (a copy is made when the menu starts), or an options panel already in this scene.")]
    [SerializeField] private OptionsMenu optionsMenu;

    [SerializeField] private GameObject creditsPanel;



    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        optionsMenu = OptionsMenu.CreateIfPrefab(optionsMenu, mainPanel);

        ShowMain();
    }

    private void Update()
    {
        // Escape backs out of Options or Credits
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        if (optionsMenu != null && optionsMenu.IsOpen)
        {
            optionsMenu.Back();
        }
        else if (creditsPanel != null && creditsPanel.activeSelf)
        {
            ShowMain();
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowMain()
    {
        if (optionsMenu != null)
        {
            optionsMenu.Close();
        }
        SetPanel(creditsPanel, false);
        SetPanel(mainPanel, true);
    }

    public void ShowOptions()
    {
        SetPanel(creditsPanel, false);

        if (optionsMenu != null)
        {
            optionsMenu.Open(mainPanel); // Back in Options returns to the main panel
        }
    }

    public void ShowCredits()
    {
        if (optionsMenu != null)
        {
            optionsMenu.Close();
        }
        SetPanel(mainPanel, false);
        SetPanel(creditsPanel, true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // When in Unity Editor
#else
        Application.Quit();
#endif
    }

    // Helper function
    private static void SetPanel(GameObject panel, bool visible)
    {
        if (panel != null)
            panel.SetActive(visible);
    }

}
