using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Manager_Ch1 : MonoBehaviour
{
    /*──────── Tunables ───────────*/
    private const int TOTAL = 26;                        // 0..25
    [Range(1, 10)] public int firstAutoCount = 4;        // autoplay at start

    [Header("Timing")]
    [Tooltip("Seconds to wait after dialog 9 finishes before auto-playing 10.")]
    public float delayAfter9To10 = 1.0f;                 // << was 5s, now faster

    [Tooltip("Seconds between dialog 11 → 12.")]
    public float delayBeforeIndex12 = 2.0f;              // << was 5s, now faster

    [Header("Seeding")]
    [Tooltip("How many unique pots/zones must be seeded AFTER index 10.")]
    public int requiredSeedPots = 6;

    [Header("References")]
    [Tooltip("OPTIONAL: specific SeedPotTrigger list. If set, target = min(requiredSeedPots, list length).")]
    public SeedPotTrigger[] seedPots;                    // optional (recommended)

    [Tooltip("The single watering zone to detect watering AFTER index 12 finishes.")]
    public WateringZoneTrigger wateringZone;             // single zone

    [Tooltip("Optional relay on the watering can to tell us when the player grabs it (to hide outline at 9).")]
    public WaterCanGrabRelay waterCanGrabRelay;

    /*──────── Outlines (NO chatbot) ─────*/
    [Header("Item Outlines")]
    public Outline potOutline;
    public Outline seedOutline;
    public Outline xOutline;
    public Outline rulerOutline;
    public Outline waterCanOutline;

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
    // Seeding gate (after idx 10)
    bool waitingForSeeds = false;
    readonly HashSet<SeedPotTrigger> seededPotsByScript = new HashSet<SeedPotTrigger>();
    readonly HashSet<int>            seededPotsByColliderID = new HashSet<int>(); // if using SeedZoneTrigger

    // Watering gate (after idx 12)
    bool waitingForWater = false;
    bool wateringDone = false;

    // One-shot guards
    bool index11Queued = false;
    bool index12Queued = false;

    /*──────── Constants ───────────*/
    static readonly Color Pink = ParseHex("#FF0047");
    const float ActiveWidth = 10f, HiddenWidth = 0f;

    /*──────── Unity ───────────────*/
    void Awake()
    {
        // Order maps indices 4..8 → [pot, seed, x, ruler, waterCan]
        outlines = new[] { potOutline, seedOutline, xOutline, rulerOutline, waterCanOutline };

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
            audioSource.spatialBlend = 0f; // 2D
            if (audioSource.volume <= 0f) audioSource.volume = 1f;
        }

        // Wire events
        if (seedPots != null)
            foreach (var pot in seedPots) if (pot) pot.SetManager(this);

        if (wateringZone) wateringZone.SetManager(this);
        if (waterCanGrabRelay) waterCanGrabRelay.SetManager(this);

        // Clamp required count against provided list
        if (seedPots != null && seedPots.Length > 0)
            requiredSeedPots = Mathf.Clamp(requiredSeedPots, 1, seedPots.Length);
        else
            requiredSeedPots = Mathf.Max(1, requiredSeedPots);
    }

    void Start()
    {
        continueButton?.SetActive(false);
        StartCoroutine(AutoplayFirstN(firstAutoCount));
    }

    /*──────── Public API (called by triggers/relay) ─────────*/

    /// Called by each SeedPotTrigger once when a pot gets its first seed (AFTER idx 10).
    public void NotifySeedPlaced(SeedPotTrigger pot)
    {
        if (!waitingForSeeds || pot == null) return;

        if (seededPotsByScript.Add(pot))
            CheckSeedsCompletion();
    }

    /// If you’re using collider-only “SeedZoneTrigger”, call this.
    public void NotifySeedPlacedCollider(Collider zoneCollider)
    {
        if (!waitingForSeeds || !zoneCollider) return;

        if (seededPotsByColliderID.Add(zoneCollider.GetInstanceID()))
            CheckSeedsCompletion();
    }

    void CheckSeedsCompletion()
    {
        int target = requiredSeedPots;
        int currentCount = (seedPots != null && seedPots.Length > 0)
            ? seededPotsByScript.Count
            : seededPotsByColliderID.Count;

        if (currentCount >= target)
        {
            waitingForSeeds = false;
            TriggerIndex11Then12(); // plays 11 immediately (no extra wait)
        }
    }

    /// Called by the single watering zone when the can tip enters (AFTER idx 12).
    public void NotifyWateringZoneHit()
    {
        if (!waitingForWater || wateringDone) return;
        wateringDone = true;

        if (waterCanOutline) SetOutlineHidden(waterCanOutline);
        currentItemIndex = -1;

        waitingForWater = false;
        TryPlay(13);
    }

    /// Called when the player grabs the watering can (during idx 9).
    public void NotifyWaterCanGrabbed()
    {
        if (waterCanOutline) SetOutlineHidden(waterCanOutline);
    }

    public void PlayDialogByIndex(int index) => TryPlay(index);

    /*──────── Internals: Dialog Flow ─────────*/
    void TryPlay(int idx)
    {
        if (idx < 0 || idx >= played.Length || played[idx]) return;

        // Show outlines for indices 4..8 mapping to [pot, seed, x, ruler, waterCan]
        int mapped = idx - 4;
        if (mapped >= 0 && mapped < outlines.Length)
            SwitchOutline(mapped);

        // Safety: ensure water can outline is hidden as we enter 10/13
        if ((idx == 10 || idx == 13) && waterCanOutline) SetOutlineHidden(waterCanOutline);

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

        // ====== Post-line gating + auto-play logic ======
        if (idx == 9)
        {
            // Faster hop to 10
            StartCoroutine(PlayAfterDelay(10, Mathf.Max(0f, delayAfter9To10)));
        }
        else if (idx == 10)
        {
            // Begin seeding phase
            seededPotsByScript.Clear();
            seededPotsByColliderID.Clear();
            waitingForSeeds = true;
            index11Queued = false;
            index12Queued = false;
        }
        else if (idx == 11)
        {
            // Shorter hop to 12
            if (!index12Queued)
            {
                index12Queued = true;
                StartCoroutine(PlayAfterDelay(12, Mathf.Max(0f, delayBeforeIndex12)));
            }
        }
        else if (idx == 12)
        {
            // Begin watering phase (single zone)
            waitingForWater = true;
            wateringDone = false;
        }
        else if (idx >= 13 && idx < TOTAL - 1)
        {
            // Keep end sequence pacing at 5s unless you want this tunable too
            StartCoroutine(PlayAfterDelay(idx + 1, 5f));
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
            TryPlay(11); // plays immediately; 12 is scheduled in PlayLine(11)
        }
    }

    /*──────── Helpers ─────────*/
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
