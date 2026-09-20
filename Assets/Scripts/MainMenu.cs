using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //
    [Header("Scenes")]
    [Tooltip("Must match the scene file name exactly and be in the Build Profile scene list.")]
    [SerializeField] private string gameSceneName = "SPScene";

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    //


    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowMain();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowMain()
    {
        SetPanel(mainPanel, true);
        SetPanel(optionsPanel, false);
        SetPanel(creditsPanel, false);
    }

    public void ShowOptions()
    {
        SetPanel(mainPanel, false);
        SetPanel(optionsPanel, false);
        SetPanel(creditsPanel, false);
    }

    public void ShowCredits()
    {
        SetPanel(mainPanel, false);
        SetPanel(optionsPanel, false);
        SetPanel(creditsPanel, true);
    }

    public void QuitGame()
    {
        UnityEditor.EditorApplication.isPlaying = false; // When in Unity Editor
        // Application.Quit()
    }

    // Helper function
    private static void SetPanel(GameObject panel, bool visible)
    {
        if (panel != null)
            panel.SetActive(visible);
    }

}
