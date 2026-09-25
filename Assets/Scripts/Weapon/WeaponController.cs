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
        bool triggerHeld;

        // Para el HUD: cualquier arma avisa cuando la agarran/sueltan, sin
        // necesidad de que el HUD conozca cada arma de la escena.
        public static event System.Action<WeaponData> WeaponEquipped;
        public static event System.Action<WeaponData> WeaponUnequipped;

        void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        void OnEnable()
        {
            grabInteractable.activated.AddListener(OnActivated);
            grabInteractable.deactivated.AddListener(OnDeactivated);
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }

        void OnDisable()
        {
            grabInteractable.activated.RemoveListener(OnActivated);
            grabInteractable.deactivated.RemoveListener(OnDeactivated);
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }

        void OnGrabbed(SelectEnterEventArgs args)
        {
            WeaponEquipped?.Invoke(weaponData);
        }

        void OnReleased(SelectExitEventArgs args)
        {
            WeaponUnequipped?.Invoke(weaponData);
        }

        void Update()
        {
            if (weaponData != null && weaponData.automatic && triggerHeld)
                TryFire();
        }

        void OnActivated(ActivateEventArgs args)
        {
            triggerHeld = true;
            TryFire();
        }

        void OnDeactivated(DeactivateEventArgs args)
        {
            triggerHeld = false;
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
            Vector3 baseDirection = muzzlePoint != null ? muzzlePoint.forward : transform.forward;

            int pellets = Mathf.Max(1, weaponData.pelletCount);
            for (int i = 0; i < pellets; i++)
            {
                Vector3 direction = ApplySpread(baseDirection, weaponData.spreadAngle);
                Vector3 tracerEnd = origin + direction * weaponData.range;

                if (Physics.Raycast(origin, direction, out RaycastHit hit, weaponData.range, hittableLayers))
                {
                    DamageSystem.ApplyDamage(hit.collider.gameObject, weaponData.damage);
                    tracerEnd = hit.point;

                    if (weaponData.impactEffectPrefab != null)
                        Object.Instantiate(weaponData.impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                }

                SpawnTracer(origin, tracerEnd);
            }
        }

        void SpawnTracer(Vector3 origin, Vector3 endPoint)
        {
            if (weaponData.bulletTracerPrefab == null) return;

            var tracerObj = Object.Instantiate(weaponData.bulletTracerPrefab, origin, Quaternion.identity);
            var tracer = tracerObj.GetComponent<BulletTracer>();
            if (tracer != null)
                tracer.Init(origin, endPoint, weaponData.tracerSpeed);
        }

        static Vector3 ApplySpread(Vector3 direction, float spreadAngle)
        {
            if (spreadAngle <= 0f) return direction;

            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spreadAngle;
            Quaternion spreadRotation = Quaternion.Euler(randomCircle.y, randomCircle.x, 0f);
            return spreadRotation * direction;
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
