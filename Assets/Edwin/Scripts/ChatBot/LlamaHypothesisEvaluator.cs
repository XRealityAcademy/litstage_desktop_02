using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;

[System.Serializable]
public class Message
{
    public string role;
    public string content;
}

[System.Serializable]
public class LlamaRequest
{
    public string model;
    public Message[] messages;
}

[System.Serializable]
public class CompletionContent
{
    public string type;
    public string text;
}
[System.Serializable]
public class CompletionMessage
{
    public CompletionContent content;
    public string role;
    public string stop_reason;
}
[System.Serializable]
public class LlamaResponse
{
    public CompletionMessage completion_message;
}

[System.Serializable]
public class HypothesisBody
{
    public string hypothesis;
    public string[] suggestions;
}
[System.Serializable]
public class HypothesisResult
{
    public string status;
    public HypothesisBody body;
}

public static class HypothesisResultStore
{
    public static HypothesisResult Latest;
}

public class LlamaHypothesisEvaluator : MonoBehaviour
{
    [Header("Input Reference")]
    public TextMeshProUGUI inputText;

    [Header("Event Name")]
    public string onCompleteEventName;

    [Header("Spinner (Optional)")]
    public GameObject spinner; // Assign your spinner GameObject in Inspector

    private const string apiKey = "LLM|579659775185384|kXxF2U_K6ikrxPkRQm2IVESph6o";
    private const string apiUrl = "https://api.llama.com/v1/chat/completions";

    public void EvaluateHypothesis()
    {
        if (inputText == null)
        {
            Debug.LogWarning("EDWIN_DEBUG: No inputText (TextMeshProUGUI) assigned to LlamaHypothesisEvaluator!");
            return;
        }

        string hypothesis = inputText.text;
        if (!string.IsNullOrWhiteSpace(hypothesis))
        {
            StartCoroutine(CallLlamaApi(hypothesis));
        }
        else
        {
            Debug.LogWarning("EDWIN_DEBUG: No hypothesis entered in inputText.");
        }
    }

    private IEnumerator CallLlamaApi(string hypothesis)
    {
        // Show spinner if assigned
        if (spinner != null)
            spinner.SetActive(true);

        LlamaRequest requestObj = new LlamaRequest
        {
            model = "Llama-4-Maverick-17B-128E-Instruct-FP8",
            messages = new Message[]
            {
                new Message { role = "system", content = HypothesisPromptTemplates.Default },
                new Message { role = "user", content = hypothesis }
            }
        };

        string jsonBody = JsonUtility.ToJson(requestObj);

        using (UnityWebRequest req = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            string apiResponse = req.downloadHandler.text;
            HypothesisResultStore.Latest = null;

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("EDWIN_DEBUG: Llama API Response: " + apiResponse);

                try
                {
                    LlamaResponse responseObj = JsonUtility.FromJson<LlamaResponse>(apiResponse);

                    if (responseObj != null && responseObj.completion_message != null && responseObj.completion_message.content != null)
                    {
                        string assistantText = responseObj.completion_message.content.text.Trim();
                        try
                        {
                            HypothesisResult hResult = JsonUtility.FromJson<HypothesisResult>(assistantText);
                            HypothesisResultStore.Latest = hResult;

                            if (!string.IsNullOrWhiteSpace(onCompleteEventName) && hResult != null)
                            {
                                Debug.Log("EDWIN_DEBUG: Sending onCompleteEventName: " + onCompleteEventName);
                                EventFunctions.SendEvent<HypothesisResult>(onCompleteEventName, hResult);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("EDWIN_DEBUG: Could not parse assistant text as JSON. Exception: " + e);
                        }
                    }
                    else
                    {
                        Debug.LogError("EDWIN_DEBUG: Could not parse Llama API response.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("EDWIN_DEBUG: Error parsing Llama API response: " + e);
                }
            }
            else
            {
                Debug.LogError($"EDWIN_DEBUG: Llama API Error: {req.error}\n{apiResponse}");
            }

            // Hide spinner after the request finishes (success or fail)
            if (spinner != null)
                spinner.SetActive(false);
        }
    }
}