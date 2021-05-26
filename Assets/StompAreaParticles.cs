using UnityEngine;

public class StompAreaParticles : MonoBehaviour
{
    private ParticleSystem[] particleSystems;

    public float fadeOutTime = 2;

    private void Start()
    {
        particleSystems = new ParticleSystem[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform particles = transform.GetChild(i);

            particleSystems[i] = particles.GetComponent<ParticleSystem>();
        }
    }

    public void FadeOut()
    {
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem particles = particleSystems[i];

            ParticleSystem.MainModule main = particles.main;

            ParticleSystem.MinMaxGradient startColor = main.startColor;

            Color colorMin = startColor.colorMin;
            colorMin.a = 0f;
            startColor.colorMin = colorMin;

            Color colorMax = startColor.colorMax;
            colorMax.a = 0f;
            startColor.colorMax = colorMax;

            main.startColor = startColor;
        }
    }
}
