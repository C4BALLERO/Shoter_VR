using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Medallas.Medals;
using Medallas.Data;

namespace Medallas.RewardMachine
{
    // Mecanica central: el jugador interactua con la maquina (ej. tirar de una
    // palanca / tocar un boton XR Simple Interactable). Si ya tiene suficientes
    // medallas entrega la recompensa; si no, avisa cuantas faltan.
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class RewardMachineController : MonoBehaviour
    {
        public RewardData reward;
        public Transform rewardSpawnPoint;
        public bool rewardGranted;

        public UnityEvent onActivated;
        public UnityEvent onNotEnoughMedals;
        public UnityEvent onRewardGranted;

        XRSimpleInteractable interactable;

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        void OnEnable()
        {
            interactable.selectEntered.AddListener(OnActivated);
        }

        void OnDisable()
        {
            interactable.selectEntered.RemoveListener(OnActivated);
        }

        void OnActivated(SelectEnterEventArgs args)
        {
            TryActivateMachine();
        }

        public void TryActivateMachine()
        {
            if (rewardGranted) return;
            onActivated?.Invoke();

            if (MedalManager.Instance == null || !MedalManager.Instance.HasEnoughForReward())
            {
                onNotEnoughMedals?.Invoke();
                return;
            }

            GrantReward();
        }

        void GrantReward()
        {
            rewardGranted = true;

            if (reward != null && reward.rewardPrefab != null)
            {
                Vector3 pos = rewardSpawnPoint != null ? rewardSpawnPoint.position : transform.position;
                Instantiate(reward.rewardPrefab, pos, Quaternion.identity);
            }

            onRewardGranted?.Invoke();
        }
    }
}
