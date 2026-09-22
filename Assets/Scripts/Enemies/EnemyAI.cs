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

        [Header("Ataque a distancia (cuando no puede acercarse, ej. detras de una barrera)")]
        public GameObject projectilePrefab;
        public Transform throwPoint;
        public float throwCooldown = 2.5f;
        public float throwSpeed = 6f;
        public float throwArcBoost = 3f;

        [Header("Animacion")]
        public Animator animator;
        public float turnSpeed = 6f;
        static readonly int MovingParam = Animator.StringToHash("Moving");
        static readonly int AttackParam = Animator.StringToHash("Attack");

        NavMeshAgent agent;
        Health health;
        float nextAttackTime;
        float nextThrowTime;

        public System.Action<int> OnEnemyDefeated;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
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
                    FaceTarget();
                    TryAttack();
                }

                // Si no logra acercarse lo suficiente (ej. bloqueado por una
                // barrera que el NavMesh no puede cruzar), ataca a distancia.
                if (distance > data.attackRange && AgentIsBlocked())
                {
                    FaceTarget();
                    TryThrow();
                }
            }
            else
            {
                agent.isStopped = true;
            }

            UpdateAnimator();
        }

        void UpdateAnimator()
        {
            if (animator == null) return;
            bool moving = !agent.isStopped && agent.velocity.sqrMagnitude > 0.05f;
            animator.SetBool(MovingParam, moving);
        }

        void FaceTarget()
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }

        bool AgentIsBlocked()
        {
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;
        }

        void TryAttack()
        {
            if (Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + 1f;

            animator?.SetTrigger(AttackParam);

            var playerHealth = target.GetComponentInParent<IDamageable>();
            playerHealth?.TakeDamage(data.attackDamage);
        }

        void TryThrow()
        {
            if (Time.time < nextThrowTime || projectilePrefab == null || target == null) return;
            nextThrowTime = Time.time + throwCooldown;

            animator?.SetTrigger(AttackParam);

            Vector3 origin = throwPoint != null ? throwPoint.position : transform.position + Vector3.up;
            var projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);

            var rb = projectile.GetComponent<Rigidbody>();
            if (rb == null) return;

            Vector3 toTarget = target.position - origin;
            rb.linearVelocity = toTarget.normalized * throwSpeed + Vector3.up * throwArcBoost;
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
