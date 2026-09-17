using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Medallas.Data;

namespace Medallas.Weapon
{
    // Arma VR simple: se agarra con XR Grab Interactable y dispara por raycast
    // al presionar el gatillo (evento Activate de la interaccion). Sin proyectil
    // fisico para mantener el sistema simple y estable en Quest.
    [RequireComponent(typeof(XRGrabInteractable))]
    public class WeaponController : MonoBehaviour
    {
        public WeaponData weaponData;
        public AmmoSystem ammoSystem;
        public Transform muzzlePoint;
        public LayerMask hittableLayers = ~0;

        XRGrabInteractable grabInteractable;
        AudioSource audioSource;
        float nextFireTime;

        void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        void OnEnable()
        {
            grabInteractable.activated.AddListener(OnActivated);
        }

        void OnDisable()
        {
            grabInteractable.activated.RemoveListener(OnActivated);
        }

        void OnActivated(ActivateEventArgs args)
        {
            TryFire();
        }

        public void TryFire()
        {
            if (weaponData == null || Time.time < nextFireTime) return;
            nextFireTime = Time.time + weaponData.fireRate;

            if (ammoSystem == null || !ammoSystem.TryConsumeAmmo(1))
            {
                PlaySound(weaponData != null ? weaponData.emptySound : null);
                return;
            }

            Fire();
        }

        void Fire()
        {
            PlaySound(weaponData.fireSound);
            SpawnMuzzleFlash();

            Vector3 origin = muzzlePoint != null ? muzzlePoint.position : transform.position;
            Vector3 direction = muzzlePoint != null ? muzzlePoint.forward : transform.forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, weaponData.range, hittableLayers))
            {
                DamageSystem.ApplyDamage(hit.collider.gameObject, weaponData.damage);

                if (weaponData.impactEffectPrefab != null)
                    Object.Instantiate(weaponData.impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        void SpawnMuzzleFlash()
        {
            if (weaponData.muzzleFlashPrefab == null || muzzlePoint == null) return;
            Object.Instantiate(weaponData.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
        }

        void PlaySound(AudioClip clip)
        {
            if (clip == null) return;
            audioSource.PlayOneShot(clip);
        }
    }
}
