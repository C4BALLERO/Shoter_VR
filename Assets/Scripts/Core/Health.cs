using UnityEngine;
using UnityEngine.Events;

namespace Medallas.Core
{
    // Componente de salud reutilizable: lo usan tanto enemigos como objetivos
    // de la galeria de tiro, evitando duplicar la logica de "recibir dano y morir".
    public class Health : MonoBehaviour, IDamageable
    {
        public int maxHealth = 30;
        public UnityEvent<int> OnDamaged;
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

            currentHealth -= amount;
            OnDamaged?.Invoke(currentHealth);

            if (currentHealth <= 0)
            {
                IsDead = true;
                OnDeath?.Invoke();
            }
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            IsDead = false;
        }
    }
}
