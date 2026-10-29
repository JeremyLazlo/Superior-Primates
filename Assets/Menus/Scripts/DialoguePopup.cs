using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Story popup: shows a message over the game, freezes gameplay while it's open and closes when the
// player clicks Continue. Lives on the root of the DialoguePopup prefab (one copy in each level).
//
// Any script can open one:
//     DialoguePopup.Show("I need to find a way out of here.");
//     DialoguePopup.Show("It won't open. There has to be a key somewhere.", "Locked");   // with a heading
//     DialoguePopup.ShowOnce("first-key", "A key. Finally.");    // only the first time this play session
//
// Freezing goes through PauseMenu.FreezeGameplay, so a popup stops exactly what Escape stops, and closing it
// never starts the game again while the pause menu or another popup still has it frozen.
// Popups asked for while one is already open wait their turn and show one after another.
public class DialoguePopup : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Backdrop, text and button, shown while a popup is open. Should start disabled.")]
    [SerializeField] private GameObject popupPanel;

    [Tooltip("Heading above the message. Hidden for popups without a title.")]
    [SerializeField] private TMP_Text titleText;

    [Tooltip("Shown and hidden along with the title (the red line under it).")]
    [SerializeField] private GameObject titleLine;

    [SerializeField] private TMP_Text messageText;

    [Tooltip("Its On Click calls DialoguePopup.Continue.")]
    [SerializeField] private Button continueButton;

    [Header("Timing (real seconds, so it runs while the game is frozen)")]
    [SerializeField] private float fadeInTime = 0.4f;

    [Tooltip("Continue can't be pressed before this, so a click or key meant for the game doesn't skip the text.")]
    [SerializeField] private float continueDelay = 0.75f;

    [Header("Layout")]
    [Tooltip("Messages taller than this shrink to fit instead of running off the screen.")]
    [SerializeField] private float maxMessageHeight = 560f;

    private const float ButtonFadeTime = 0.25f;

    // True while a popup is on screen
    public static bool IsOpen => instance != null && instance.current != null;

    private static DialoguePopup instance;

    // IDs given to ShowOnce this play session. Kept when a level reloads, cleared when the game starts again.
    private static readonly HashSet<string> shownIds = new HashSet<string>();

    private class Message
    {
        public string text;
        public string title;
        public Action onClosed;
    }

    private readonly Queue<Message> waiting = new Queue<Message>();
    private Message current;
    private float openedAt;
    private float messageFontSize;
    private CanvasGroup panelGroup;
    private CanvasGroup buttonGroup;
    private bool isSetUp;


    // ---------------------------------------------------------------- for other scripts

    // Shows a popup with this message and an optional heading. onClosed (optional) runs after Continue is clicked.
    public static void Show(string message, string title = null, Action onClosed = null)
    {
        TryShow(message, title, onClosed);
    }

    // Like Show, but only the first time it's asked for with this id during a play session,
    // so dying or restarting the level doesn't show it again. Returns true if it was shown.
    public static bool ShowOnce(string id, string message, string title = null, Action onClosed = null)
    {
        if (shownIds.Contains(id) || !TryShow(message, title, onClosed))
        {
            return false;
        }

        shownIds.Add(id);
        return true;
    }

    // Lets every ShowOnce popup (like the intro) show again, e.g. when starting a new game from the main menu
    public static void ForgetShownPopups()
    {
        shownIds.Clear();
    }

    // Continue button: closes this popup, then shows the next one waiting (if any)
    public void Continue()
    {
        if (current == null)
        {
            return;
        }

        Message finished = current;
        current = null;

        if (waiting.Count > 0)
        {
            Open(waiting.Dequeue()); // gameplay stays frozen in between
        }
        else
        {
            Close();
        }

        finished.onClosed?.Invoke();
    }


    // ---------------------------------------------------------------- showing and hiding

    private static bool TryShow(string message, string title, Action onClosed)
    {
        if (instance == null)
        {
            instance = FindAnyObjectByType<DialoguePopup>(); // e.g. asked for before this popup's Awake ran
        }

        if (instance == null)
        {
            Debug.LogWarning("There's no DialoguePopup in this scene, so this popup wasn't shown: \"" + message + "\". " +
                             "Drag the DialoguePopup prefab (Assets/Menus/Prefabs) into the level.");
            return false;
        }

        instance.Add(new Message { text = message, title = title, onClosed = onClosed });
        return true;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("There are two DialoguePopups in this scene. Using the one on '" + instance.name +
                             "'. Delete the one on '" + name + "'.", this);
        }

        SetUp();

        // Hidden until a popup is shown (unless another script's Awake already opened one)
        if (current == null && popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private void SetUp()
    {
        if (isSetUp)
        {
            return;
        }
        isSetUp = true;

        if (popupPanel == null || messageText == null || continueButton == null)
        {
            Debug.LogError("DialoguePopup is missing its Popup Panel, Message Text or Continue Button.", this);
        }

        // Fading is done with CanvasGroups, added here so the prefab doesn't need them
        if (popupPanel != null)
        {
            panelGroup = GetOrAddCanvasGroup(popupPanel);
        }
        if (continueButton != null)
        {
            buttonGroup = GetOrAddCanvasGroup(continueButton.gameObject);
        }
        if (messageText != null)
        {
            messageFontSize = messageText.fontSize;
        }
    }

    private void Add(Message message)
    {
        if (current != null)
        {
            waiting.Enqueue(message); // shown after the ones before it
            return;
        }

        Open(message);
    }

    private void Open(Message message)
    {
        SetUp();
        if (popupPanel == null)
        {
            return;
        }

        bool alreadyShowing = popupPanel.activeSelf; // one popup straight after another: no fade, no flicker
        current = message;
        openedAt = Time.unscaledTime;

        PauseMenu.FreezeGameplay(this); // already held if this popup follows another

        popupPanel.SetActive(true);
        ShowTitle(message.title);
        ShowMessage(message.text);

        if (panelGroup != null && !alreadyShowing)
        {
            panelGroup.alpha = fadeInTime > 0f ? 0f : 1f;
        }
        if (continueButton != null)
        {
            continueButton.interactable = false; // turned on in Update after continueDelay
        }
        if (buttonGroup != null)
        {
            buttonGroup.alpha = 0f;
        }
    }

    private void Close()
    {
        if (continueButton != null && EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == continueButton.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        PauseMenu.UnfreezeGameplay(this);
    }

    private void ShowTitle(string title)
    {
        bool hasTitle = !string.IsNullOrEmpty(title);

        if (titleText != null)
        {
            titleText.text = hasTitle ? title : string.Empty;
            titleText.gameObject.SetActive(hasTitle);
        }
        if (titleLine != null)
        {
            titleLine.SetActive(hasTitle);
        }
    }

    private void ShowMessage(string text)
    {
        if (messageText == null)
        {
            return;
        }
        text = text ?? string.Empty;

        messageText.enableAutoSizing = false;
        messageText.fontSize = messageFontSize;
        messageText.text = text;

        // Make the text box as tall as the message, and the layout group stacks the title and button around it.
        // A message too long for the screen shrinks to fit instead.
        RectTransform box = messageText.rectTransform;
        float height = messageText.GetPreferredValues(text, box.rect.width, 0f).y;
        if (height > maxMessageHeight)
        {
            height = maxMessageHeight;
            messageText.fontSizeMax = messageFontSize;
            messageText.enableAutoSizing = true;
        }
        box.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        if (box.parent is RectTransform layoutRoot)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        }
    }

    // Fades the popup in and turns Continue on once it's been up for a moment. Unscaled time: the game is frozen.
    private void Update()
    {
        if (current == null)
        {
            return;
        }

        float shownFor = Time.unscaledTime - openedAt;

        if (panelGroup != null && panelGroup.alpha < 1f)
        {
            panelGroup.alpha = fadeInTime > 0f ? Mathf.Clamp01(shownFor / fadeInTime) : 1f;
        }

        if (shownFor < continueDelay)
        {
            return;
        }

        if (buttonGroup != null && buttonGroup.alpha < 1f)
        {
            buttonGroup.alpha = Mathf.Clamp01((shownFor - continueDelay) / ButtonFadeTime);
        }

        if (continueButton != null && !continueButton.interactable)
        {
            continueButton.interactable = true;

            // Selected, so Enter or Space press Continue as well as the mouse
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
            }
        }
    }

    private void OnEnable()
    {
        if (current != null)
        {
            PauseMenu.FreezeGameplay(this); // switched back on with a popup still open
        }
    }

    // Switched off or unloaded with a popup open: don't leave the game frozen
    private void OnDisable()
    {
        PauseMenu.UnfreezeGameplay(this);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        return group != null ? group : target.AddComponent<CanvasGroup>();
    }

    // Static fields survive between Play sessions when domain reload is turned off (Enter Play Mode Options),
    // so start every session with no popups marked as shown
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        instance = null;
        shownIds.Clear();
    }
}
