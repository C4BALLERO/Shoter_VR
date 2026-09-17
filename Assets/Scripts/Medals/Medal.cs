using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Medallas.Data;

namespace Medallas.Medals
{
    // Objeto fisico agarrable en el mundo. Al agarrarla se registra en el
    // MedalManager y desaparece, dando feedback sonoro/visual.
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Medal : MonoBehaviour
    {
        public MedalData data;
        public AudioClip collectSound;
        public GameObject collectEffectPrefab;

        XRGrabInteractable grabInteractable;

        void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        void OnEnable()
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
        }

        void OnDisable()
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        }

        void OnGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
        {
            if (data == null || MedalManager.Instance == null) return;
            if (MedalManager.Instance.HasCollected(data.medalId)) return;

            MedalManager.Instance.CollectMedal(data);

            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position);

            if (collectEffectPrefab != null)
                Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
