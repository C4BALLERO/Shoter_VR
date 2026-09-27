using System.IO;
using UnityEngine;
using Medallas.Medals;
using Medallas.Core;
using Medallas.Weapon;
using Medallas.RewardMachine;

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
            }

            if (ScoreManager.Instance != null)
                data.score = ScoreManager.Instance.CurrentScore;

            var ammoSystem = Object.FindFirstObjectByType<AmmoSystem>();
            if (ammoSystem != null)
                data.ammo = ammoSystem.CurrentAmmo;

            var rewardMachine = Object.FindFirstObjectByType<RewardMachineController>();
            if (rewardMachine != null)
                data.rewardGranted = rewardMachine.rewardGranted;

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log("[SaveSystem] Progreso guardado en " + SavePath);
        }

        public static bool HasSaveFile()
        {
            return File.Exists(SavePath);
        }

        public static void LoadGame()
        {
            if (!HasSaveFile())
            {
                Debug.LogWarning("[SaveSystem] No hay partida guardada.");
                return;
            }

            string json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<SaveData>(json);

            if (MedalManager.Instance != null)
                MedalManager.Instance.RestoreCollectedMedals(data.collectedMedalIds, data.spentMedals);

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.SetScore(data.score);

            var ammoSystem = Object.FindFirstObjectByType<AmmoSystem>();
            if (ammoSystem != null)
                ammoSystem.SetAmmo(data.ammo);

            var rewardMachine = Object.FindFirstObjectByType<RewardMachineController>();
            if (rewardMachine != null)
                rewardMachine.rewardGranted = data.rewardGranted;

            Debug.Log("[SaveSystem] Progreso cargado desde " + SavePath);
        }
    }
}
