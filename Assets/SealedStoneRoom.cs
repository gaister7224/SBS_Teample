using UnityEngine;

public class SealedStoneRoom : MonoBehaviour
{
    public bool isFake = false;
    public WaveMonsterSpawn waveMonsterSpawn;

    private void Awake()
    {
        if (waveMonsterSpawn == null)
            waveMonsterSpawn = GetComponentInChildren<WaveMonsterSpawn>();
    }
}