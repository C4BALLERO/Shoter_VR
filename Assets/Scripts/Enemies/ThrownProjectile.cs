using UnityEngine;
using Medallas.Weapon;

namespace Medallas.Enemies
{
    // Objeto simple que un enemigo lanza por encima de la barrera. Vuela con
    // fisica normal (Rigidbody) para que el jugador tenga tiempo de esquivarlo;
    // al chocar aplica dano y se destruye.
    [RequireComponent(typeof(Rigidbody))]
    public class ThrownProjectile : MonoBehaviour
    {
        public int damage = 5;
        public float lifeTime = 5f;

        void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        void OnCollisionEnter(Collision collision)
        {
            DamageSystem.ApplyDamage(collision.gameObject, damage);
            Destroy(gameObject);
        }
    }
}
