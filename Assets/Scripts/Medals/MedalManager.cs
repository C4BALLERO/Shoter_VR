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
        int spentMedals;
        int bonusMedals;

        public event Action<MedalData> OnMedalCollected;
        public event Action<int, int> OnMedalCountChanged;
        public event Action<int> OnBonusMedalsAdded;

        public int CollectedCount => collectedMedalIds.Count;
        // Gastar medallas (maquina expendedora) no borra las recogidas: la
        // maquina de recompensa final cuenta las recogidas, no el saldo.
        public int SpentCount => spentMedals;
        // Medallas extra de los niveles 2+: solo suman saldo para gastar, no
        // cuentan como coleccionables unicas.
        public int BonusCount => bonusMedals;
        public int AvailableCount => Mathf.Max(0, CollectedCount + bonusMedals - spentMedals);
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

        public bool TrySpendMedals(int amount)
        {
            if (amount <= 0) return true;
            if (AvailableCount < amount) return false;

            spentMedals += amount;
            OnMedalCountChanged?.Invoke(CollectedCount, TotalCount);
            return true;
        }

        public void AddBonusMedals(int amount)
        {
            if (amount <= 0) return;
            bonusMedals += amount;
            OnBonusMedalsAdded?.Invoke(amount);
            OnMedalCountChanged?.Invoke(CollectedCount, TotalCount);
        }

        // Usado por el SaveSystem para restaurar el progreso.
        public void RestoreCollectedMedals(IEnumerable<string> medalIds, int spent, int bonus)
        {
            collectedMedalIds.Clear();
            foreach (var id in medalIds) collectedMedalIds.Add(id);
            bonusMedals = Mathf.Max(0, bonus);
            spentMedals = Mathf.Clamp(spent, 0, collectedMedalIds.Count + bonusMedals);
            OnMedalCountChanged?.Invoke(CollectedCount, TotalCount);
        }

        public IReadOnlyCollection<string> GetCollectedMedalIds() => collectedMedalIds;
    }
}
