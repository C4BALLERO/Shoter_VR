using UnityEngine;
using UnityEngine.UI;
using Medallas.Core;

namespace Medallas.Enemies
{
    // Barra de vida sobre la cabeza del enemigo: escucha el mismo componente
    // Health que ya usa el sistema de dano, y gira para mirar siempre al jugador
    // (billboard), igual que un enemigo tipico de shooter VR.
    public class EnemyHealthBar : MonoBehaviour
    {
        public Health health;
        public Image fillImage;
        public Canvas canvas;

        Camera mainCamera;

        void Awake()
        {
            if (health == null) health = GetComponentInParent<Health>();
            mainCamera = Camera.main;
            if (canvas != null) canvas.enabled = false;
        }

        void OnEnable()
        {
            if (health != null)
            {
                health.OnDamaged.AddListener(HandleDamaged);
                health.OnDeath.AddListener(HandleDeath);
            }
        }

        void OnDisable()
        {
            if (health != null)
            {
                health.OnDamaged.RemoveListener(HandleDamaged);
                health.OnDeath.RemoveListener(HandleDeath);
            }
        }

        void LateUpdate()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }

        void HandleDamaged(int currentHealth)
        {
            if (fillImage == null || health == null) return;
            fillImage.fillAmount = (float)currentHealth / health.maxHealth;
            if (canvas != null) canvas.enabled = currentHealth < health.maxHealth;
        }

        void HandleDeath()
        {
            if (canvas != null) canvas.enabled = false;
        }
    }
}
