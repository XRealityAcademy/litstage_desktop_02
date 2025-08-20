using UnityEngine;

public class Pouring : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem waterParticles;   // Assign your particle system in inspector

    [Header("Pour Settings")]
    public float pourThreshold = 0.5f;  // Angle threshold (adjust in inspector)
    public float stopThreshold = 0.3f;  // Slightly less to avoid flicker

    private bool isPouring = false;

    void Update()
    {
        // Measure how much the spout is pointing downward
        float tiltAmount = Vector3.Dot(transform.forward, Vector3.down);
        // forward = spout direction; adjust if your model is oriented differently

        if (!isPouring && tiltAmount > pourThreshold)
        {
            StartPour();
        }
        else if (isPouring && tiltAmount < stopThreshold)
        {
            StopPour();
        }
    }

    void StartPour()
    {
        waterParticles.Play();
        isPouring = true;
    }

    void StopPour()
    {
        waterParticles.Stop();
        isPouring = false;
    }
}