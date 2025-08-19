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
    //public Outline chatBotOutline; // keep as in your current 0–9 setup
    public float delayBeforeIndex12 = 5f;

    Outline[] outlines;
    int currentItemIndex = -1;   // starts at “none”

    /*──────── Dialog UI / Audio ───*/
    [Header("UI & Audio")]
    public TMP_Text dialogText;
    public AudioSource audioSource;

    [Header("Dialog Content (exactly 26)")]
    public AudioClip[] dialogClips = new AudioClip[TOTAL];
    public string[] dialogLines = new string[TOTAL];

    [Header("Per-Dialog Delays (seconds)")]
    [Tooltip("Delay after each dialog index before the next one plays (size = 26).")]
    public float[] delayAfterLine = new float[TOTAL];

    public GameObject continueButton;
    readonly bool[] played = new bool[TOTAL];

    /*──────── Progress State ─────*/
    int seedsPlacedCount = 0;     
    bool waitingForSeeds = false; 
    bool waitingForWater = false; 
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
            rulerOutline, waterCanOutline
        };

        if (dialogClips.Length != TOTAL || dialogLines.Length != TOTAL)
        {
            Debug.LogError($"Manager_Ch1: need EXACTLY {TOTAL} clips + {TOTAL} lines.");
            enabled = false;
            return;
        }

        if (delayAfterLine.Length != TOTAL)
        {
            delayAfterLine = new float[TOTAL]; // reset to correct size if mismatch
        }

        // Set defaults (only if unset)
        delayAfterLine[9] = Mathf.Approximately(delayAfterLine[9], 0f) ? 1.0f : delayAfterLine[9];   // after 9→10
        delayAfterLine[11] = Mathf.Approximately(delayAfterLine[11], 0f) ? 5.0f : delayAfterLine[11]; // before 12
        for (int i = 14; i < TOTAL; i++)
            if (Mathf.Approximately(delayAfterLine[i], 0f)) delayAfterLine[i] = 5.0f; // 14→25 defaults

        foreach (var o in outlines) if (o) SetOutlineHidden(o);

        if (audioSource)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.enabled = true;
            audioSource.spatialBlend = 0f;
            if (audioSource.volume <= 0f) audioSource.volume = 1f;
        }

        for (int i = 0; i < seedPots.Length; i++)
            if (seedPots[i]) seedPots[i].SetManager(this);
    }

    void Start()
    {
        continueButton?.SetActive(false);
        StartCoroutine(AutoplayFirstN(firstAutoCount));   
    }

    /*──────── Public API (kept) ─────────*/
    public void PlayDialogByIndex(int index) => TryPlay(index);

    public void NotifySeedPlaced() =>
        Debug.LogWarning("NotifySeedPlaced() legacy call ignored. Use SeedPotTrigger per pot.");

    public void NotifySeedPlaced(SeedPotTrigger pot)
    {
        if (!waitingForSeeds || pot == null) return;

        bool recognized = false;
        for (int i = 0; i < seedPots.Length; i++)
            if (seedPots[i] == pot) { recognized = true; break; }
        if (!recognized) return;

        if (uniqueSeededPots.Add(pot))
        {
            if (uniqueSeededPots.Count >= 6)
            {
                waitingForSeeds = false;
                TriggerIndex11Then12();
            }
        }
    }

    public void NotifyWateringDone()
    {
        if (!waitingForWater) return;
        waitingForWater = false;
        TryPlay(13); 
    }

    void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(seedPotTriggerTag) && other.CompareTag(seedPotTriggerTag))
            NotifySeedPlaced();
        else if (!string.IsNullOrEmpty(wateringTriggerTag) && other.CompareTag(wateringTriggerTag))
            NotifyWateringDone();
    }

    /*──────── Internals ──────────*/
    void TryPlay(int idx)
    {
        if (idx < 0 || idx >= played.Length || played[idx]) return;

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

        // Apply custom per-index delay (if any)
        if (delayAfterLine[idx] > 0f)
            yield return new WaitForSeconds(delayAfterLine[idx]);

        if (idx == 9)
        {
            StartCoroutine(PlayAfterDelay(10, delayAfterLine[9]));
        }
        else if (idx == 10)
        {
            // Begin strict seeding gate: 6 unique pots, 1 seed each            
            waitingForSeeds = true;
            uniqueSeededPots.Clear();
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
            waitingForWater = true;
        }
        else if (idx == 13)
        {
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
            TryPlay(11); 
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

    public void NotifyWaterCanGrabbed()
    {
        if (waterCanOutline)
        {
            waterCanOutline.OutlineWidth = 0f;
            waterCanOutline.OutlineMode = Outline.Mode.OutlineHidden;
            waterCanOutline.enabled = true;
        }
    }

    IEnumerator AutoPlayFrom13()
    {
        for (int i = 14; i < TOTAL; i++)
        {
            yield return new WaitForSeconds(delayAfterLine[i]);
            TryPlay(i);
            while (!played[i]) yield return null;
        }
    }
}
