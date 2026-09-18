using UnityEngine;

namespace Medallas.Data
{
    // Configuracion de un arma: permite ajustar balance (dano, cadencia,
    // municion) desde el inspector sin recompilar ni tocar WeaponController.
    [CreateAssetMenu(fileName = "Weapon_00", menuName = "Medallas/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        public string weaponName;
        public int damage = 10;
        public int magazineSize = 10;
        public float fireRate = 0.25f;
        public float range = 50f;
        public bool automatic = false;
        public int pelletCount = 1;
        public float spreadAngle = 0f;
        public AudioClip fireSound;
        public AudioClip emptySound;
        public GameObject muzzleFlashPrefab;
        public GameObject impactEffectPrefab;
    }
}
