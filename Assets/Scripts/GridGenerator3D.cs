// 12/7/2025 AI-Tag 
// 3D grid generator that uses FindSpawnPositions FLOOR data
// and keeps the same core logic as the original GridGenerator.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class GridGenerator3D : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Number of rows in the grid")]
    public int rows = 10; // Number of rows in the grid

    [Tooltip("Number of columns in the grid")]
    public int columns = 10; // Number of columns in the grid

    [Tooltip("Size of each square (meters)")]
    public float squareSize = 0.3048f; // 1 ft in meters

    [Header("Materials")]
    public Material lavaMaterial;                       // Material for lava plane under the grid
    public Material rockMaterial_Uncracked;            // Material for uncracked rock tiles
    public Material rockMaterial_Cracked;              // Material for cracked rock tiles
    public Material rockMaterial_VergeOfCrumbling;     // Material for rock tiles on verge of crumbling

    [Header("FX / Prefabs")]
    public GameObject fireParticlePrefab;              // Particle prefab for RockTile collapse
    public GameObject bloodFxPrefab;                   // Blood splash FX when player hits lava

    [Tooltip("Prefab for lava tile (must have a Collider marked as Trigger).")]
    public GameObject lavaTilePrefab;

    [Tooltip("Prefab for safe brick tile (must have a Collider marked as Trigger).")]
    public GameObject brickTilePrefab;

    [Header("Treasure Settings")]
    [Tooltip("Treasure spawned after Wave 2 / grid generation.")]
    public GameObject treasurePrefab;

    [Header("MR Floor Detection")]
    [Tooltip("FindSpawnPositions that is configured to spawn on FLOOR surfaces.")]
    public FindSpawnPositions floorSpawnPositions;

    private GameObject[,] grid;                        // Store references to grid tiles
    public List<RockTile> allTiles = new List<RockTile>();
    private Material[] rockMaterials;                  // Array to store rock materials by state

    private void Start()
    {
        // Initialize rock materials array for easy access by state index
        rockMaterials = new Material[]
        {
            rockMaterial_Uncracked,
            rockMaterial_Cracked,
            rockMaterial_VergeOfCrumbling
        };

        if (floorSpawnPositions == null)
        {
            floorSpawnPositions = FindObjectOfType<FindSpawnPositions>();
        }

        StartCoroutine(InitializeGridWhenFloorReady());
    }

    private IEnumerator InitializeGridWhenFloorReady()
    {
        // Wait at least one frame so FindSpawnPositions can run Start()
        yield return null;

        // Default center is this object's position
        Vector3 floorCenter = transform.position;

        if (floorSpawnPositions != null)
        {
            var spawned = floorSpawnPositions.SpawnedObjects;
            float timer = 0f;

            // Wait for FLOOR spawn objects to appear (up to 5 seconds)
            while ((spawned == null || spawned.Count == 0) && timer < 5f)
            {
                spawned = floorSpawnPositions.SpawnedObjects;
                timer += Time.deltaTime;
                yield return null;
            }

            if (spawned != null && spawned.Count > 0)
            {
                Vector3 sum = Vector3.zero;
                int count = 0;
                foreach (var go in spawned)
                {
                    if (go == null) continue;
                    sum += go.transform.position;
                    count++;
                }

                if (count > 0)
                {
                    floorCenter = sum / count; // Average FLOOR position (x, y, z)
                }
            }
        }

        // Ensure grid sits exactly on the floor height
        floorCenter.y = floorCenter.y;

        GenerateGrid(floorCenter);
        SpawnTreasureOnFloor(floorCenter);

        // Keep original behavior: ensure GameManager exists
        if (GameManager.Instance == null)
        {
            Debug.Log("GameManager not found in scene. Auto-creating...");
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }
    }

    private void GenerateGrid(Vector3 floorCenter)
    {
        float startX = -(columns / 2f) * squareSize;
        float startZ = -(rows / 2f) * squareSize;

        GameObject gridParent = new GameObject("Grid3D");
        gridParent.transform.parent = transform;

        grid = new GameObject[rows, columns];
        allTiles.Clear();

        // Base origin is aligned so the grid is centered at floorCenter
        Vector3 baseOrigin = new Vector3(floorCenter.x, floorCenter.y, floorCenter.z);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject square = GameObject.CreatePrimitive(PrimitiveType.Cube);
                square.transform.localScale = new Vector3(squareSize, 0.01f, squareSize);
                square.transform.parent = gridParent.transform;

                Vector3 localPos = new Vector3(
                    startX + j * squareSize,
                    0f,
                    startZ + i * squareSize
                );
                square.transform.position = baseOrigin + localPos;

                RockTile rockTile = square.AddComponent<RockTile>();
                rockTile.SetInitialState(0);
                rockTile.particleEffectPrefab = fireParticlePrefab;

                Renderer squareRenderer = square.GetComponent<Renderer>();
                if (squareRenderer != null && rockMaterials.Length > 0)
                {
                    squareRenderer.material = rockMaterials[0];
                }

                allTiles.Add(rockTile);
                grid[i, j] = square;

                // Spawn lava / brick hazard on top of this tile
                SpawnHazardOnTile(square.transform);
            }
        }

        // Lava plane under the grid (still useful for visual background)
        GameObject lavaPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        lavaPlane.transform.localScale = new Vector3(columns * squareSize / 10f, 1f, rows * squareSize / 10f);
        lavaPlane.transform.position = baseOrigin + new Vector3(0f, -0.02f, 0f);
        if (lavaMaterial != null)
        {
            lavaPlane.GetComponent<Renderer>().material = lavaMaterial;
        }
        lavaPlane.name = "LavaPlane";

        // NO background plane anymore (removed as requested)
    }

    private void SpawnHazardOnTile(Transform tile)
    {
        if (lavaTilePrefab == null && brickTilePrefab == null)
        {
            return;
        }

        // Simple 50/50 choice between lava and brick (can be tuned later)
        bool useLava = lavaTilePrefab != null &&
                       (brickTilePrefab == null || Random.value > 0.5f);

        GameObject prefab = useLava ? lavaTilePrefab : brickTilePrefab;
        if (prefab == null) return;

        Vector3 spawnPos = tile.position + Vector3.up * 0.01f;
        GameObject hazard = Instantiate(prefab, spawnPos, Quaternion.identity, tile);

        if (useLava)
        {
            LavaTile3D lava = hazard.GetComponent<LavaTile3D>();
            if (lava == null) lava = hazard.AddComponent<LavaTile3D>();
            lava.bloodFxPrefab = bloodFxPrefab;
        }
        else
        {
            BrickTile3D brick = hazard.GetComponent<BrickTile3D>();
            if (brick == null) hazard.AddComponent<BrickTile3D>();
        }
    }

    private void SpawnTreasureOnFloor(Vector3 floorCenter)
    {
        if (treasurePrefab == null)
        {
            Debug.LogWarning("[GridGenerator3D] No treasure prefab assigned.");
            return;
        }

        if (floorSpawnPositions == null ||
            floorSpawnPositions.SpawnedObjects == null ||
            floorSpawnPositions.SpawnedObjects.Count == 0)
        {
            Debug.LogWarning("[GridGenerator3D] No FLOOR spawn points available for treasure.");
            return;
        }

        // Random FLOOR location
        GameObject randomAnchor =
            floorSpawnPositions.SpawnedObjects[
                Random.Range(0, floorSpawnPositions.SpawnedObjects.Count)
            ];

        Vector3 spawnPos = new Vector3(
            randomAnchor.transform.position.x,
            floorCenter.y + 0.05f,   // small lift so it doesn't clip the floor
            randomAnchor.transform.position.z
        );

        GameObject treasure = Instantiate(treasurePrefab, spawnPos, Quaternion.identity);
        treasure.name = "Treasure3D";

        Debug.Log("[GridGenerator3D] Treasure spawned successfully on FLOOR.");
    }

    // Same helper methods as original GridGenerator
    public Material GetRockMaterialForState(int state)
    {
        if (state >= 0 && state < rockMaterials.Length)
        {
            return rockMaterials[state];
        }
        return rockMaterials[0]; // Default to uncracked
    }

    public List<RockTile> GetAllRockTiles()
    {
        return allTiles;
    }
}

/// <summary>
/// Lava hazard tile – checks for Player tag, spawns blood FX, and logs debug lose.
/// </summary>
public class LavaTile3D : MonoBehaviour
{
    [HideInInspector]
    public GameObject bloodFxPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (bloodFxPrefab != null)
        {
            Instantiate(bloodFxPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log("[GridGenerator3D] Player hit LAVA tile – LOSE (debug).");
    }
}

/// <summary>
/// Safe brick tile – logs debug safe when player steps on it.
/// </summary>
public class BrickTile3D : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("[GridGenerator3D] Player stepped on SAFE BRICK tile (debug).");
    }
}
