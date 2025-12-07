// 12/6/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public int rows = 10; // Number of rows in the grid
    public int columns = 10; // Number of columns in the grid
    public float squareSize = 0.3048f; // 1 ft in meters

    public Material lavaMaterial; // Material for lava tiles
    public Material rockMaterial_Uncracked; // Material for uncracked rock tiles
    public Material rockMaterial_Cracked; // Material for cracked rock tiles
    public Material rockMaterial_VergeOfCrumbling; // Material for rock tiles on verge of crumbling
    public Material backgroundMaterial; // Material for background

    public GameObject fireParticlePrefab; // Assign in Inspector

    private GameObject[,] grid; // Store references to grid tiles
    public System.Collections.Generic.List<RockTile> allTiles = new System.Collections.Generic.List<RockTile>();
    private Material[] rockMaterials; // Array to store rock materials by state

    void Start()
    {
        // Initialize rock materials array for easy access by state index
        rockMaterials = new Material[]
        {
            rockMaterial_Uncracked,
            rockMaterial_Cracked,
            rockMaterial_VergeOfCrumbling
        };

        GenerateGrid(); // Call the method to generate the grid when the scene starts

        // Ensure GameManager exists and starts the game
        if (GameManager.Instance == null)
        {
            Debug.Log("GameManager not found in scene. Auto-creating...");
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }
    }

void GenerateGrid()
{
    float startX = -(columns / 2f) * squareSize;
    float startZ = -(rows / 2f) * squareSize;

    GameObject gridParent = new GameObject("Grid");
    gridParent.transform.parent = transform;

    grid = new GameObject[rows, columns];

    for (int i = 0; i < rows; i++)
    {
        for (int j = 0; j < columns; j++)
        {
            GameObject square = GameObject.CreatePrimitive(PrimitiveType.Cube);
            square.transform.localScale = new Vector3(squareSize, 0.01f, squareSize);
            square.transform.parent = gridParent.transform;

            // ⬇️ Important part: position relative to GridGenerator
            Vector3 localPos = new Vector3(
                startX + j * squareSize,
                0f,
                startZ + i * squareSize
            );
            square.transform.position = transform.TransformPoint(localPos);

            RockTile rockTile = square.AddComponent<RockTile>();
            rockTile.SetInitialState(0);
            rockTile.particleEffectPrefab = fireParticlePrefab;

            Renderer squareRenderer = square.GetComponent<Renderer>();
            if (squareRenderer != null)
            {
                squareRenderer.material = rockMaterials[0];
            }

            allTiles.Add(rockTile);
            grid[i, j] = square;
        }
    }

    // Lava plane under the grid, also relative to GridGenerator
    GameObject lavaPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
    lavaPlane.transform.localScale = new Vector3(columns * squareSize / 10, 1, rows * squareSize / 10);
    lavaPlane.transform.position = transform.TransformPoint(new Vector3(0, -0.02f, 0));
    lavaPlane.GetComponent<Renderer>().material = lavaMaterial;
    lavaPlane.name = "LavaPlane";

    // Background plane, relative too
    GameObject background = GameObject.CreatePrimitive(PrimitiveType.Plane);
    background.transform.localScale = new Vector3(columns * squareSize / 5, 1, rows * squareSize / 5);
    background.transform.position = transform.TransformPoint(new Vector3(0, -0.1f, 0));
    background.GetComponent<Renderer>().material = backgroundMaterial;
    background.name = "Background";
}


    public Material GetRockMaterialForState(int state)
    {
        if (state >= 0 && state < rockMaterials.Length)
        {
            return rockMaterials[state];
        }
        return rockMaterials[0]; // Default to uncracked
    }

    public System.Collections.Generic.List<RockTile> GetAllRockTiles()
    {
        return allTiles;
    }
}
