using UnityEngine;
using UnityEngine.AI;
using Medallas.Core;
using Medallas.Data;

namespace Medallas.Enemies
{
    // IA simple: persigue al jugador cuando esta dentro del rango de deteccion
    // y ataca cuando esta dentro del rango de ataque. Sin maquina de estados
    // ni sensores complejos, para mantener estabilidad en Quest.
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        public EnemyData data;
        public Transform target;
        public int scoreValueOverride = -1;

        NavMeshAgent agent;
        Health health;
        float nextAttackTime;

        public System.Action<int> OnEnemyDefeated;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
        }

        void Start()
        {
            if (data != null)
            {
                health.maxHealth = data.maxHealth;
                health.ResetHealth();
                agent.speed = data.moveSpeed;
            }

            health.OnDeath.AddListener(HandleDeath);

            if (target == null && Camera.main != null)
                target = Camera.main.transform;
        }

        void Update()
        {
            if (health.IsDead || target == null || data == null) return;

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance <= data.detectionRange)
            {
                if (distance > data.attackRange)
                {
                    agent.isStopped = false;
                    agent.SetDestination(target.position);
                }
                else
                {
                    agent.isStopped = true;
                    TryAttack();
                }
            }
            else
            {
                agent.isStopped = true;
            }
        }

        void TryAttack()
        {
            if (Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + 1f;

            var playerHealth = target.GetComponentInParent<IDamageable>();
            playerHealth?.TakeDamage(data.attackDamage);
        }

        void HandleDeath()
        {
            agent.isStopped = true;
            int score = scoreValueOverride >= 0 ? scoreValueOverride : (data != null ? data.scoreValue : 0);
            OnEnemyDefeated?.Invoke(score);
            Destroy(gameObject, 1.5f);
        }
    }
}
