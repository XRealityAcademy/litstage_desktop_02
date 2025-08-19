using System.Collections;
using System.Collections.Generic;
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
    public float delayAfter9To10 = 1.0f;

    [Tooltip("Seconds to wait between index 11 and 12.")]
    public float delayBeforeIndex12 = 5f;

    [Tooltip("Tag used by seed-pot triggers (legacy OnTriggerEnter on THIS Manager).")]
    public string seedPotTriggerTag = "SeedPot";

    [Tooltip("Tag used by the watering trigger zone (legacy OnTriggerEnter on THIS Manager).")]
    public string wateringTriggerTag = "WaterZone";

    /* === NEW: strict per-pot seeding (does NOT change 0–9) === */
    [Header("Strict Seeding (exactly 6 unique pots)")]
    [Tooltip("Assign the SIX SeedPotTrigger components (one on each pot).")]
    public SeedPotTrigger[] seedPots = new SeedPotTrigger[6];

    /*──────── Outline refs ───────*/
    [Header("Item Outlines")]
    public Outline potOutline;
    public Outline seedOutline;
    public Outline xOutline;
    public Outline rulerOutline;
    public Outline waterCanOutline;
    public Outline chatBotOutline; // keep as in your current 0–9 setup

    Outline[] outlines;
    int currentItemIndex = -1;   // starts at “none”

    /*──────── Dialog UI / Audio ───*/
    [Header("UI & Audio")]
    public TMP_Text dialogText;
    public AudioSource audioSource;

    [Header("Dialog Content (exactly 26)")]
    public AudioClip[] dialogClips = new AudioClip[TOTAL];
    public string[] dialogLines = new string[TOTAL];

    public GameObject continueButton;
    readonly bool[] played = new bool[TOTAL];

    /*──────── Progress State ─────*/
    int seedsPlacedCount = 0;     // legacy counter (kept)
    bool waitingForSeeds = false; // becomes true after index 10 finishes
    bool waitingForWater = false; // becomes true after index 12 finishes
    bool index11Queued = false;
    bool index12Queued = false;

    /* === NEW: track unique pots that have received their first seed === */
    readonly HashSet<SeedPotTrigger> uniqueSeededPots = new HashSet<SeedPotTrigger>();

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

        foreach (var o in outlines) if (o) SetOutlineHidden(o);

        if (audioSource)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.enabled = true;
            audioSource.spatialBlend = 0f;
            if (audioSource.volume <= 0f) audioSource.volume = 1f;
        }

        // NEW: wire seed pots so they can call back
        for (int i = 0; i < seedPots.Length; i++)
            if (seedPots[i]) seedPots[i].SetManager(this);
    }

    void Start()
    {
        continueButton?.SetActive(false);
        StartCoroutine(AutoplayFirstN(firstAutoCount));   // 0–9 behavior remains
    }

    /*──────── Public API (kept) ─────────*/
    public void PlayDialogByIndex(int index) => TryPlay(index);

    /// Legacy seed callback (kept for backward compatibility)
