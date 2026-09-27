using System;
using UnityEngine;
using UnityEngine.UI;
using Medallas.Core;
using Medallas.Data;
using Medallas.Enemies;
using Medallas.Weapon;

namespace Medallas.UI
{
    // Pantalla de inicio simple: mientras esta activa, las oleadas de
    // enemigos no arrancan y el jugador no tiene arma en mano. Se elige la
    // dificultad con botones fisicos y se cierra tocando el boton de inicio.
    public class StartMenuController : MonoBehaviour
    {
        public GameObject menuPanel;
        public WaveManager waveManager;
        public HUDController hud;
        public StartingWeapon startingWeapon;

        [Header("Dificultad")]
        public DifficultyData[] difficulties;
        public int selectedIndex = 1;
        public Health playerHealth;
        public Text difficultyText;
        [Tooltip("Botones de dificultad: se ocultan al empezar la partida.")]
        public GameObject difficultyButtonsRoot;

        public event Action<DifficultyData> DifficultyChanged;

        bool started;

        public DifficultyData SelectedDifficulty => GetDifficulty(selectedIndex);

        public DifficultyData GetDifficulty(int index)
        {
            if (difficulties == null || index < 0 || index >= difficulties.Length) return null;
            return difficulties[index];
        }

        void Start()
        {
            RefreshText();
        }

        public void SelectDifficulty(int index)
        {
            if (started || GetDifficulty(index) == null) return;
            selectedIndex = index;
            RefreshText();
            DifficultyChanged?.Invoke(SelectedDifficulty);
        }

        void RefreshText()
        {
            var d = SelectedDifficulty;
            if (difficultyText != null && d != null) difficultyText.text = "DIFICULTAD: " + d.displayName;
        }

        public void StartGame()
        {
            if (started) return;
            started = true;

            var d = SelectedDifficulty;
            if (d != null)
            {
                if (waveManager != null) waveManager.difficulty = d;
                if (playerHealth != null)
                {
                    playerHealth.maxHealth = d.playerMaxHealth;
                    playerHealth.ResetHealth();
                }
            }

            if (menuPanel != null) menuPanel.SetActive(false);
            if (difficultyButtonsRoot != null) difficultyButtonsRoot.SetActive(false);
            startingWeapon?.Equip();
            waveManager?.BeginGame();
            hud?.ShowMessage(d != null ? "COMIENZA LA PARTIDA - MODO " + d.displayName : "COMIENZA LA PARTIDA");
        }
    }
}
