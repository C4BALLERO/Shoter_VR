using System;
using System.IO;
using UnityEngine;
using Medallas.Medals;
using Medallas.Core;
using Medallas.Weapon;
using Medallas.RewardMachine;
using Medallas.UI;

namespace Medallas.SaveSystem
{
    // Guardado/carga real en JSON (no solo variables en memoria). Reune el
    // estado de los managers de la escena en un SaveData y lo persiste en
    // Application.persistentDataPath, que sobrevive a cerrar y reabrir la app.
    public static class SaveSystem
    {
        static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

        public static void SaveGame()
        {
            var data = new SaveData();

            if (MedalManager.Instance != null)
            {
                data.collectedMedalIds.AddRange(MedalManager.Instance.GetCollectedMedalIds());
                data.spentMedals = MedalManager.Instance.SpentCount;
                data.bonusMedals = MedalManager.Instance.BonusCount;
            }

            if (ScoreManager.Instance != null)
            {
                data.score = ScoreManager.Instance.CurrentScore;
                data.kills = ScoreManager.Instance.Kills;
            }

            var ammoSystem = UnityEngine.Object.FindFirstObjectByType<AmmoSystem>();
            if (ammoSystem != null)
                data.ammo = ammoSystem.CurrentAmmo;

            var rewardMachine = UnityEngine.Object.FindFirstObjectByType<RewardMachineController>();
            if (rewardMachine != null)
                data.rewardGranted = rewardMachine.rewardGranted;

            var menu = UnityEngine.Object.FindFirstObjectByType<StartMenuController>();
            if (menu != null)
            {
                data.level = menu.ResumeLevel;
                data.difficultyIndex = menu.selectedIndex;
            }

            data.savedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log("[SaveSystem] Progreso guardado en " + SavePath);
        }

        public static bool HasSaveFile()
        {
            return File.Exists(SavePath);
        }

        public static bool TryReadSave(out SaveData data)
        {
            data = null;
            if (!HasSaveFile()) return false;

            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch (Exception e)
            {
                // Un archivo corrupto no debe bloquear el menu: se trata como "sin partida".
                Debug.LogWarning("[SaveSystem] No se pudo leer la partida guardada: " + e.Message);
                data = null;
            }
            return data != null;
        }

        public static void DeleteSave()
        {
            if (HasSaveFile()) File.Delete(SavePath);
        }

        public static void LoadGame()
        {
            if (!TryReadSave(out var data))
            {
                Debug.LogWarning("[SaveSystem] No hay partida guardada.");
                return;
            }

            if (MedalManager.Instance != null)
                MedalManager.Instance.RestoreCollectedMedals(data.collectedMedalIds, data.spentMedals, data.bonusMedals);

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.SetScore(data.score);
                ScoreManager.Instance.SetKills(data.kills);
            }

            var ammoSystem = UnityEngine.Object.FindFirstObjectByType<AmmoSystem>();
            if (ammoSystem != null)
                ammoSystem.SetAmmo(data.ammo);

            var rewardMachine = UnityEngine.Object.FindFirstObjectByType<RewardMachineController>();
            if (rewardMachine != null)
                rewardMachine.rewardGranted = data.rewardGranted;

            Debug.Log("[SaveSystem] Progreso cargado desde " + SavePath);
        }
    }
}
