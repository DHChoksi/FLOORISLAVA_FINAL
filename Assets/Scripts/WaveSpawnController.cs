using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;   // For FindSpawnPositions

public class WaveSpawnController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private FindSpawnPositions findSpawnPositions; // FLOOR spawn points
    [SerializeField] private GameManager gameManager;               // Uses waveInterval
    [SerializeField] private GridGenerator gridGenerator;           // Current grid (for reference / future use)

    [Header("Spawned Prefabs")]
    [SerializeField] private GameObject lavaPrefab;      // Lava tile (has collider + SpawnedTileHazard)
    [SerializeField] private GameObject brickPrefab;     // Safe tile (has collider + SpawnedTileHazard)
    [SerializeField] private GameObject treasurePrefab;  // Treasure shown at end of Wave 2
    [SerializeField] private GameObject bloodFxPrefab;   // Blood splash FX for lava hits

    [Header("Settings")]
    [SerializeField] private bool autoStartOnPlay = true;
    [Tooltip("If true, all FindSpawnPositions markers will be hidden once we cache their positions.")]
    [SerializeField] private bool hideSpawnMarkers = true;

    private readonly List<GameObject> spawnedWaveObjects = new List<GameObject>();
    private readonly List<Transform> spawnPoints = new List<Transform>();

    private void Awake()
    {
        if (gameManager == null)
            gameManager = GameManager.Instance;

        if (gridGenerator == null)
            gridGenerator = FindObjectOfType<GridGenerator>();

        if (findSpawnPositions == null)
            findSpawnPositions = FindObjectOfType<FindSpawnPositions>();
    }

    private void Start()
    {
        if (autoStartOnPlay)
        {
            StartCoroutine(WaveRoutine());
        }
    }

    private IEnumerator WaveRoutine()
    {
        // Wait until FindSpawnPositions has actually spawned FLOOR markers
        yield return new WaitUntil(() =>
            findSpawnPositions != null &&
            findSpawnPositions.SpawnedObjects != null &&
            findSpawnPositions.SpawnedObjects.Count > 0);

        CacheSpawnPoints();

        // ---- Wave 1 ----
        Debug.Log("[WaveSpawnController] Wave 1 starting.");
        yield return StartCoroutine(SpawnWave(1));

        float interval = (gameManager != null) ? gameManager.waveInterval : 5f;
        yield return new WaitForSeconds(interval);

        // ---- Wave 2 ----
        Debug.Log("[WaveSpawnController] Wave 2 starting.");
        yield return StartCoroutine(SpawnWave(2));

        // End of Wave 2 -> spawn treasure
        SpawnTreasure();
    }

    private void CacheSpawnPoints()
    {
        spawnPoints.Clear();

        foreach (var go in findSpawnPositions.SpawnedObjects)
        {
            if (go == null) continue;
            spawnPoints.Add(go.transform);

            if (hideSpawnMarkers)
            {
                go.SetActive(false); // hide the original marker objects
            }
        }

        Debug.Log($"[WaveSpawnController] Cached {spawnPoints.Count} FLOOR spawn points.");
    }

    private IEnumerator SpawnWave(int waveIndex)
    {
        // Clear anything from previous wave
        ClearWaveObjects();

        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;

            // 50/50 chance of Lava vs Brick – tweak as needed
            bool isLava = Random.value > 0.5f;
            GameObject prefabToSpawn = isLava ? lavaPrefab : brickPrefab;
            if (prefabToSpawn == null) continue;

            GameObject instance = Instantiate(prefabToSpawn, point.position, point.rotation);
            spawnedWaveObjects.Add(instance);

            // Configure the hazard script on the prefab
            var hazard = instance.GetComponent<SpawnedTileHazard>();
            if (hazard != null)
            {
                hazard.Configure(isLava, bloodFxPrefab);
            }
        }

        Debug.Log($"[WaveSpawnController] Wave {waveIndex} spawned {spawnedWaveObjects.Count} tiles.");

        // If you want a short delay to “feel” the wave happening, you can add:
        // yield return new WaitForSeconds(0.5f);

        yield break;
    }

    private void ClearWaveObjects()
    {
        for (int i = 0; i < spawnedWaveObjects.Count; i++)
        {
            if (spawnedWaveObjects[i] != null)
            {
                Destroy(spawnedWaveObjects[i]);
            }
        }
        spawnedWaveObjects.Clear();
    }

    private void SpawnTreasure()
    {
        if (treasurePrefab == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("[WaveSpawnController] Cannot spawn treasure – missing prefab or spawn points.");
            return;
        }

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
        Instantiate(treasurePrefab, point.position, point.rotation);
        Debug.Log("[WaveSpawnController] Treasure spawned at end of Wave 2.");
    }
}
