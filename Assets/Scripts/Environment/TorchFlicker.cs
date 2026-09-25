using UnityEngine;

namespace Medallas.Environment
{
    // Luz de antorcha con parpadeo sutil (ruido Perlin, no random puro, para
    // que la variacion sea suave en vez de parpadear como una bombilla rota).
    [RequireComponent(typeof(Light))]
    public class TorchFlicker : MonoBehaviour
    {
        public float baseIntensity = 2.2f;
        public float flickerAmount = 0.5f;
        public float flickerSpeed = 2.5f;

        Light torchLight;
        float noiseOffset;

        void Awake()
        {
            torchLight = GetComponent<Light>();
            noiseOffset = Random.Range(0f, 100f);
        }

        void Update()
        {
            float noise = Mathf.PerlinNoise(noiseOffset, Time.time * flickerSpeed);
            torchLight.intensity = baseIntensity + (noise - 0.5f) * 2f * flickerAmount;
        }
    }
}
