using UnityEngine;
using System.Collections.Generic;

public class GridGenerator3D : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 10;
    public int columns = 10;
    public float squareSize = 0.3048f; // 1 ft in meters

    [Header("Materials")]
    public Material lavaMaterial;                       // Material for lava base under the grid
    public Material rockMaterial_Uncracked;
    public Material rockMaterial_Cracked;
    public Material rockMaterial_VergeOfCrumbling;
    public Material backgroundMaterial;                // (unused but kept for compatibility)

    [Header("FX")]
    public GameObject fireParticlePrefab;              // Particle prefab for RockTile collapse

    // Store generated tiles
    private readonly List<RockTile> allTiles = new List<RockTile>();
    private Material[] rockMaterials;

    // -------------------------------------------------
    // Start – clean any previous grid then generate a fresh one
    // -------------------------------------------------
    private void Start()
    {
        // Clean up any leftover grid from a previous play session
        Transform old = transform.Find("Grid");
        if (old != null)
        {
            foreach (Transform child in old)
                Destroy(child.gameObject);

            Destroy(old.gameObject);
        }

        GenerateGrid();   // guarantees a full set of normal bricks
    }

    // -------------------------------------------------
    // PUBLIC method – builds the grid
    // -------------------------------------------------
    public void GenerateGrid()
    {
        // Initialise rock material array
        rockMaterials = new Material[]
        {
            rockMaterial_Uncracked,
            rockMaterial_Cracked,
            rockMaterial_VergeOfCrumbling
        };

        allTiles.Clear();

        float startX = -(columns / 2f) * squareSize;
        float startZ = -(rows / 2f) * squareSize;

        GameObject gridParent = new GameObject("Grid");
        gridParent.transform.SetParent(transform, false);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                // Create a cube for the brick tile
                GameObject tileObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tileObj.transform.localScale = new Vector3(squareSize, 0.01f, squareSize);
                tileObj.transform.parent = gridParent.transform;

                // Position relative to this GridGenerator3D
                Vector3 localPos = new Vector3(
                    startX + j * squareSize,
                    0f,
                    startZ + i * squareSize
                );
                tileObj.transform.position = transform.TransformPoint(localPos);

                // Add RockTile component and start as *uncracked* brick
                RockTile rockTile = tileObj.AddComponent<RockTile>();
                rockTile.SetInitialState(0);               // always normal brick
                rockTile.particleEffectPrefab = fireParticlePrefab;

                // Apply initial material (uncracked) for visuals
                Renderer rend = tileObj.GetComponent<Renderer>();
                if (rend != null && rockMaterials != null && rockMaterials.Length > 0)
                {
                    rend.material = rockMaterials[0];
                }

                // Store for later lookup
                allTiles.Add(rockTile);
            }
        }

        // -------------------------------------------------
        // 🔥 Lava base under the whole grid (slightly below tiles)
        // -------------------------------------------------
        if (lavaMaterial != null)
        {
            GameObject lavaPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            lavaPlane.name = "LavaBase";

            // Scale (plane is 10x10 by default)
            lavaPlane.transform.localScale = new Vector3(
                columns * squareSize / 10f,
                1f,
                rows * squareSize / 10f
            );

            // Position under the grid
            Vector3 lavaLocalPos = new Vector3(0f, -0.2f, 0f);
            lavaPlane.transform.position = transform.TransformPoint(lavaLocalPos);

            // Assign material
            Renderer lavaRend = lavaPlane.GetComponent<Renderer>();
            lavaRend.material = lavaMaterial;

            // Make collider work as trigger
            Collider col = lavaPlane.GetComponent<Collider>();
            col.isTrigger = true;

            // Add LavaTile behavior to lava base
            LavaTile lavaTile = lavaPlane.AddComponent<LavaTile>();
            lavaTile.bloodFxPrefab = fireParticlePrefab; // or assign your blood FX prefab
        }
    }

    // -------------------------------------------------
    // Public accessor used by GameManager
    // -------------------------------------------------
    public List<RockTile> GetAllRockTiles()
    {
        return allTiles;
    }

    // -------------------------------------------------
    // Helper: get material for a given crack state
    // -------------------------------------------------
    public Material GetRockMaterialForState(int state)
    {
        if (rockMaterials == null || rockMaterials.Length == 0)
            return null;

        if (state >= 0 && state < rockMaterials.Length)
            return rockMaterials[state];

        return rockMaterials[0];
    }
}
