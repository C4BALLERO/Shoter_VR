using UnityEngine;

namespace Medallas.Data
{
    // Balance de un tipo de enemigo (salud, velocidad, dano, rangos de deteccion
    // y ataque) editable sin tocar el script de IA.
    [CreateAssetMenu(fileName = "Enemy_00", menuName = "Medallas/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName;
        public int maxHealth = 30;
        public float moveSpeed = 2f;
        public int attackDamage = 10;
        public float attackRange = 1.5f;
        public float detectionRange = 8f;
        public int scoreValue = 100;
    }
}