// LEGACY (no-arg) – ignored for strict 1-seed-per-unique-pot rule
    public void NotifySeedPlaced()
    {
        if (!waitingForSeeds) return;
        // We intentionally ignore this legacy path to guarantee:
        // 6 seeds in 6 distinct pots (reported via SeedPotTrigger).
        // (Keeps 0–9 untouched; only affects the post-10 seeding gate.)
        Debug.LogWarning("NotifySeedPlaced() legacy call ignored. Use SeedPotTrigger per pot for strict 1-per-pot.");
    }

    /// NEW: strict per-pot version — counts UNIQUE pots. Call from SeedPotTrigger.
    public void NotifySeedPlaced(SeedPotTrigger pot)
    {
        if (!waitingForSeeds || pot == null) return;

        // Only count if it's one of the configured six pots
        bool recognized = false;
        for (int i = 0; i < seedPots.Length; i++)
            if (seedPots[i] == pot) { recognized = true; break; }
        if (!recognized) return;

        if (uniqueSeededPots.Add(pot))
        {
            // As soon as 6 distinct pots each have one seed → trigger 11
            if (uniqueSeededPots.Count >= 6)
            {
                waitingForSeeds = false;
                TriggerIndex11Then12(); // plays 11 immediately; 12 is scheduled after 11
            }
        }
    }

    /// Watering done (kept API). Called by WateringZoneTrigger below.
    public void NotifyWateringDone()
    {
        if (!waitingForWater) return;
        waitingForWater = false;
        TryPlay(13); // jump to index 13
    }

    /*──────── Collision Hooks (optional legacy) ─────*/
    void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(seedPotTriggerTag) && other.CompareTag(seedPotTriggerTag))
        {
            // legacy: simple counter path (unique pots recommended instead)
            NotifySeedPlaced();
        }
        else if (!string.IsNullOrEmpty(wateringTriggerTag) && other.CompareTag(wateringTriggerTag))
        {
            // legacy: zone touched → treat as watering done
            NotifyWateringDone();
        }
    }

    /*──────── Internals (0–9 logic unchanged) ──────────*/
    void TryPlay(int idx)
    {
        if (idx < 0 || idx >= played.Length || played[idx]) return;

        // Show corresponding item outline for Dialog 5–10 (indices 4..9)
        if (idx >= 4 && idx <= 9) SwitchOutline(idx - 4);

        StartCoroutine(PlayLine(idx));
    }

    IEnumerator AutoplayFirstN(int count)
    {
        // one-frame + tiny delay: lets Quest get audio focus
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
        if (idx == 9)
        {
            // Auto-advance from 9 to 10 after a short delay
            StartCoroutine(PlayAfterDelay(10, Mathf.Max(0f, delayAfter9To10)));
        }

        // ====== Post-line gating logic (ONLY 10+ touched) ======
        if (idx == 10)
        {
            // Begin strict seeding gate: 6 unique pots, 1 seed each
            waitingForSeeds = true;
            uniqueSeededPots.Clear();

            // Count any pots already seeded before this moment
            foreach (var p in seedPots)
                if (p && p.IsSeeded) uniqueSeededPots.Add(p);

            if (uniqueSeededPots.Count >= 6)
            {
                waitingForSeeds = false;
                TriggerIndex11Then12();
            }
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
            waitingForWater = true; // WateringZoneTrigger will call NotifyWateringDone()
        }
        else if (idx == 13)
        {
            // after dialog 13, autoplay through to the end (14 → 25)
            StartCoroutine(AutoPlayFrom13());
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
        if (currentItemIndex >= 0 && currentItemIndex < outlines.Length && outlines[currentItemIndex])
            SetOutlineHidden(outlines[currentItemIndex]);

        currentItemIndex = newIndex;
        if (currentItemIndex >= 0 && currentItemIndex < outlines.Length && outlines[currentItemIndex])
            SetOutlineRed(outlines[currentItemIndex]);
    }

    void SetOutlineRed(Outline o)
    {
        if (!o) return;
        o.OutlineColor = Pink;
        o.OutlineWidth = ActiveWidth;
        o.OutlineMode = Outline.Mode.OutlineAll;
        o.enabled = true;
    }

    void SetOutlineHidden(Outline o)
    {
        if (!o) return;
        o.OutlineWidth = HiddenWidth;
        o.OutlineMode = Outline.Mode.OutlineHidden;
        o.enabled = true;
    }

    static Color ParseHex(string hex)
    {
        Color c; ColorUtility.TryParseHtmlString(hex, out c); return c;
    }

    // Add somewhere inside Manager_Ch1
    public void NotifyWaterCanGrabbed()
    {
        // Do whatever you want when the can is grabbed (hide outline, etc.)
        if (waterCanOutline)
        {
            waterCanOutline.OutlineWidth = 0f;
            waterCanOutline.OutlineMode = Outline.Mode.OutlineHidden;
            waterCanOutline.enabled = true;
        }
        // (Optional) any other side-effects
    }
    IEnumerator AutoPlayFrom13()
    {
        // start at 14 and play sequentially until 25
        for (int i = 14; i < TOTAL; i++)
        {
            // wait 5 seconds before each line
            yield return new WaitForSeconds(5f);
            TryPlay(i);

            // wait until this line finishes before moving to the next
            while (!played[i]) yield return null;
        }
    }

}
