using System;
using System.Collections.Generic;
using UnityEngine;
using Medallas.Data;

namespace Medallas.Medals
{
    // Punto unico de verdad de las medallas recogidas. UI, SaveSystem y la
    // maquina de recompensa consultan este manager en vez de contar objetos
    // en la escena.
    public class MedalManager : MonoBehaviour
    {
        public static MedalManager Instance { get; private set; }

        public GameConfig gameConfig;

        readonly HashSet<string> collectedMedalIds = new HashSet<string>();

        public event Action<MedalData> OnMedalCollected;
        public event Action<int, int> OnMedalCountChanged;

        public int CollectedCount => collectedMedalIds.Count;
        public int TotalCount => gameConfig != null ? gameConfig.totalMedalCount : collectedMedalIds.Count;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool HasCollected(string medalId)
        {
            return collectedMedalIds.Contains(medalId);
        }

        public void CollectMedal(MedalData medal)
        {
            if (medal == null || collectedMedalIds.Contains(medal.medalId)) return;

            collectedMedalIds.Add(medal.medalId);
            OnMedalCollected?.Invoke(medal);
            OnMedalCountChanged?.Invoke(CollectedCount, TotalCount);
        }

        public bool HasEnoughForReward()
        {
            return gameConfig != null && CollectedCount >= gameConfig.medalsRequiredForReward;
        }

        // Usado por el SaveSystem para restaurar el progreso.
        public void RestoreCollectedMedals(IEnumerable<string> medalIds)
        {
            collectedMedalIds.Clear();
            foreach (var id in medalIds) collectedMedalIds.Add(id);
            OnMedalCountChanged?.Invoke(CollectedCount, TotalCount);
        }

        public IReadOnlyCollection<string> GetCollectedMedalIds() => collectedMedalIds;
    }
}
