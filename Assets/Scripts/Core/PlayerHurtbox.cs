using UnityEngine;

namespace Medallas.Core
{
    // El collider que recibe los golpes vive en la raiz del XR Origin, pero la
    // cabeza se mueve dentro del espacio de tracking (caminar en la habitacion
    // o WASD del simulador). Sin esto el jugador se aleja de su propio
    // hitbox y los proyectiles le pasan por encima sin hacer dano.
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerHurtbox : MonoBehaviour
    {
        public Transform head;
        public float minHeight = 1f;
        public float headroom = 0.15f;

        CapsuleCollider hurtbox;

        void Awake()
        {
            hurtbox = GetComponent<CapsuleCollider>();
            if (head == null && Camera.main != null) head = Camera.main.transform;
        }

        void LateUpdate()
        {
            if (head == null) return;

            Vector3 local = transform.InverseTransformPoint(head.position);
            float height = Mathf.Max(minHeight, local.y + headroom);
            hurtbox.height = height;
            hurtbox.center = new Vector3(local.x, height * 0.5f, local.z);
        }
    }
}
