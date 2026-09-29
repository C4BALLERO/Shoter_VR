using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Medallas.Data;
using Medallas.Medals;
using Medallas.UI;
using Random = UnityEngine.Random;

namespace Medallas.Enemies
{
    // Hace aparecer enemigos por oleadas desde el fondo de la zona de
    // enemigos. Un "nivel" son totalWaves oleadas; al terminarlas avisa con
    // LevelCompleted y el jugador decide si continua al siguiente (mas dificil).
    // Algunos enemigos cargan un medallon (ver MedalCarrier).
    public class WaveManager : MonoBehaviour
    {
        public GameObject[] enemyPrefabs;
        public Transform[] spawnPoints;
        public MedalData[] medalsToDistribute;
        public GameObject medalCarrierVisualPrefab;

        public int enemiesPerWave = 3;
        public int totalWaves = 4;
        public float spawnInterval = 2f;
        public float delayBetweenWaves = 4f;
        public HUDController hud;
        [Tooltip("La asigna StartMenuController segun el modo elegido.")]
        public DifficultyData difficulty;

        [Header("Niveles")]
        public int level = 1;
        [Tooltip("Medallas extra (solo saldo) por nivel a partir del nivel 2.")]
        public int bonusMedalsPerLevel = 4;

        public event Action<int> LevelCompleted;

        readonly List<GameObject> aliveEnemies = new List<GameObject>();
        int medalIndex;
        int bonusMedalsThisLevel;
        bool running;
        bool paused;

        public bool IsRunning => running;
        public bool IsPaused => paused;

        // Pausa el combate sin tocar Time.timeScale (congelar el tiempo tambien
        // congelaria el simulador XR y el jugador no podria apuntar al menu).
        public void SetPaused(bool value)
        {
            paused = value;
            aliveEnemies.RemoveAll(e => e == null);
            foreach (var enemy in aliveEnemies)
            {
                var ai = enemy.GetComponent<EnemyAI>();
                if (ai != null) ai.SetFrozen(value);
            }
            ClearProjectiles();
        }

        // Vuelve a empezar el nivel actual desde la primera oleada.
        public void RestartLevel()
        {
            StopAllCoroutines();
            aliveEnemies.RemoveAll(e => e == null);
            foreach (var enemy in aliveEnemies) Destroy(enemy);
            aliveEnemies.Clear();
            ClearProjectiles();
            running = false;
            paused = false;
            // Las medallas unicas ya recogidas se saltan al repartir (ver SpawnOne).
            medalIndex = 0;
            BeginGame();
        }

        static void ClearProjectiles()
        {
            foreach (var p in FindObjectsByType<ThrownProjectile>(FindObjectsSortMode.None))
                Destroy(p.gameObject);
        }

        // Las oleadas no arrancan solas: las dispara el menu de inicio
        // (ver StartMenuController) para que el jugador tenga tiempo de
        // ubicarse antes de que aparezcan enemigos.
        public void BeginGame()
        {
            if (running) return;
            running = true;
            bonusMedalsThisLevel = 0;
            if (difficulty != null) enemiesPerWave = difficulty.EnemiesPerWave(level);
            StartCoroutine(RunWaves());
        }

        public void BeginNextLevel()
        {
            if (running) return;
            level++;
            BeginGame();
        }

        IEnumerator RunWaves()
        {
            for (int wave = 1; wave <= totalWaves; wave++)
            {
                Debug.Log($"[WaveManager] Nivel {level} - oleada {wave}/{totalWaves}");
                hud?.ShowMessage($"NIVEL {level} - OLEADA {wave} / {totalWaves}");

                yield return StartCoroutine(SpawnWave());
                yield return new WaitUntil(AllEnemiesGone);
                if (wave < totalWaves) yield return new WaitForSeconds(delayBetweenWaves);
                yield return new WaitWhile(() => paused);
            }

            Debug.Log($"[WaveManager] Nivel {level} completado.");
            running = false;
            LevelCompleted?.Invoke(level);
        }

        bool AllEnemiesGone()
        {
            aliveEnemies.RemoveAll(e => e == null);
            return aliveEnemies.Count == 0;
        }

        IEnumerator SpawnWave()
        {
            for (int i = 0; i < enemiesPerWave; i++)
            {
                yield return new WaitWhile(() => paused);
                SpawnOne();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        void SpawnOne()
        {
            if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            var enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            aliveEnemies.Add(enemy);

            var ai = enemy.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.ApplyDifficulty(difficulty, level);
                ai.OnEnemyDefeated += HandleEnemyDefeated;
            }

            if (Random.value >= 0.6f) return;

            // Al continuar una partida guardada, no repartir medallas ya recogidas.
            while (medalIndex < medalsToDistribute.Length && MedalManager.Instance != null
                   && MedalManager.Instance.HasCollected(medalsToDistribute[medalIndex].medalId))
                medalIndex++;

            if (medalIndex < medalsToDistribute.Length)
            {
                AttachMedal(enemy, medalsToDistribute[medalIndex], false);
                medalIndex++;
            }
            else if (level > 1 && bonusMedalsThisLevel < bonusMedalsPerLevel)
            {
                AttachMedal(enemy, null, true);
                bonusMedalsThisLevel++;
            }
        }

        void HandleEnemyDefeated(int score)
        {
            Medallas.Core.ScoreManager.Instance?.RegisterKill(score);
        }

        void AttachMedal(GameObject enemy, MedalData medal, bool bonus)
        {
            if (medalCarrierVisualPrefab == null) return;

            var visual = Instantiate(medalCarrierVisualPrefab, enemy.transform);
            visual.transform.localPosition = new Vector3(0, 1.2f, 0);
            visual.transform.localScale = Vector3.one * 0.18f;

            var carrier = visual.GetComponent<MedalCarrier>();
            if (carrier == null) carrier = visual.AddComponent<MedalCarrier>();
            carrier.data = medal;
            carrier.bonus = bonus;
        }
    }
}
