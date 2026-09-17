using System;
using UnityEngine;
using Medallas.Data;

namespace Medallas.Weapon
{
    // Municion separada del controller para que UI y SaveSystem puedan
    // leerla/escribirla sin depender del arma concreta que la dispara.
    public class AmmoSystem : MonoBehaviour
    {
        public GameConfig gameConfig;

        public int CurrentAmmo { get; private set; }
        public event Action<int> OnAmmoChanged;

        void Start()
        {
            CurrentAmmo = gameConfig != null ? gameConfig.startingAmmo : 30;
            OnAmmoChanged?.Invoke(CurrentAmmo);
        }

        public bool TryConsumeAmmo(int amount = 1)
        {
            if (CurrentAmmo < amount) return false;
            CurrentAmmo -= amount;
            OnAmmoChanged?.Invoke(CurrentAmmo);
            return true;
        }

        public void AddAmmo(int amount)
        {
            CurrentAmmo += amount;
            OnAmmoChanged?.Invoke(CurrentAmmo);
        }

        public void SetAmmo(int amount)
        {
            CurrentAmmo = amount;
            OnAmmoChanged?.Invoke(CurrentAmmo);
        }
    }
}
