using UnityEngine;
using UnityEngine.Events;

namespace Medallas.Core
{
    // Componente de salud reutilizable: lo usan tanto enemigos como objetivos
    // de la galeria de tiro, evitando duplicar la logica de "recibir dano y morir".
    public class Health : MonoBehaviour, IDamageable
    {
        public int maxHealth = 30;
        [Tooltip("1 = dano normal; el power-up de escudo lo baja a 0.5.")]
        public float incomingDamageMultiplier = 1f;
        public UnityEvent<int> OnDamaged;
        public UnityEvent<int> OnHealed;
        public UnityEvent OnDeath;

        int currentHealth;

        public bool IsDead { get; private set; }
        public int CurrentHealth => currentHealth;

        void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            if (incomingDamageMultiplier != 1f)
                amount = Mathf.Max(1, Mathf.RoundToInt(amount * incomingDamageMultiplier));

            currentHealth = Mathf.Max(0, currentHealth - amount);
            OnDamaged?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                IsDead = true;
                OnDeath?.Invoke();
            }
        }

        public bool IsFull => currentHealth >= maxHealth;

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealed?.Invoke(currentHealth);
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            IsDead = false;
            OnHealed?.Invoke(currentHealth);
        }
    }
}
