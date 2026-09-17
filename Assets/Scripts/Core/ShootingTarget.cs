using UnityEngine;
using Medallas.Data;

namespace Medallas.Core
{
    // Objetivo de la galeria de tiro: recibe dano via Health, suma puntos al
    // ser destruido y opcionalmente reaparece tras un tiempo (blanco reutilizable).
    [RequireComponent(typeof(Health))]
    public class ShootingTarget : MonoBehaviour
    {
        public int pointsOverride = -1;
        public GameConfig gameConfig;
        public GameObject hitEffectPrefab;
        public AudioClip hitSound;
        public float respawnDelay = 2f;
        public bool respawns = true;

        Health health;
        Vector3 startPosition;
        Quaternion startRotation;
        MeshRenderer[] renderers;
        Collider[] colliders;

        void Awake()
        {
            health = GetComponent<Health>();
            renderers = GetComponentsInChildren<MeshRenderer>();
            colliders = GetComponentsInChildren<Collider>();
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        void OnEnable()
        {
            health.OnDeath.AddListener(HandleHit);
        }

        void OnDisable()
        {
            health.OnDeath.RemoveListener(HandleHit);
        }

        void HandleHit()
        {
            int points = pointsOverride >= 0 ? pointsOverride : (gameConfig != null ? gameConfig.pointsPerTargetHit : 50);
            ScoreManager.Instance?.AddScore(points);

            if (hitSound != null)
                AudioSource.PlayClipAtPoint(hitSound, transform.position);

            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

            SetVisible(false);

            if (respawns)
                Invoke(nameof(Respawn), respawnDelay);
        }

        void Respawn()
        {
            transform.SetPositionAndRotation(startPosition, startRotation);
            health.ResetHealth();
            SetVisible(true);
        }

        void SetVisible(bool visible)
        {
            foreach (var r in renderers) r.enabled = visible;
            foreach (var c in colliders) c.enabled = visible;
        }
    }
}
