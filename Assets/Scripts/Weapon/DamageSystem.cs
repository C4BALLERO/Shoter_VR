using UnityEngine;
using Medallas.Core;

namespace Medallas.Weapon
{
    // Punto unico por el que pasa todo dano infligido por armas. Evita que
    // cada arma tenga que buscar y castear IDamageable por su cuenta, y deja
    // un solo lugar donde agregar reglas futuras (criticos, resistencias).
    public static class DamageSystem
    {
        public static bool ApplyDamage(GameObject target, int amount)
        {
            if (target == null) return false;

            var damageable = target.GetComponentInParent<IDamageable>();
            if (damageable == null || damageable.IsDead) return false;

            damageable.TakeDamage(amount);
            return true;
        }
    }
}
