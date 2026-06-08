using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class BarrelSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float weight;
    }

    public SpawnEntry[] entries;
    public float spawnInterval = 2f;
    public float positionJitter = 0.05f;
    private float _timer;

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            Spawn();
        }
    }

    void Spawn()
    {
        GameObject prefab = PickWeightedRandom();
        if (prefab != null)
        {
            Vector3 offset = new Vector3(
                Random.Range(-positionJitter, positionJitter),
                0f,
                Random.Range(-positionJitter, positionJitter)
            );
            Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(prefab, transform.position + offset, randomRot);
        }
    }

    GameObject PickWeightedRandom()
    {
        float totalWeight = 0f;
        foreach (var entry in entries)
        {
            totalWeight += entry.weight;
        }
        float rand = Random.Range(0f, totalWeight);
        float cumWeight = 0f;
        foreach (var entry in entries)
        {
            cumWeight += entry.weight;
            if (rand <= cumWeight)
            {
                return entry.prefab;
            }
        }
        return null;
    }
}
