using System.Collections;
using UnityEngine;
using TMPro;

public class Manager_Ch1 : MonoBehaviour
{
    /*──────── Tunables ───────────*/
    private const int TOTAL = 26;                        // 0..25
    [Range(1, 10)] public int firstAutoCount = 4;        // autoplay at start

    [Header("Gameplay Gating")]
    [Tooltip("How many pots must receive seeds after index 10.")]
    public int requiredSeedPots = 6;

    [Tooltip("Seconds to wait between index 11 and 12.")]
    public float delayBeforeIndex12 = 5f;

    [Header("Tags (must match your scene)")]
    [Tooltip("Tag assigned to seed GameObjects (with Rigidbody + non-trigger collider).")]
    public string seedTag = "Seed";
    [Tooltip("Tag assigned to the watering trigger volume / zone.")]
    public string wateringTriggerTag = "WaterZone";

    /*──────── Outline refs ───────*/
    [Header("Item Outlines")]
    public Outline potOutline;
    public Outline seedOutline;
    public Outline xOutline;
    public Outline rulerOutline;
    public Outline waterCanOutline;
    public Outline chatBotOutline;

    Outline[] outlines;
    int       currentItemIndex = -1;   // starts at “none”

    /*──────── Dialog UI / Audio ───*/
    [Header("UI & Audio")]
    public TMP_Text    dialogText;
    public AudioSource audioSource;

    [Header("Dialog Content (exactly 26)")]
    public AudioClip[] dialogClips = new AudioClip[TOTAL];
    public string[]    dialogLines = new string[TOTAL];

    public GameObject continueButton;
    readonly bool[] played = new bool[TOTAL];

    /*──────── Progress State ─────*/
    int   seedsPlacedCount = 0;
    bool  waitingForSeeds  = false; // becomes true after index 10 finishes
    bool  waitingForWater  = false; // becomes true after index 12 finishes
    bool  index11Queued    = false;
    bool  index12Queued    = false;

    /*──────── Constants ───────────*/
    static readonly Color Pink = ParseHex("#FF0047");
    const float ActiveWidth = 10f, HiddenWidth = 0f;

    /*──────── Unity ───────────────*/
    void Awake()
    {
        outlines = new[] {
            potOutline, seedOutline, xOutline,
            rulerOutline, waterCanOutline, chatBotOutline
        };

        if (dialogClips.Length != TOTAL || dialogLines.Length != TOTAL)
        {
            Debug.LogError($"Manager_Ch1: need EXACTLY {TOTAL} clips + {TOTAL} lines.");
            enabled = false;
            return;
        }

        foreach (var o in outlines) SetOutlineHidden(o);

        if (audioSource)
        {
            audioSource.playOnAwake = false;
            audioSource.loop        = false;
            audioSource.enabled     = true;

            // Start narration in 2D so distance/FOV can’t mute it at scene start.
            audioSource.spatialBlend = 0f;
            if (audioSource.volume <= 0f) audioSource.volume = 1f;
        }
    }

    void Start()
    {
        continueButton?.SetActive(false);
        StartCoroutine(AutoplayFirstN(firstAutoCount));
    }

    /*──────── Public API (called by triggers) ─────────*/
    /// <summary>
    /// Call when a seed successfully enters a *unique* pot trigger.
    /// </summary>
    public void NotifySeedPlaced()
    {
        if (!waitingForSeeds) return;

        seedsPlacedCount = Mathf.Clamp(seedsPlacedCount + 1, 0, requiredSeedPots);
        // Debug.Log($"Seeds placed: {seedsPlacedCount}/{requiredSeedPots}");

        if (seedsPlacedCount >= requiredSeedPots)
        {
            waitingForSeeds = false;
            TriggerIndex11Then12();
        }
    }

    /// <summary>
    /// Call when the watering interaction (can / shader-graph hit) is detected.
    /// </summary>
    public void NotifyWateringDone()
    {
        if (!waitingForWater) return;
        waitingForWater = false;
        TryPlay(13); // jump to index 13
    }

    public void PlayDialogByIndex(int index) => TryPlay(index);

    /*──────── Internals ──────────*/
    void TryPlay(int idx)
    {
        if (idx < 0 || idx >= played.Length || played[idx]) return;

        // Show corresponding item outline for Dialog 5–10 (indices 4..9)
        if (idx >= 4 && idx <= 9) SwitchOutline(idx - 4);

        StartCoroutine(PlayLine(idx));
    }

    IEnumerator AutoplayFirstN(int count)
    {
        yield return null;
        yield return new WaitForSeconds(0.05f);

        if (Time.timeScale == 0f) Time.timeScale = 1f;
        int safe = Mathf.Clamp(count, 0, TOTAL);

        for (int i = 0; i < safe; i++)
            yield return PlayLine(i);
    }

    IEnumerator PlayLine(int idx)
    {
        played[idx] = true;

        if (dialogText) dialogText.text = dialogLines[idx];

        if (audioSource && dialogClips[idx])
        {
            audioSource.Stop();
            audioSource.clip = dialogClips[idx];
            audioSource.time = 0f;
            audioSource.Play();

            float len = audioSource.clip.length;
            float t = 0f;
            while (t < len) { t += Time.unscaledDeltaTime; yield return null; }
        }
        else
        {
            Debug.LogWarning($"Manager_Ch1: Missing clip at index {idx}. Using 3s fallback.");
            yield return new WaitForSeconds(3f);
        }

        // ====== Post-line gating logic ======
        if (idx == 10) // after “place seeds” instruction
        {
            waitingForSeeds = true;
            seedsPlacedCount = 0;
        }
        else if (idx == 11)
        {
            if (!index12Queued)
            {
                index12Queued = true;
                StartCoroutine(PlayAfterDelay(12, Mathf.Max(0f, delayBeforeIndex12)));
            }
        }
        else if (idx == 12)
        {
            waitingForWater = true;
        }

        if (idx == TOTAL - 1 && continueButton) continueButton.SetActive(true);
    }

    IEnumerator PlayAfterDelay(int index, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        TryPlay(index);
    }

    void TriggerIndex11Then12()
    {
        if (!index11Queued)
        {
            index11Queued = true;
            TryPlay(11); // 12 gets scheduled in PlayLine(11)
        }
    }

    /*──────── Outline helpers ────*/
    void SwitchOutline(int newIndex)
    {
        if (currentItemIndex >= 0 && currentItemIndex < outlines.Length)
            SetOutlineHidden(outlines[currentItemIndex]);

        currentItemIndex = newIndex;
        if (currentItemIndex >= 0 && currentItemIndex < outlines.Length)
            SetOutlineRed(outlines[currentItemIndex]);
    }

    void SetOutlineRed(Outline o)
    {
        if (!o) return;
        o.OutlineColor = Pink;
        o.OutlineWidth = ActiveWidth;
        o.OutlineMode  = Outline.Mode.OutlineAll;
        o.enabled      = true;
    }

    void SetOutlineHidden(Outline o)
    {
        if (!o) return;
        o.OutlineWidth = HiddenWidth;
        o.OutlineMode  = Outline.Mode.OutlineHidden;
        o.enabled      = true;
    }

    static Color ParseHex(string hex)
    {
        Color c; ColorUtility.TryParseHtmlString(hex, out c); return c;
    }
}
