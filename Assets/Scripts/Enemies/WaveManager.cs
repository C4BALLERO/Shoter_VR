using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Medallas.Data;
using Medallas.Medals;
using Medallas.UI;

namespace Medallas.Enemies
{
    // Hace aparecer enemigos por oleadas desde el fondo de la zona de
    // enemigos. Algunos enemigos cargan un medallon (ver MedalCarrier) que
    // reparte las medallas configuradas. Reutiliza el mismo EnemyAI/Health
    // de siempre; solo controla cuando y donde aparecen.
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

        readonly List<GameObject> aliveEnemies = new List<GameObject>();
        int medalIndex;
        bool started;

        // Las oleadas no arrancan solas: las dispara el menu de inicio
        // (ver StartMenuController) para que el jugador tenga tiempo de
        // ubicarse antes de que aparezcan enemigos.
        public void BeginGame()
        {
            if (started) return;
            started = true;
            if (difficulty != null) enemiesPerWave = difficulty.enemiesPerWave;
            StartCoroutine(RunWaves());
        }

        IEnumerator RunWaves()
        {
            for (int wave = 1; wave <= totalWaves; wave++)
            {
                Debug.Log($"[WaveManager] Iniciando oleada {wave}/{totalWaves}");
                hud?.ShowMessage($"OLEADA {wave} / {totalWaves}");

                yield return StartCoroutine(SpawnWave());
                yield return new WaitUntil(AllEnemiesGone);
                yield return new WaitForSeconds(delayBetweenWaves);
            }

            Debug.Log("[WaveManager] Todas las oleadas completadas.");
            hud?.ShowMessage("TODAS LAS OLEADAS COMPLETADAS");
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
                ai.ApplyDifficulty(difficulty);
                ai.OnEnemyDefeated += HandleEnemyDefeated;
            }

            if (medalIndex < medalsToDistribute.Length && Random.value < 0.6f)
            {
                AttachMedal(enemy, medalsToDistribute[medalIndex]);
                medalIndex++;
            }
        }

        void HandleEnemyDefeated(int score)
        {
            Medallas.Core.ScoreManager.Instance?.RegisterKill(score);
        }

        void AttachMedal(GameObject enemy, MedalData medal)
        {
            if (medalCarrierVisualPrefab == null) return;

            var visual = Instantiate(medalCarrierVisualPrefab, enemy.transform);
            visual.transform.localPosition = new Vector3(0, 1.2f, 0);
            visual.transform.localScale = Vector3.one * 0.18f;

            var carrier = visual.GetComponent<MedalCarrier>();
            if (carrier == null) carrier = visual.AddComponent<MedalCarrier>();
            carrier.data = medal;
        }
    }
}
