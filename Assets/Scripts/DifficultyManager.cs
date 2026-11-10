using UnityEngine;

// adaptive difficulty: quietly watches how the player is doing and makes the enemies a little easier or harder to match
// it creates itself when the game starts, so nothing has to be added to the scene
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Current Difficulty (0 = easiest, 1 = hardest, starts in the middle)")]
    [Range(0f, 1f)] public float difficulty = 0.5f;

    [Header("How the player is judged")]
    public float damageMemorySeconds = 30f; // damage taken fades out of memory over about this long
    public float killMemorySeconds = 45f;   // same for kills
    public float strugglingDamage = 40f;    // taking this much damage inside the memory window counts as fully struggling
    public float dominatingKills = 4f;      // this many kills inside the window, without getting hurt, counts as fully dominating
    public float changeSpeed = 0.04f;       // how fast difficulty moves toward its target each second, 0.04 is about 12 seconds to cross half the range

    [Header("What difficulty changes (value at easiest, value at hardest)")]
    public Vector2 speedRange = new Vector2(0.85f, 1.2f);      // chase and investigate speed
    public Vector2 detectionRange = new Vector2(0.8f, 1.3f);   // how far away enemies notice the player
    public Vector2 damageRange = new Vector2(0.7f, 1.3f);      // how hard enemy attacks hit
    public Vector2 aggressionRange = new Vector2(0.8f, 1.25f); // how often enemies attack, higher = shorter wait between attacks

    [Header("Debug")]
    public bool showDebug = true; // small readout in the top right corner, for testing and debug

    // what we have seen the player do recently, both numbers fade away over time
    private float recentDamage;
    private float recentKills;
    private float struggle;    // 0 to 1, how badly the player is doing
    private float dominance;   // 0 to 1, how easily the player is winning
    private int lastTier = 1;  // 0 = easing off, 1 = normal, 2 = pushing harder, only used to log changes

    // multipliers the enemies read, they all return 1 (no change) if the manager doesn't exist for some reason
    public static float SpeedMultiplier { get { return Instance != null ? Instance.Evaluate(Instance.speedRange) : 1f; } }
    public static float DetectionMultiplier { get { return Instance != null ? Instance.Evaluate(Instance.detectionRange) : 1f; } }
    public static float DamageMultiplier { get { return Instance != null ? Instance.Evaluate(Instance.damageRange) : 1f; } }
    public static float AggressionMultiplier { get { return Instance != null ? Mathf.Max(0.1f, Instance.Evaluate(Instance.aggressionRange)) : 1f; } }

    // enemies call these when player damage or kills happen, so the manager can keep track of how the player is doing
    public static void ReportPlayerDamaged(float amount)
    {
        if (Instance != null) Instance.recentDamage += amount;
    }

    public static void ReportKill()
    {
        if (Instance != null) Instance.recentKills += 1f;
    }

    // makes the manager once when the game starts, so it doesn't need to be put in SPScene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (Instance == null)
        {
            new GameObject("DifficultyManager").AddComponent<DifficultyManager>();
        }
    }

    // makes sure only one exists, and it doesn't get destroyed when a scene reloads
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // only one allowed
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // keeps working if a scene gets reloaded
    }
    // updates the difficulty every frame, based on how the player is doing
    void Update()
    {
        // Time.deltaTime is 0 while the game is paused, so nothing changes during menus and popups
        recentDamage *= Mathf.Exp(-Time.deltaTime / damageMemorySeconds);
        recentKills *= Mathf.Exp(-Time.deltaTime / killMemorySeconds);

        // struggling = taking a lot of damage lately
        struggle = Mathf.Clamp01(recentDamage / strugglingDamage);

        // dominating = killing things fast without getting hurt much, getting hurt cancels it out
        dominance = Mathf.Clamp01(recentKills / dominatingKills) * (1f - struggle);

        // quiet moments drift back to the middle, struggling pulls toward 0, dominating pulls toward 1
        float target = 0.5f + 0.5f * (dominance - struggle);

        // move gradually so the player never feels a sudden jump
        difficulty = Mathf.MoveTowards(difficulty, target, changeSpeed * Time.deltaTime);

        LogTierChange();
    }

    // picks a value between the easy and hard end of a range
    float Evaluate(Vector2 range)
    {
        return Mathf.Lerp(range.x, range.y, difficulty);
    }
    // returns a string for the current tier, used for logging and debug display
    string TierName(int tier)
    {
        if (tier == 0) return "Easing off";
        if (tier == 2) return "Pushing harder";
        return "Normal";
    }

    int CurrentTier()
    {
        if (difficulty < 0.33f) return 0;
        if (difficulty > 0.66f) return 2;
        return 1;
    }

    // writes a line in the Console whenever the difficulty crosses into a new band
    void LogTierChange()
    {
        int tier = CurrentTier();
        if (tier == lastTier) return;

        lastTier = tier;
        Debug.Log("Difficulty is now: " + TierName(tier) + " (" + Mathf.RoundToInt(difficulty * 100f) + "%)");
    }
    // draws a small readout in the top right corner, for testing and debug
    void OnGUI()
    {
        if (!showDebug) return;

        string text = "Difficulty: " + Mathf.RoundToInt(difficulty * 100f) + "% (" + TierName(CurrentTier()) + ")\n"
                    + "Recent damage taken: " + recentDamage.ToString("F0") + "\n"
                    + "Recent kills: " + recentKills.ToString("F1") + "\n"
                    + "Enemy speed x" + SpeedMultiplier.ToString("F2") + "  damage x" + DamageMultiplier.ToString("F2");

        GUI.Label(new Rect(Screen.width - 270f, 10f, 260f, 80f), text);
    }
}