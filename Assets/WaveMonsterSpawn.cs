using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public List<GameObject> spawnPrefabs = new List<GameObject>();
}

public class WaveMonsterSpawn : MonoBehaviour
{
    public List<WaveData> waves = new List<WaveData>();
    public float waveClearDelay = 2f;
    public float gridSpacing = 2f;

    public Vector3 spawnPos;

    public List<GameObject> currentAliveMonsters = new List<GameObject>();

    public int currentWave = 0;
    public bool isAllWavesCleared = false;

    StageManager stageManager;

    private bool isWaitingNextWave = false;

    private void Awake()
    {
        stageManager = StageManager.instance;
    }

    public void StartWaves()
    {
        currentWave = 0;
        isAllWavesCleared = false;
        isWaitingNextWave = false;
        currentAliveMonsters.Clear();
        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        while (currentWave < waves.Count)
        {
            SpawnWave(currentWave);

            yield return new WaitUntil(() => IsWaveCleared());

            currentWave++;

            if (currentWave < waves.Count)
                yield return new WaitForSeconds(waveClearDelay);
        }

        isAllWavesCleared = true;

        if (stageManager != null)
        {
            stageManager.activePortal = true;
            stageManager.curStageCleared = true;
        }
    }

    void SpawnWave(int waveIndex)
    {
        if (waves == null || waveIndex >= waves.Count)
            return;

        WaveData wave = waves[waveIndex];

        List<GameObject> validPrefabs = new List<GameObject>();
        for (int i = 0; i < wave.spawnPrefabs.Count; i++)
        {
            if (wave.spawnPrefabs[i] != null)
                validPrefabs.Add(wave.spawnPrefabs[i]);
        }

        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(validPrefabs.Count));
        int count = 0;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                if (count >= validPrefabs.Count)
                    break;

                Vector3 offset = new Vector3(x * gridSpacing, 0f, z * gridSpacing);
                GameObject monster = Instantiate(validPrefabs[count], spawnPos + offset, Quaternion.identity, transform);
                currentAliveMonsters.Add(monster);
                count++;
            }
        }
    }

    bool IsWaveCleared()
    {
        PruneDeadMonsters();
        return currentAliveMonsters.Count == 0;
    }

    void PruneDeadMonsters()
    {
        for (int i = currentAliveMonsters.Count - 1; i >= 0; i--)
        {
            if (currentAliveMonsters[i] == null)
                currentAliveMonsters.RemoveAt(i);
        }
    }
}