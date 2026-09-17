using System;
using UnityEngine;

namespace Medallas.Core
{
    // Punto unico de puntuacion. Los objetivos de la galeria y los enemigos
    // reportan aqui en vez de manejar su propio contador de puntos.
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public int CurrentScore { get; private set; }
        public event Action<int> OnScoreChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddScore(int amount)
        {
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void SetScore(int amount)
        {
            CurrentScore = amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
