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
        public Text weaponText;
        public Text messageText;
        public AmmoSystem ammoSystem;
        public RewardMachineController rewardMachine;
        public Health playerHealth;
        public Image healthBarFill;
        public float messageDuration = 3f;

        Coroutine messageRoutine;
        AmmoSystem boundAmmo;

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

            BindAmmo(ammoSystem);

            WeaponController.WeaponEquipped += HandleWeaponEquipped;
            WeaponController.WeaponUnequipped += HandleWeaponUnequipped;
            WeaponController.DryFired += HandleDryFired;
            if (weaponText != null) weaponText.text = "ARMA: NINGUNA";

            if (playerHealth != null)
            {
                playerHealth.OnDamaged.AddListener(HandleHealthChanged);
                HandleHealthChanged(playerHealth.CurrentHealth);
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

            BindAmmo(null);

            WeaponController.WeaponEquipped -= HandleWeaponEquipped;
            WeaponController.WeaponUnequipped -= HandleWeaponUnequipped;
            WeaponController.DryFired -= HandleDryFired;

            if (playerHealth != null)
                playerHealth.OnDamaged.RemoveListener(HandleHealthChanged);

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

        void HandleMedalCountChanged(int collected, int total)
        {
            if (medalsText == null) return;
            int available = MedalManager.Instance != null ? MedalManager.Instance.AvailableCount : collected;
            medalsText.text = $"MEDALLAS: {available} ({collected}/{total})";
        }

        void HandleScoreChanged(int score)
        {
            if (pointsText != null) pointsText.text = $"PUNTOS: {score}";
        }

        void BindAmmo(AmmoSystem ammo)
        {
            if (boundAmmo != null)
            {
                boundAmmo.OnAmmoChanged -= HandleAmmoChanged;
                boundAmmo.OnReloadStateChanged -= HandleReloadStateChanged;
            }

            boundAmmo = ammo;

            if (boundAmmo != null)
            {
                boundAmmo.OnAmmoChanged += HandleAmmoChanged;
                boundAmmo.OnReloadStateChanged += HandleReloadStateChanged;
                HandleAmmoChanged(boundAmmo.CurrentAmmo);
            }
        }

        void HandleAmmoChanged(int magazine)
        {
            if (ammoText == null || boundAmmo == null) return;
            string reserve = boundAmmo.InfiniteReserve ? "∞" : boundAmmo.ReserveAmmo.ToString();
            ammoText.text = $"MUNICION: {magazine} / {reserve}";
        }

        void HandleReloadStateChanged(bool reloading)
        {
            if (reloading) ShowMessage("RECARGANDO...");
            else if (messageText != null && messageText.text == "RECARGANDO...") messageText.text = string.Empty;
        }

        void HandleDryFired(WeaponController weapon)
        {
            if (weapon.ammoSystem != null && weapon.ammoSystem.CanReload)
                ShowMessage("SIN BALAS - RECARGA CON M (o boton A)");
            else
                ShowMessage("SIN MUNICION - CONSIGUE OTRA ARMA");
        }

        void HandleWeaponEquipped(WeaponController weapon)
        {
            var data = weapon.weaponData;
            if (weaponText != null) weaponText.text = "ARMA: " + (data != null ? data.weaponName.ToUpper() : "?");
            BindAmmo(weapon.ammoSystem);
        }

        void HandleWeaponUnequipped(WeaponController weapon)
        {
            if (weaponText != null) weaponText.text = "ARMA: NINGUNA";
        }

        void HandleHealthChanged(int current)
        {
            if (healthBarFill == null || playerHealth == null) return;
            healthBarFill.fillAmount = playerHealth.maxHealth > 0 ? Mathf.Clamp01((float)current / playerHealth.maxHealth) : 0f;

            if (current <= 0)
                ShowMessage("HAS CAIDO");
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
