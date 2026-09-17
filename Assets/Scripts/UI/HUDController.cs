using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Medallas.Medals;
using Medallas.Core;
using Medallas.Weapon;
using Medallas.RewardMachine;
using Medallas.Data;

namespace Medallas.UI
{
    // HUD VR simple (Canvas en World Space frente al jugador). Se limita a
    // escuchar los eventos de los managers existentes, sin duplicar estado.
    public class HUDController : MonoBehaviour
    {
        public Text medalsText;
        public Text pointsText;
        public Text ammoText;
        public Text messageText;
        public AmmoSystem ammoSystem;
        public RewardMachineController rewardMachine;
        public float messageDuration = 3f;

        Coroutine messageRoutine;

        void OnEnable()
        {
            if (MedalManager.Instance != null)
            {
                MedalManager.Instance.OnMedalCountChanged += HandleMedalCountChanged;
                MedalManager.Instance.OnMedalCollected += HandleMedalCollected;
                HandleMedalCountChanged(MedalManager.Instance.CollectedCount, MedalManager.Instance.TotalCount);
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
                HandleScoreChanged(ScoreManager.Instance.CurrentScore);
            }

            if (ammoSystem != null)
            {
                ammoSystem.OnAmmoChanged += HandleAmmoChanged;
            }

            if (rewardMachine != null)
            {
                rewardMachine.onNotEnoughMedals.AddListener(HandleNotEnoughMedals);
                rewardMachine.onRewardGranted.AddListener(HandleRewardGranted);
            }
        }

        void OnDisable()
        {
            if (MedalManager.Instance != null)
            {
                MedalManager.Instance.OnMedalCountChanged -= HandleMedalCountChanged;
                MedalManager.Instance.OnMedalCollected -= HandleMedalCollected;
            }

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;

            if (ammoSystem != null)
                ammoSystem.OnAmmoChanged -= HandleAmmoChanged;

            if (rewardMachine != null)
            {
                rewardMachine.onNotEnoughMedals.RemoveListener(HandleNotEnoughMedals);
                rewardMachine.onRewardGranted.RemoveListener(HandleRewardGranted);
            }
        }

        void HandleMedalCollected(MedalData medal)
        {
            ShowMessage("MEDALLA OBTENIDA");
        }

        void HandleMedalCountChanged(int current, int total)
        {
            if (medalsText != null) medalsText.text = $"MEDALLAS: {current} / {total}";
        }

        void HandleScoreChanged(int score)
        {
            if (pointsText != null) pointsText.text = $"PUNTOS: {score}";
        }

        void HandleAmmoChanged(int ammo)
        {
            if (ammoText != null) ammoText.text = $"MUNICION: {ammo}";
        }

        void HandleNotEnoughMedals()
        {
            ShowMessage("NECESITAS MAS MEDALLAS");
        }

        void HandleRewardGranted()
        {
            ShowMessage("MAQUINA ACTIVADA - RECOMPENSA OBTENIDA");
        }

        public void ShowMessage(string text)
        {
            if (messageText == null) return;
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            messageRoutine = StartCoroutine(ShowMessageRoutine(text));
        }

        IEnumerator ShowMessageRoutine(string text)
        {
            messageText.text = text;
            yield return new WaitForSeconds(messageDuration);
            messageText.text = string.Empty;
        }
    }
}
