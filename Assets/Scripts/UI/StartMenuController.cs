using System;
using UnityEngine;
using UnityEngine.UI;
using Medallas.Core;
using Medallas.Data;
using Medallas.Enemies;
using Medallas.Weapon;
using Saves = Medallas.SaveSystem.SaveSystem;

namespace Medallas.UI
{
    // Flujo de partida con botones fisicos:
    //  - Menu: elegir dificultad y "Nueva partida" (boton verde) o "Continuar"
    //    (boton azul, solo si hay partida guardada).
    //  - Al terminar las oleadas de un nivel se autoguarda y el boton verde
    //    pasa a "Siguiente nivel", que arranca uno mas dificil.
    public class StartMenuController : MonoBehaviour
    {
        public enum State { Menu, Playing, LevelComplete }

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

        [Header("Textos del panel y botones")]
        public Text titleText;
        public Text subtitleText;
        public Text startButtonLabel;
        public GameObject continueButton;
        public Text continueButtonLabel;

        public event Action<DifficultyData> DifficultyChanged;

        public State CurrentState { get; private set; } = State.Menu;
        public DifficultyData SelectedDifficulty => GetDifficulty(selectedIndex);

        // Nivel desde el que se retomaria si se guarda ahora.
        public int ResumeLevel
        {
            get
            {
                if (waveManager == null) return 1;
                return CurrentState == State.LevelComplete ? waveManager.level + 1 : waveManager.level;
            }
        }

        public DifficultyData GetDifficulty(int index)
        {
            if (difficulties == null || index < 0 || index >= difficulties.Length) return null;
            return difficulties[index];
        }

        void OnEnable()
        {
            if (waveManager != null) waveManager.LevelCompleted += HandleLevelCompleted;
        }

        void OnDisable()
        {
            if (waveManager != null) waveManager.LevelCompleted -= HandleLevelCompleted;
        }

        void Start()
        {
            ShowMainMenu();
        }

        void ShowMainMenu()
        {
            CurrentState = State.Menu;
            if (menuPanel != null) menuPanel.SetActive(true);
            if (difficultyButtonsRoot != null) difficultyButtonsRoot.SetActive(true);
            SetText(titleText, "MEDALLAS: ZONA PERDIDA");
            SetText(startButtonLabel, "NUEVA PARTIDA");

            bool hasSave = Saves.TryReadSave(out var save);
            SetText(subtitleText, hasSave
                ? "Verde: nueva partida  -  Azul: continuar la guardada"
                : "Elige la dificultad y toca el boton verde");

            if (continueButton != null) continueButton.SetActive(hasSave);
            if (hasSave)
            {
                var d = GetDifficulty(save.difficultyIndex);
                SetText(continueButtonLabel, $"CONTINUAR\nNIVEL {save.level} - {(d != null ? d.displayName : "?")}");
            }
            RefreshDifficultyText();
        }

        public void SelectDifficulty(int index)
        {
            if (CurrentState != State.Menu || GetDifficulty(index) == null) return;
            selectedIndex = index;
            RefreshDifficultyText();
            DifficultyChanged?.Invoke(SelectedDifficulty);
        }

        void RefreshDifficultyText()
        {
            var d = SelectedDifficulty;
            if (d != null) SetText(difficultyText, "DIFICULTAD: " + d.displayName);
        }

        // Boton verde: "Nueva partida" en el menu, "Siguiente nivel" al completar uno.
        public void StartGame()
        {
            switch (CurrentState)
            {
                case State.Menu:
                    NewGame();
                    break;
                case State.LevelComplete:
                    NextLevel();
                    break;
            }
        }

        void NewGame()
        {
            if (waveManager != null) waveManager.level = 1;
            BeginPlaying();
            // Nueva partida reemplaza la guardada desde el primer momento.
            Saves.SaveGame();
        }

        // Boton azul: retoma la partida guardada en el nivel donde quedo.
        public void ContinueGame()
        {
            if (CurrentState != State.Menu || !Saves.TryReadSave(out var save)) return;

            if (GetDifficulty(save.difficultyIndex) != null)
            {
                selectedIndex = save.difficultyIndex;
                DifficultyChanged?.Invoke(SelectedDifficulty);
            }
            Saves.LoadGame();
            if (waveManager != null) waveManager.level = Mathf.Max(1, save.level);
            BeginPlaying();
        }

        void BeginPlaying()
        {
            CurrentState = State.Playing;
            var d = SelectedDifficulty;
            if (waveManager != null) waveManager.difficulty = d;
            RestorePlayerHealth();

            if (menuPanel != null) menuPanel.SetActive(false);
            if (difficultyButtonsRoot != null) difficultyButtonsRoot.SetActive(false);
            if (continueButton != null) continueButton.SetActive(false);
            SetText(startButtonLabel, "");

            startingWeapon?.Equip();
            waveManager?.BeginGame();
            int level = waveManager != null ? waveManager.level : 1;
            hud?.ShowMessage($"NIVEL {level} - MODO {(d != null ? d.displayName : "")}");
        }

        void NextLevel()
        {
            CurrentState = State.Playing;
            RestorePlayerHealth();
            if (menuPanel != null) menuPanel.SetActive(false);
            SetText(startButtonLabel, "");
            waveManager?.BeginNextLevel();
        }

        void HandleLevelCompleted(int level)
        {
            CurrentState = State.LevelComplete;
            Saves.SaveGame();

            if (menuPanel != null) menuPanel.SetActive(true);
            SetText(titleText, $"NIVEL {level} COMPLETADO");
            SetText(subtitleText, "Partida guardada. Toca el boton verde para seguir");
            SetText(difficultyText, $"SIGUIENTE: NIVEL {level + 1} (MAS DIFICIL)");
            SetText(startButtonLabel, "SIGUIENTE NIVEL");
            hud?.ShowMessage($"NIVEL {level} COMPLETADO");
        }

        // Cada nivel empieza con la vida completa (la del modo elegido).
        public void RestorePlayerHealth()
        {
            if (playerHealth == null) return;
            var d = SelectedDifficulty;
            if (d != null) playerHealth.maxHealth = d.playerMaxHealth;
            playerHealth.ResetHealth();
        }

        static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }
    }
}
