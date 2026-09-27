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

        [Header("Progresion por nivel (al continuar tras las 5 oleadas)")]
        public float levelHealthIncrease = 0.25f;
        public float levelDamageIncrease = 0.15f;
        public float levelSpeedIncrease = 0.05f;
        [Tooltip("Cada nivel multiplica el tiempo entre hachas por este factor.")]
        public float levelThrowCooldownFactor = 0.9f;
        public int levelExtraEnemies = 1;
        public int maxEnemiesPerWave = 12;

        int Steps(int level) => Mathf.Max(0, level - 1);

        public float HealthMultiplier(int level) => enemyHealthMultiplier * (1f + levelHealthIncrease * Steps(level));
        public float DamageMultiplier(int level) => enemyDamageMultiplier * (1f + levelDamageIncrease * Steps(level));
        // Tope de velocidad: mas alla los agentes del NavMesh se ven erraticos.
        public float SpeedMultiplier(int level) => enemySpeedMultiplier * Mathf.Min(1.5f, 1f + levelSpeedIncrease * Steps(level));
        public float ThrowCooldownMultiplier(int level) => Mathf.Max(0.35f, throwCooldownMultiplier * Mathf.Pow(levelThrowCooldownFactor, Steps(level)));
        public int EnemiesPerWave(int level) => Mathf.Min(maxEnemiesPerWave, enemiesPerWave + levelExtraEnemies * Steps(level));
    }
}
