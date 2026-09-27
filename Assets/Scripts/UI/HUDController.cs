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
        public Text killsText;
        public Image weaponIcon;
        public Text ammoText;
        public Text weaponText;
        public Text messageText;
        public AmmoSystem ammoSystem;
        public RewardMachineController rewardMachine;
        public Health playerHealth;
        public Image healthBarFill;
        public Text healthText;
        public Text powerUpsText;
        public float messageDuration = 3f;

        Coroutine messageRoutine;
        AmmoSystem boundAmmo;

        void OnEnable()
        {
            if (MedalManager.Instance != null)
            {
                MedalManager.Instance.OnMedalCountChanged += HandleMedalCountChanged;
                MedalManager.Instance.OnMedalCollected += HandleMedalCollected;
                MedalManager.Instance.OnBonusMedalsAdded += HandleBonusMedals;
                HandleMedalCountChanged(MedalManager.Instance.CollectedCount, MedalManager.Instance.TotalCount);
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
                ScoreManager.Instance.OnKillsChanged += HandleKillsChanged;
                HandleScoreChanged(ScoreManager.Instance.CurrentScore);
                HandleKillsChanged(ScoreManager.Instance.Kills);
            }

            BindAmmo(ammoSystem);

            WeaponController.WeaponEquipped += HandleWeaponEquipped;
            WeaponController.WeaponUnequipped += HandleWeaponUnequipped;
            WeaponController.DryFired += HandleDryFired;
            ShowWeapon(null);

            if (playerHealth != null)
            {
                playerHealth.OnDamaged.AddListener(HandleHealthChanged);
                playerHealth.OnHealed.AddListener(HandleHealthChanged);
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
                MedalManager.Instance.OnBonusMedalsAdded -= HandleBonusMedals;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
                ScoreManager.Instance.OnKillsChanged -= HandleKillsChanged;
            }

            BindAmmo(null);

            WeaponController.WeaponEquipped -= HandleWeaponEquipped;
            WeaponController.WeaponUnequipped -= HandleWeaponUnequipped;
            WeaponController.DryFired -= HandleDryFired;

            if (playerHealth != null)
            {
                playerHealth.OnDamaged.RemoveListener(HandleHealthChanged);
                playerHealth.OnHealed.RemoveListener(HandleHealthChanged);
            }

            if (rewardMachine != null)
            {
                rewardMachine.onNotEnoughMedals.RemoveListener(HandleNotEnoughMedals);
                rewardMachine.onRewardGranted.RemoveListener(HandleRewardGranted);
            }
        }

        readonly System.Text.StringBuilder powerUpBuilder = new System.Text.StringBuilder();
        float nextPowerUpRefresh;

        void Update()
        {
            if (powerUpsText == null || Time.time < nextPowerUpRefresh) return;
            nextPowerUpRefresh = Time.time + 0.2f;

            var powerUps = PlayerPowerUps.Instance;
            powerUpBuilder.Clear();
            if (powerUps != null)
            {
                foreach (var type in powerUps.ActivePowerUps())
                {
                    if (powerUpBuilder.Length > 0) powerUpBuilder.Append('\n');
                    powerUpBuilder.Append(PowerUpLabel(type)).Append(' ').Append(Mathf.CeilToInt(powerUps.Remaining(type))).Append('s');
                }
            }
            powerUpsText.text = powerUpBuilder.ToString();
        }

        static string PowerUpLabel(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.DoubleDamage: return "DAÑO x2";
                case PowerUpType.Shield: return "ESCUDO";
                case PowerUpType.RapidFire: return "DISPARO RAPIDO";
                default: return type.ToString();
            }
        }

        void HandleMedalCollected(MedalData medal)
        {
            ShowMessage("MEDALLA OBTENIDA");
        }

        void HandleBonusMedals(int amount)
        {
            ShowMessage($"MEDALLA EXTRA +{amount}");
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

        void HandleKillsChanged(int kills)
        {
            if (killsText != null) killsText.text = $"BAJAS: {kills}";
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
            ammoText.text = $"{magazine} / {reserve}";
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
            ShowWeapon(weapon.weaponData);
            BindAmmo(weapon.ammoSystem);
        }

        void HandleWeaponUnequipped(WeaponController weapon)
        {
            ShowWeapon(null);
        }

        void ShowWeapon(WeaponData data)
        {
            if (weaponText != null) weaponText.text = data != null ? data.weaponName.ToUpper() : "SIN ARMA";
            if (weaponIcon != null)
            {
                weaponIcon.sprite = data != null ? data.icon : null;
                weaponIcon.enabled = weaponIcon.sprite != null;
            }
        }

        void HandleHealthChanged(int current)
        {
            if (healthBarFill == null || playerHealth == null) return;
            float fraction = playerHealth.maxHealth > 0 ? Mathf.Clamp01((float)current / playerHealth.maxHealth) : 0f;
            healthBarFill.fillAmount = fraction;
            if (healthText != null) healthText.text = $"VIDA {Mathf.RoundToInt(fraction * 100f)}%";

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
