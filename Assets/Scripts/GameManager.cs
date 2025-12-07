using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GridGenerator gridGenerator;
    
    [Header("Wave Settings")]
    public float waveInterval = 5f; // Time between waves
    public float warningDuration = 3f; // How long tiles flash before dropping

    private int currentWave = 0;
    private List<RockTile> activeTiles;

    public WaveTimer3D waveTimer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Auto-find references if missing
        if (gridGenerator == null) gridGenerator = FindObjectOfType<GridGenerator>();
        if (waveTimer == null) waveTimer = FindObjectOfType<WaveTimer3D>();

        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        Debug.Log("GameLoop Started. Waiting for grid...");
        // Wait for grid to initialize
        yield return new WaitForSeconds(1f);
        
        activeTiles = gridGenerator.GetAllRockTiles();
        Debug.Log($"GameManager found {activeTiles.Count} tiles.");

        if (activeTiles.Count == 0)
        {
            Debug.LogError("No tiles found! Wave System cannot start.");
            yield break;
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

        // --- Wave 3 ---
        currentWave = 3;
        Debug.Log("Wave 3 Starting");
        // Flash all remaining, leave only 2
        int tilesToDrop = activeTiles.Count - 2;
        if (tilesToDrop > 0)
        {
            yield return StartCoroutine(HandleWave(activeTiles.Count, tilesToDrop));
        }
        yield return StartCoroutine(WaitInterval(waveInterval));

        // --- Wave 4 ---
        currentWave = 4;
        Debug.Log("Wave 4 Starting");
        // Flash the last 2, drop 1
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

    IEnumerator HandleWave(int tilesToFlashCount, int tilesToDropCount)
    {
        // Sanity checks
        tilesToFlashCount = Mathf.Min(tilesToFlashCount, activeTiles.Count);
        tilesToDropCount = Mathf.Min(tilesToDropCount, tilesToFlashCount);

        List<RockTile> tilesToFlash = GetRandomTiles(activeTiles, tilesToFlashCount);
        
        // Pick subset of flashed tiles to drop
        List<RockTile> tilesToDrop = GetRandomTiles(tilesToFlash, tilesToDropCount);

        // 1. Warning Phase
        foreach (var tile in tilesToFlash)
        {
            tile.SetWarning(true);
        }

        // Count down warning duration
        float timer = warningDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            // Optionally flash timer red or show different visual?
            if (waveTimer != null) waveTimer.SetTime(timer);
            yield return null;
        }

        // 2. Collapse Phase
        foreach (var tile in tilesToFlash)
        {
            tile.SetWarning(false); // Turn off warning visuals
            
            if (tilesToDrop.Contains(tile))
            {
                tile.Collapse();
                activeTiles.Remove(tile); // Remove from valid list
            }
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
