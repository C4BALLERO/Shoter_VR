using UnityEngine;

namespace Medallas.Weapon
{
    // Efecto simple y generico (flash de disparo o chispa de impacto):
    // aparece y se autodestruye solo, sin logica de juego.
    public class MuzzleFlashEffect : MonoBehaviour
    {
        public float lifetime = 0.08f;

        void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
