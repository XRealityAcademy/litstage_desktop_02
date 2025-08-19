using UnityEngine;

/// <summary>
/// Attach this to any GameObject to document its purpose or reason in the scene.
/// This can help you organize, debug, or communicate with your team.
/// </summary>
public class ObjectPurposeNote : MonoBehaviour
{
    [Tooltip("Describe the reason or purpose of this GameObject in the scene.")]
    [TextArea(2, 5)]
    public string reasonForThisObject;
}