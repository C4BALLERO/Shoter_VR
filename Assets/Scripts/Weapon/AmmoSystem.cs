using System;
using System.Collections;
using UnityEngine;
using Medallas.Data;

namespace Medallas.Weapon
{
    // Municion separada del controller para que UI y SaveSystem puedan
    // leerla/escribirla sin depender del arma concreta que la dispara.
    // CurrentAmmo es lo que hay en el cargador; ReserveAmmo lo que queda
    // para recargar.
    public class AmmoSystem : MonoBehaviour
    {
        public int magazineSize = 12;
        public int spareMagazines = 4;
        public bool infiniteReserve;

        public int CurrentAmmo { get; private set; }
        public int ReserveAmmo { get; private set; }
        public bool IsReloading { get; private set; }
        public bool InfiniteReserve => infiniteReserve;

        public event Action<int> OnAmmoChanged;
        public event Action<bool> OnReloadStateChanged;

        public void Configure(WeaponData data)
        {
            if (data == null) return;
            magazineSize = Mathf.Max(1, data.magazineSize);
            spareMagazines = Mathf.Max(0, data.spareMagazines);
            infiniteReserve = data.infiniteReserve;
        }

        void Start()
        {
            CurrentAmmo = magazineSize;
            ReserveAmmo = magazineSize * spareMagazines;
            OnAmmoChanged?.Invoke(CurrentAmmo);
        }

        void OnDisable()
        {
            // Si el arma se desactiva a media recarga la corrutina muere sola;
            // sin esto quedaria bloqueada en "recargando" para siempre.
            if (IsReloading) SetReloading(false);
        }

        public bool TryConsumeAmmo(int amount = 1)
        {
            if (IsReloading || CurrentAmmo < amount) return false;
            CurrentAmmo -= amount;
            OnAmmoChanged?.Invoke(CurrentAmmo);
            return true;
        }

        public bool CanReload => !IsReloading && CurrentAmmo < magazineSize && (infiniteReserve || ReserveAmmo > 0);

        public bool Reload(float duration)
        {
            if (!CanReload) return false;
            StartCoroutine(ReloadRoutine(duration));
            return true;
        }

        IEnumerator ReloadRoutine(float duration)
        {
            SetReloading(true);
            yield return new WaitForSeconds(duration);

            int needed = magazineSize - CurrentAmmo;
            int taken = infiniteReserve ? needed : Mathf.Min(needed, ReserveAmmo);
            if (!infiniteReserve) ReserveAmmo -= taken;
            CurrentAmmo += taken;

            SetReloading(false);
            OnAmmoChanged?.Invoke(CurrentAmmo);
        }

        void SetReloading(bool value)
        {
            IsReloading = value;
            OnReloadStateChanged?.Invoke(value);
        }

        public void AddAmmo(int amount)
        {
            ReserveAmmo += amount;
            OnAmmoChanged?.Invoke(CurrentAmmo);
        }

        public void SetAmmo(int amount)
        {
            CurrentAmmo = Mathf.Clamp(amount, 0, magazineSize);
            OnAmmoChanged?.Invoke(CurrentAmmo);
        }
    }
}
