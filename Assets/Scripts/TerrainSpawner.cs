using UnityEngine;
using System.Collections.Generic;

public class TerrainSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] terrainChunks;
    [SerializeField] private Transform player;
    [SerializeField] private int chunksAhead = 5;

    public Queue<GameObject> spawnedChunks = new Queue<GameObject>();
    private int chunkIndex = 0;
    private Vector3 nextSpawnPosition = Vector3.zero;

    private bool isTimeTrial = false;
    private int maxChunksTimeTrial = 4;

    private void Start()
    {
        Invoke(nameof(CheckForMode), 2f);
        // Spawn first chunk
        GameObject firstChunk = SpawnChunk(terrainChunks[0], nextSpawnPosition);
        spawnedChunks.Enqueue(firstChunk);
        nextSpawnPosition = GetEndPoint(firstChunk);
        chunkIndex++;

        // Spawn rest
        for (int i = 1; i < chunksAhead; i++)
        {
            SpawnNextChunk();
        }
    }

    void CheckForMode()
    {
        // Get the game mode status
        GameModeManager gm = FindObjectOfType<GameModeManager>();

        if (gm != null && gm.currentGameMode == GameModeManager.GameMode.TimeTrial)
        {
            isTimeTrial = true;
        }
    }

    private void Update()
    {
        if (isTimeTrial && spawnedChunks.Count >= maxChunksTimeTrial)
            return;

        if (player.position.x > nextSpawnPosition.x - chunksAhead * 10f)
        {
            SpawnNextChunk();
        }
    }

    private void SpawnNextChunk()
    {
        if (chunkIndex >= terrainChunks.Length)
            chunkIndex = 0;

        nextSpawnPosition.Set(nextSpawnPosition.x, 0, 0);

        GameObject prefab = terrainChunks[chunkIndex];
        GameObject newChunk = SpawnChunk(prefab, nextSpawnPosition);
        spawnedChunks.Enqueue(newChunk);
        nextSpawnPosition = GetEndPoint(newChunk);
        chunkIndex++;

        if (!isTimeTrial && spawnedChunks.Count > chunksAhead + 2)
        {
            GameObject oldChunk = spawnedChunks.Dequeue();
            Destroy(oldChunk);
        }
    }

    private GameObject SpawnChunk(GameObject prefab, Vector3 targetPosition)
    {
        GameObject temp = Instantiate(prefab);
        Transform startPoint = temp.transform.Find("StartPoint");

        if (startPoint == null)
        {
            Debug.LogWarning("StartPoint not found on prefab " + prefab.name);
            return temp;
        }

        startPoint.position.Set(startPoint.position.x, 0, 0);
        Vector3 offset = temp.transform.position - startPoint.position;
        temp.transform.position = targetPosition + offset;

        return temp;
    }

    private Vector3 GetEndPoint(GameObject chunk)
    {
        Transform endPoint = chunk.transform.Find("EndPoint");
        if (endPoint != null)
        {
            return endPoint.position;
        }
        else
        {
            Debug.LogWarning("EndPoint not found on chunk " + chunk.name);
            return chunk.transform.position + Vector3.right * 10f;
        }
    }
}
