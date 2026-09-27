using UnityEngine;
using Medallas.Weapon;

namespace Medallas.Enemies
{
    // Objeto que un enemigo lanza por encima de la barrera. Vuela con fisica
    // normal (Rigidbody) para que el jugador tenga tiempo de esquivarlo; al
    // chocar aplica dano y se destruye.
    [RequireComponent(typeof(Rigidbody))]
    public class ThrownProjectile : MonoBehaviour
    {
        public int damage = 5;
        public float lifeTime = 5f;
        public Vector3 spinAxis = Vector3.right;
        public float spinSpeed = 0f;

        GameObject owner;
        bool consumed;

        // Sin esto el proyectil nace dentro del collider del enemigo que lo
        // lanza y se destruye al instante contra su propio cuerpo.
        public void Launch(GameObject thrower, Vector3 velocity)
        {
            owner = thrower;
            if (owner != null)
            {
                var myColliders = GetComponentsInChildren<Collider>();
                foreach (var ownerCol in owner.GetComponentsInChildren<Collider>())
                    foreach (var myCol in myColliders)
                        Physics.IgnoreCollision(myCol, ownerCol);
            }

            var rb = GetComponent<Rigidbody>();
            rb.linearVelocity = velocity;
            if (spinSpeed != 0f)
                rb.angularVelocity = transform.TransformDirection(spinAxis.normalized) * spinSpeed;
        }

        void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        void OnCollisionEnter(Collision collision)
        {
            Hit(collision.gameObject, true);
        }

        // El jugador usa un collider trigger (para no empujar fisicamente
        // armas/medallas al caminar), asi que tambien hay que escuchar esto.
        void OnTriggerEnter(Collider other)
        {
            Hit(other.gameObject, false);
        }

        void Hit(GameObject other, bool solid)
        {
            if (consumed || other == owner) return;
            if (other.GetComponentInParent<EnemyAI>() != null) return;

            bool damaged = DamageSystem.ApplyDamage(other, damage);

            // Triggers que no son el jugador (zonas, agarres XR) no deben
            // hacer desaparecer el hacha en pleno vuelo.
            if (!damaged && !solid) return;

            consumed = true;
            Destroy(gameObject);
        }
    }
}
