using UnityEngine;

namespace Medallas.Data
{
    // Configuracion general ajustable sin tocar codigo: cuantas medallas hacen
    // falta para activar la maquina, municion inicial, etc.
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Medallas/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Medallas")]
        public int totalMedalCount = 8;
        public int medalsRequiredForReward = 5;

        [Header("Puntuacion")]
        public int pointsPerTargetHit = 50;
        public int pointsPerEnemyDefeated = 100;
        public int pointsPerChallengeCompleted = 250;

        [Header("Comfort VR")]
        public float moveSpeed = 1.5f;
        public bool snapTurnEnabled = true;
        public float snapTurnAngle = 45f;
    }
}
