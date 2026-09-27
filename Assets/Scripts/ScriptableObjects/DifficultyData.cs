using UnityEngine;

namespace Medallas.Data
{
    // Un asset por modo (Facil/Medio/Dificil): el balance de cada modo se
    // ajusta desde el inspector sin tocar EnemyAI ni WaveManager.
    [CreateAssetMenu(fileName = "Difficulty_00", menuName = "Medallas/Difficulty Data")]
    public class DifficultyData : ScriptableObject
    {
        public string displayName = "MEDIO";
        public Color buttonColor = Color.yellow;

        [Header("Jugador")]
        public int playerMaxHealth = 100;

        [Header("Enemigos (multiplicadores sobre EnemyData)")]
        public float enemyHealthMultiplier = 1f;
        public float enemyDamageMultiplier = 1f;
        public float enemySpeedMultiplier = 1f;
        [Tooltip(">1 lanzan menos seguido, <1 mas seguido.")]
        public float throwCooldownMultiplier = 1f;
        [Tooltip(">1 fallan mas, <1 apuntan mejor.")]
        public float aimSpreadMultiplier = 1f;

        [Header("Oleadas")]
        public int enemiesPerWave = 5;
    }
}
