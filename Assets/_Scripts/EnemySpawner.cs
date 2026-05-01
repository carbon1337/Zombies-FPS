using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemyPrefabs;
    public List<Transform> spawnPoints;

    private int enemiesLeft;
    private bool spawning;
    public float waveDelay = 3f;

    void Start()
    {
        GameManager.Instance.OnRoundChanged += StartWave;
    }

    private void StartWave()
    {
        int round = GameManager.Instance.currentRound;
        enemiesLeft = (round + 1) * 3; // Example: Increase enemy count with each round
        spawning = true;
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (enemiesLeft > 0 && spawning)
        {
            yield return new WaitForSeconds(5f); // Adjust this delay as needed
            
            // Randomly select an enemy prefab and spawn point
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            enemiesLeft--;

        }
    }
}