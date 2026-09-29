using System;
using System.Collections.Generic;
using UnityEngine;

namespace Medallas.Core
{
    public enum PowerUpType
    {
        DoubleDamage,
        Shield,
        RapidFire,
        Invulnerable
    }

    // Power-ups temporales del jugador. Las armas consultan DamageMultiplier /
    // FireRateMultiplier al disparar; el escudo ajusta el dano recibido en Health.
    public class PlayerPowerUps : MonoBehaviour
    {
        public static PlayerPowerUps Instance { get; private set; }

        public Health health;
        public float doubleDamageMultiplier = 2f;
        public float shieldDamageTakenMultiplier = 0.5f;
        public float rapidFireMultiplier = 2f;

        readonly Dictionary<PowerUpType, float> endTimes = new Dictionary<PowerUpType, float>();
        readonly List<PowerUpType> expired = new List<PowerUpType>();

        public event Action<PowerUpType, float> Activated;
        public event Action<PowerUpType> Expired;

        public float DamageMultiplier => IsActive(PowerUpType.DoubleDamage) ? doubleDamageMultiplier : 1f;
        public float FireRateMultiplier => IsActive(PowerUpType.RapidFire) ? rapidFireMultiplier : 1f;

        void Awake()
        {
            Instance = this;
            if (health == null) health = GetComponent<Health>();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool IsActive(PowerUpType type)
        {
            return endTimes.TryGetValue(type, out float end) && Time.time < end;
        }

        public float Remaining(PowerUpType type)
        {
            return endTimes.TryGetValue(type, out float end) ? Mathf.Max(0f, end - Time.time) : 0f;
        }

        public IEnumerable<PowerUpType> ActivePowerUps()
        {
            foreach (var kv in endTimes)
                if (Time.time < kv.Value) yield return kv.Key;
        }

        // Comprar el mismo power-up activo suma tiempo en vez de reiniciarlo.
        public void Activate(PowerUpType type, float duration)
        {
            float start = IsActive(type) ? endTimes[type] : Time.time;
            endTimes[type] = start + duration;
            RecomputeDamageTaken();
            Activated?.Invoke(type, Remaining(type));
        }

        void Update()
        {
            if (endTimes.Count == 0) return;

            expired.Clear();
            foreach (var kv in endTimes)
                if (Time.time >= kv.Value) expired.Add(kv.Key);

            foreach (var type in expired)
            {
                endTimes.Remove(type);
                Expired?.Invoke(type);
            }
            if (expired.Count > 0) RecomputeDamageTaken();
        }

        // Invulnerable (al revivir) manda sobre el escudo; sin ninguno, dano normal.
        void RecomputeDamageTaken()
        {
            if (health == null) return;
            if (IsActive(PowerUpType.Invulnerable)) health.incomingDamageMultiplier = 0f;
            else if (IsActive(PowerUpType.Shield)) health.incomingDamageMultiplier = shieldDamageTakenMultiplier;
            else health.incomingDamageMultiplier = 1f;
        }
    }
}
