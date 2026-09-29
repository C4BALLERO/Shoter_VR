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
        public float maxThrowRange = 14f;
        public float throwSpeed = 7f;
        public float minFlightTime = 0.9f;
        public float maxFlightTime = 2f;
        public float aimBelowHead = 0.4f;
        public float aimSpread = 0.35f;

        [Header("Animacion")]
        public Animator animator;
        public float turnSpeed = 6f;
        static readonly int MovingParam = Animator.StringToHash("Moving");
        static readonly int AttackParam = Animator.StringToHash("Attack");

        NavMeshAgent agent;
        Health health;
        float nextAttackTime;
        float nextThrowTime;
        DifficultyData difficulty;
        int level = 1;

        public System.Action<int> OnEnemyDefeated;

        float DamageMultiplier => difficulty != null ? difficulty.DamageMultiplier(level) : 1f;

        // Se llama justo despues de Instantiate (antes de Start) desde WaveManager.
        public void ApplyDifficulty(DifficultyData settings, int currentLevel)
        {
            difficulty = settings;
            level = Mathf.Max(1, currentLevel);
        }

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        void Start()
        {
            if (difficulty != null)
            {
                throwCooldown *= difficulty.ThrowCooldownMultiplier(level);
                aimSpread *= difficulty.aimSpreadMultiplier;
            }

            if (data != null)
            {
                float healthMult = difficulty != null ? difficulty.HealthMultiplier(level) : 1f;
                float speedMult = difficulty != null ? difficulty.SpeedMultiplier(level) : 1f;
                health.maxHealth = Mathf.Max(1, Mathf.RoundToInt(data.maxHealth * healthMult));
                health.ResetHealth();
                agent.speed = data.moveSpeed * speedMult;
            }

            health.OnDeath.AddListener(HandleDeath);

            // Desfase inicial para que una oleada no lance todas las hachas a la vez.
            nextThrowTime = Time.time + Random.Range(0.5f, throwCooldown);

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

                // Fuera del alcance cuerpo a cuerpo lanza hachas; no se exige
                // estar "bloqueado" porque al amontonarse contra la barrera los
                // agentes nunca llegan al final del camino y no lanzaban nunca.
                if (distance > data.attackRange + 0.5f && distance <= maxThrowRange)
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

        // Congela al enemigo mientras el menu de "has caido" esta abierto.
        public void SetFrozen(bool frozen)
        {
            enabled = !frozen;
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = frozen || agent.isStopped;
                if (frozen) agent.velocity = Vector3.zero;
            }
            if (animator != null) animator.speed = frozen ? 0f : 1f;

            // Al reanudar, que no lancen todos a la vez por el tiempo que estuvieron congelados.
            if (!frozen)
            {
                nextThrowTime = Time.time + Random.Range(1f, throwCooldown);
                nextAttackTime = Time.time + 1f;
            }
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

        void TryAttack()
        {
            if (Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + 1f;

            animator?.SetTrigger(AttackParam);

            var playerHealth = target.GetComponentInParent<IDamageable>();
            playerHealth?.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(data.attackDamage * DamageMultiplier)));
        }

        void TryThrow()
        {
            if (Time.time < nextThrowTime || projectilePrefab == null || target == null) return;
            nextThrowTime = Time.time + throwCooldown;

            animator?.SetTrigger(AttackParam);

            Vector3 origin = throwPoint != null ? throwPoint.position : transform.position + Vector3.up;
            Vector2 spread = Random.insideUnitCircle * aimSpread;
            Vector3 aimPoint = target.position + Vector3.down * aimBelowHead + new Vector3(spread.x, 0f, spread.y);
            Vector3 velocity = BallisticVelocity(origin, aimPoint);

            Vector3 flatDir = new Vector3(velocity.x, 0f, velocity.z);
            Quaternion rotation = flatDir.sqrMagnitude > 0.001f ? Quaternion.LookRotation(flatDir) : Quaternion.identity;
            var projectile = Instantiate(projectilePrefab, origin, rotation);

            var thrown = projectile.GetComponent<ThrownProjectile>();
            if (thrown != null)
                thrown.Launch(gameObject, velocity, DamageMultiplier);
            else if (projectile.TryGetComponent(out Rigidbody rb))
                rb.linearVelocity = velocity;
        }

        // Velocidad inicial que hace caer el proyectil justo en aimPoint: el
        // tiempo de vuelo crece con la distancia, asi el arco siempre supera
        // la barrera y el jugador tiene margen para esquivar.
        Vector3 BallisticVelocity(Vector3 origin, Vector3 aimPoint)
        {
            Vector3 delta = aimPoint - origin;
            float horizontal = new Vector2(delta.x, delta.z).magnitude;
            float flightTime = Mathf.Clamp(horizontal / Mathf.Max(0.1f, throwSpeed), minFlightTime, maxFlightTime);
            return delta / flightTime - 0.5f * Physics.gravity * flightTime;
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
