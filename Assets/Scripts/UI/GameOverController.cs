using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Medallas.Core;
using Medallas.Enemies;
using Medallas.Medals;
using Saves = Medallas.SaveSystem.SaveSystem;

namespace Medallas.UI
{
    // Al quedarse sin vida congela el combate y muestra, frente al jugador,
    // dos opciones: reiniciar el nivel desde su punto guardado, o pagar
    // medallas para revivir en el mismo punto de la oleada.
    public class GameOverController : MonoBehaviour
    {
        public Health playerHealth;
        public WaveManager waveManager;
        public StartMenuController startMenu;
        public HUDController hud;
        public PlayerPowerUps powerUps;

        [Header("Menu (tablero fisico con botones XR)")]
        public GameObject menuRoot;
        public XRSimpleInteractable restartButton;
        public XRSimpleInteractable continueButton;
        public Text continueLabel;
        public Text infoText;
        public float menuDistance = 1.3f;
        public float menuHeightOffset = 0.02f;

        [Header("Continuar con medallas")]
        public int continueCost = 2;
        public float invulnerableSeconds = 3f;

        [Header("Sonidos")]
        public AudioClip reviveSound;
        public AudioClip denySound;

        public event Action Opened;
        public event Action<bool> Closed; // true = revivio con medallas

        public bool IsOpen { get; private set; }

        AudioSource audioSource;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            if (menuRoot != null) menuRoot.SetActive(false);
        }

        void OnEnable()
        {
            if (playerHealth != null) playerHealth.OnDeath.AddListener(Open);
            if (restartButton != null) restartButton.selectEntered.AddListener(OnRestartPressed);
            if (continueButton != null) continueButton.selectEntered.AddListener(OnContinuePressed);
        }

        void OnDisable()
        {
            if (playerHealth != null) playerHealth.OnDeath.RemoveListener(Open);
            if (restartButton != null) restartButton.selectEntered.RemoveListener(OnRestartPressed);
            if (continueButton != null) continueButton.selectEntered.RemoveListener(OnContinuePressed);
        }

        void OnRestartPressed(SelectEnterEventArgs args) => RestartLevel();
        void OnContinuePressed(SelectEnterEventArgs args) => ContinueWithMedals();

        public void Open()
        {
            if (IsOpen) return;
            if (startMenu != null && startMenu.CurrentState != StartMenuController.State.Playing) return;

            IsOpen = true;
            waveManager?.SetPaused(true);
            PlaceInFrontOfPlayer();
            RefreshTexts();
            if (menuRoot != null) menuRoot.SetActive(true);
            hud?.ShowMessage(string.Empty);
            hud?.SetVisible(false);
            Opened?.Invoke();
        }

        void PlaceInFrontOfPlayer()
        {
            var cam = Camera.main;
            if (menuRoot == null || cam == null) return;

            Vector3 forward = cam.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            // Si hay una pared mas cerca que menuDistance, el tablero quedaria dentro
            // de ella: se acerca. Solo cuentan colliders estaticos (sin Rigidbody),
            // asi el arma en la mano no lo empuja contra la cara.
            float distance = menuDistance;
            Vector3 origin = cam.transform.position + Vector3.up * menuHeightOffset;
            foreach (var hit in Physics.RaycastAll(origin, forward, menuDistance + 0.25f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.attachedRigidbody != null) continue;
                distance = Mathf.Min(distance, Mathf.Max(0.5f, hit.distance - 0.25f));
            }

            menuRoot.transform.position = origin + forward * distance;
            menuRoot.transform.rotation = Quaternion.LookRotation(forward);
        }

        void RefreshTexts()
        {
            int available = MedalManager.Instance != null ? MedalManager.Instance.AvailableCount : 0;
            if (continueLabel != null)
                continueLabel.text = $"CONTINUAR\n{continueCost} MEDALLAS (tienes {available})";
            if (infoText != null)
            {
                int level = waveManager != null ? waveManager.level : 1;
                infoText.text = $"Reiniciar: vuelves al inicio del nivel {level}\nContinuar: revives aqui con la vida llena";
            }
        }

        public void RestartLevel()
        {
            if (!IsOpen) return;
            Close(false);

            // Vuelve al punto guardado al empezar el nivel (medallas, puntos, bajas).
            if (Saves.HasSaveFile()) Saves.LoadGame();
            startMenu?.RestorePlayerHealth();
            waveManager?.RestartLevel();
            int level = waveManager != null ? waveManager.level : 1;
            hud?.ShowMessage($"REINTENTANDO NIVEL {level}");
        }

        public void ContinueWithMedals()
        {
            if (!IsOpen) return;

            var medals = MedalManager.Instance;
            if (medals == null || !medals.TrySpendMedals(continueCost))
            {
                if (denySound != null) audioSource.PlayOneShot(denySound);
                if (infoText != null) infoText.text = $"NECESITAS {continueCost} MEDALLAS\nToca REINICIAR para volver a intentarlo";
                return;
            }

            Close(true);
            startMenu?.RestorePlayerHealth();
            powerUps?.Activate(PowerUpType.Invulnerable, invulnerableSeconds);
            waveManager?.SetPaused(false);
            if (reviveSound != null) audioSource.PlayOneShot(reviveSound);
            hud?.ShowMessage($"DE VUELTA AL COMBATE  -{continueCost} MEDALLAS");
        }

        void Close(bool revived)
        {
            IsOpen = false;
            if (menuRoot != null) menuRoot.SetActive(false);
            hud?.SetVisible(true);
            Closed?.Invoke(revived);
        }
    }
}
