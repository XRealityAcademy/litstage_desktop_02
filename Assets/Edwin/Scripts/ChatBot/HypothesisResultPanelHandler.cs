using UnityEngine;
using TMPro;
using System.Text;

public class HypothesisResultPanelHandler : MonoBehaviour
{
    [Header("Event Name")]
    public string eventName;

    [Header("Panel References")]
    public GameObject badPanel;
    public GameObject goodPanel;

    [Header("Bad Panel Texts")]
    public TextMeshProUGUI badPanelHypothesisText;
    public TextMeshProUGUI badPanelSuggestionsText;

    [Header("Good Panel Texts")]
    public TextMeshProUGUI goodPanelHypothesisText;
    public TextMeshProUGUI goodPanelSuggestionsText;

    private void Awake()
    {
        // Hide panels by default
        if (badPanel) badPanel.SetActive(false);
        if (goodPanel) goodPanel.SetActive(false);

        Debug.Log("EDWIN_DEBUG: Panels hidden by default in Awake.");
    }

    private void OnEnable()
    {
        // Listen for the event
        EventFunctions.ListenEvent<HypothesisResult>(eventName, OnHypothesisEventReceived);
        Debug.Log("EDWIN_DEBUG: Listening for event: " + eventName);
    }

    private void OnDisable()
    {
        // Remove listener to prevent leaks
        EventFunctions.RemoveListener<HypothesisResult>(eventName, OnHypothesisEventReceived);
        Debug.Log("EDWIN_DEBUG: Removed listener for event: " + eventName);
    }

    private void OnHypothesisEventReceived(HypothesisResult result)
    {
        Debug.Log("EDWIN_DEBUG: Event object received:\n" + JsonUtility.ToJson(result, true));
        Debug.Log("EDWIN_DEBUG: Received HypothesisResult event: " + eventName);

        // Hide both panels first
        if (badPanel) badPanel.SetActive(false);
        if (goodPanel) goodPanel.SetActive(false);

        if (result == null || result.body == null)
        {
            Debug.LogWarning("EDWIN_DEBUG: HypothesisResult or body is null!");
            return;
        }

        if (result.status == "bad")
        {
            Debug.Log("EDWIN_DEBUG: Showing BAD panel.");
            if (badPanel) badPanel.SetActive(true);

            if (badPanelHypothesisText)
            {
                badPanelHypothesisText.text = result.body.hypothesis ?? "";
                Debug.Log("EDWIN_DEBUG: Set badPanelHypothesisText: " + badPanelHypothesisText.text);
            }
            else
            {
                Debug.LogWarning("EDWIN_DEBUG: badPanelHypothesisText is null!");
            }

            if (badPanelSuggestionsText)
            {
                badPanelSuggestionsText.text = FormatSuggestions(result.body.suggestions);
                Debug.Log("EDWIN_DEBUG: Set badPanelSuggestionsText: " + badPanelSuggestionsText.text);
            }
            else
            {
                Debug.LogWarning("EDWIN_DEBUG: badPanelSuggestionsText is null!");
            }
        }
        else if (result.status == "good")
        {
            Debug.Log("EDWIN_DEBUG: Showing GOOD panel.");
            if (goodPanel) goodPanel.SetActive(true);

            if (goodPanelHypothesisText)
            {
                goodPanelHypothesisText.text = result.body.hypothesis ?? "";
                Debug.Log("EDWIN_DEBUG: Set goodPanelHypothesisText: " + goodPanelHypothesisText.text);
            }
            else
            {
                Debug.LogWarning("EDWIN_DEBUG: goodPanelHypothesisText is null!");
            }

            if (goodPanelSuggestionsText)
            {
                goodPanelSuggestionsText.text = FormatSuggestions(result.body.suggestions);
                Debug.Log("EDWIN_DEBUG: Set goodPanelSuggestionsText: " + goodPanelSuggestionsText.text);
            }
            else
            {
                Debug.LogWarning("EDWIN_DEBUG: goodPanelSuggestionsText is null!");
            }
        }
        else
        {
            Debug.LogWarning("EDWIN_DEBUG: Unrecognized status in HypothesisResult: " + result.status);
        }
    }

    private string FormatSuggestions(string[] suggestions)
    {
        if (suggestions == null || suggestions.Length == 0)
            return "";

        StringBuilder sb = new StringBuilder();
        foreach (var suggestion in suggestions)
        {
            
            if (!string.IsNullOrWhiteSpace(suggestion))
                sb.AppendLine("* " + suggestion.Trim());
        }
        return sb.ToString().TrimEnd();
    }

    public void Hide()
    {
        if (badPanel) badPanel.SetActive(false);
        if (goodPanel) goodPanel.SetActive(false);
    }
}