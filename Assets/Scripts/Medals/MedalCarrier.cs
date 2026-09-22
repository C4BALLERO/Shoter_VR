using UnityEngine;
using Medallas.Core;
using Medallas.Data;

namespace Medallas.Medals
{
    // Medallon visible que cargan algunos enemigos. Es un objetivo separado
    // del cuerpo del enemigo: un solo impacto lo entrega como medalla y lo
    // destruye, sin depender de la salud/muerte del enemigo que lo porta.
    public class MedalCarrier : MonoBehaviour, IDamageable
    {
        public MedalData data;

        public bool IsDead { get; private set; }

        public void TakeDamage(int amount)
        {
            if (IsDead || data == null) return;
            IsDead = true;

            MedalManager.Instance?.CollectMedal(data);
            Destroy(gameObject);
        }
    }
}
