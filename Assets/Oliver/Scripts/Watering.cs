using UnityEngine;

public class Watering : MonoBehaviour
{
    [Header("Assign the Particle System for water here")]
    public ParticleSystem waterParticles;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlantSoil"))
        {
            if (waterParticles != null && !waterParticles.isPlaying)
            {
                waterParticles.Play();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlantSoil"))
        {
            if (waterParticles != null && waterParticles.isPlaying)
            {
                waterParticles.Stop();
            }
        }
    }
}
