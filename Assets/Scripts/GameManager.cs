using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GridGenerator3D gridGenerator;

    [Header("Wave Settings")]
    public float waveInterval = 5f; // Time between waves
    public float warningDuration = 3f; // How long tiles flash before dropping

    private int currentWave = 0;
    private List<RockTile> activeTiles;

    public GameObject treasureChestPrefab; // Assign in Inspector
    public WaveTimer3D waveTimer;          // Reference to timer UI

    // Spawn a treasure chest at a random remaining tile after wave 2
    private void SpawnTreasureChest()
    {
        if (treasureChestPrefab == null)
        {
            Debug.LogWarning("Treasure chest prefab not assigned.");
            return;
        }
        if (activeTiles == null || activeTiles.Count == 0)
        {
            Debug.LogWarning("No remaining tiles to spawn treasure chest.");
            return;
        }

        // Pick a random tile from the remaining active rock tiles (still bricks)
        var tile = activeTiles[Random.Range(0, activeTiles.Count)];
        Vector3 spawnPos = tile.transform.position + Vector3.up * 0.5f; // slight offset above tile
        Instantiate(treasureChestPrefab, spawnPos, Quaternion.identity);
        Debug.Log("[GameManager] Treasure chest spawned.");
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (gridGenerator == null) gridGenerator = FindObjectOfType<GridGenerator3D>();
        if (waveTimer == null) waveTimer = FindObjectOfType<WaveTimer3D>();

        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        Debug.Log("GameLoop Started. Waiting for grid...");

        float timeout = 15f;
        float elapsed = 0f;

        while (true)
        {
            if (gridGenerator == null)
                gridGenerator = FindObjectOfType<GridGenerator3D>();

            if (gridGenerator != null)
            {
                var tiles = gridGenerator.GetAllRockTiles();
                if (tiles != null && tiles.Count > 0)
                {
                    activeTiles = new List<RockTile>(tiles);
                    Debug.Log($"GameManager found {activeTiles.Count} tiles.");
                    break;
                }
            }

            elapsed += Time.deltaTime;
            if (elapsed >= timeout)
            {
                Debug.LogError("No tiles found after waiting for grid; Wave System cannot start.");
                yield break;
            }

            yield return null;
        }

        int initialTileCount = activeTiles.Count;
        int dropAmountPerWave = initialTileCount / 4;

        // --- Wave 1 ---
        currentWave = 1;
        Debug.Log("Wave 1 Starting");
        yield return StartCoroutine(HandleWave(dropAmountPerWave + 2, dropAmountPerWave));
        yield return StartCoroutine(WaitInterval(waveInterval));

        // --- Wave 2 ---
        currentWave = 2;
        Debug.Log("Wave 2 Starting");
        yield return StartCoroutine(HandleWave(dropAmountPerWave + 2, dropAmountPerWave));
        yield return StartCoroutine(WaitInterval(waveInterval));

        // After wave 2, spawn a treasure chest at a random remaining tile
        SpawnTreasureChest();

        // --- Wave 3 ---
        currentWave = 3;
        Debug.Log("Wave 3 Starting");
        int tilesToDrop = activeTiles.Count - 2;
        if (tilesToDrop > 0)
        {
            yield return StartCoroutine(HandleWave(activeTiles.Count, tilesToDrop));
        }
        yield return StartCoroutine(WaitInterval(waveInterval));

        // --- Wave 4 ---
        currentWave = 4;
        Debug.Log("Wave 4 Starting");
        if (activeTiles.Count > 1)
        {
            yield return StartCoroutine(HandleWave(activeTiles.Count, 1));
        }

        if (waveTimer != null) waveTimer.SetTime(0);
        Debug.Log("Game Over - Winner Determined!");
    }

    IEnumerator WaitInterval(float duration)
    {
        float timer = duration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            if (waveTimer != null) waveTimer.SetTime(timer);
            yield return null;
        }
    }

    // 🔄 NEW: all tiles blink during warning, only subset collapses
    IEnumerator HandleWave(int tilesToFlashCount, int tilesToDropCount)
    {
        if (activeTiles == null || activeTiles.Count == 0)
            yield break;

        // Clamp counts
        tilesToFlashCount = Mathf.Min(tilesToFlashCount, activeTiles.Count);
        tilesToDropCount = Mathf.Min(tilesToDropCount, tilesToFlashCount);

        // 1. Warning Phase – flash *all* remaining tiles
        foreach (var tile in activeTiles)
        {
            tile.SetWarning(true);
        }

        float timer = warningDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            if (waveTimer != null) waveTimer.SetTime(timer);
            yield return null;
        }

        // Turn off warning visuals everywhere
        foreach (var tile in activeTiles)
        {
            tile.SetWarning(false);
        }

        // 2. Collapse Phase – pick random subset to actually collapse
        List<RockTile> tilesToFlash = GetRandomTiles(activeTiles, tilesToFlashCount);
        List<RockTile> tilesToDrop = GetRandomTiles(tilesToFlash, tilesToDropCount);

        foreach (var tile in tilesToDrop)
        {
            tile.Collapse();          // RockTile logic turns this into lava
            activeTiles.Remove(tile); // no longer a safe brick
        }
    }

    List<T> GetRandomTiles<T>(List<T> sourceList, int count)
    {
        List<T> randomList = new List<T>(sourceList);
        // Fisher-Yates shuffle
        for (int i = 0; i < randomList.Count; i++)
        {
            T temp = randomList[i];
            int randomIndex = Random.Range(i, randomList.Count);
            randomList[i] = randomList[randomIndex];
            randomList[randomIndex] = temp;
        }
        return randomList.GetRange(0, count);
    }
}
