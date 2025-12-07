// 12/6/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class RockTile : MonoBehaviour
{
    public enum CrackState
    {
        Uncracked = 0,
        Cracked = 1,
        VergeOfCrumbling = 2
    }

    [Header("Rock State")]
    public CrackState currentState = CrackState.Uncracked;

    [Header("Flicker Settings")]
    public float[] baseFlickerSpeeds = { 0.5f, 1.5f, 3.5f }; // Speed for each crack state
    public float minBrightness = 1.0f;
    public float maxBrightness = 1.5f;

    [Header("Damage Settings")]
    public float damagePerSecond = 0.25f; // Takes 4 seconds to break each state
    
    private Material rockMaterial;
    private Renderer tileRenderer;
    private float damageAccumulated = 0f;
    private bool playerStandingOn = false;
    private float flickerOffset;
    private Color originalColor;

    void Start()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            rockMaterial = tileRenderer.material; // Create unique instance
            if (rockMaterial.HasProperty("_BaseColor"))
            {
                originalColor = rockMaterial.GetColor("_BaseColor");
            }
        }

        flickerOffset = Random.Range(0f, 100f); // Random start offset for variation
        UpdateMaterialForState();
    }

    public void SetInitialState(int state)
    {
        currentState = (CrackState)Mathf.Clamp(state, 0, 2);
        if (rockMaterial != null)
        {
            UpdateMaterialForState();
        }
    }

    void Update()
    {
        if (isWarning)
        {
            // Flash between materials or colors to indicate warning
            float flicker = Mathf.PingPong(Time.time * 10f, 1f);
            if (tileRenderer != null)
            {
                // Simple color flash for now, red tint
                if (rockMaterial.HasProperty("_BaseColor"))
                {
                    rockMaterial.SetColor("_BaseColor", Color.Lerp(originalColor, Color.red, flicker));
                }
            }
        }
        else if (playerStandingOn)
        {
            // Apply flicker effect based on current crack state
            float currentFlickerSpeed = baseFlickerSpeeds[(int)currentState];
            float brightness = Mathf.Lerp(minBrightness, maxBrightness,
                (Mathf.Sin(Time.time * currentFlickerSpeed + flickerOffset) + 1f) / 2f);

            if (rockMaterial != null && rockMaterial.HasProperty("_BaseColor"))
            {
                rockMaterial.SetColor("_BaseColor", originalColor * brightness);
            }

            // Accumulate damage
            damageAccumulated += damagePerSecond * Time.deltaTime;

            // Check if ready to break to next state
            if (damageAccumulated >= 1f)
            {
                BreakToNextState();
                damageAccumulated = 0f;
            }
        }
        else
        {
            // Reset brightness when not standing on tile
            if (rockMaterial != null && rockMaterial.HasProperty("_BaseColor"))
            {
                rockMaterial.SetColor("_BaseColor", originalColor);
            }
        }
    }

    private bool isWarning = false;

    public void SetWarning(bool active)
    {
        isWarning = active;
        if (!active && rockMaterial != null && rockMaterial.HasProperty("_BaseColor"))
        {
            rockMaterial.SetColor("_BaseColor", originalColor); // Reset color
        }
    }

    public GameObject particleEffectPrefab;

    public void Collapse()
    {
        // specific particle effect
        if (particleEffectPrefab != null)
        {
            GameObject p = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            Destroy(p, 2f); // Cleanup particle after 2 seconds
        }

        // Force transition to lava immediately
        currentState = CrackState.VergeOfCrumbling; // Set to last state before lava
        ConvertToLava();
    }

    void BreakToNextState()
    {
        if (currentState < CrackState.VergeOfCrumbling)
        {
            currentState++;
            UpdateMaterialForState();
            
            // Find GridGenerator to get the appropriate material
            GridGenerator gridGen = FindObjectOfType<GridGenerator>();
            if (gridGen != null && rockMaterial != null)
            {
                // Clean up old material
                Destroy(rockMaterial);
                
                // Get new material for the updated state
                Material newMaterial = gridGen.GetRockMaterialForState((int)currentState);
                tileRenderer.material = newMaterial;
                rockMaterial = tileRenderer.material;
                
                if (rockMaterial.HasProperty("_BaseColor"))
                {
                    originalColor = rockMaterial.GetColor("_BaseColor");
                }
            }

            // If we just entered VergeOfCrumbling, hide the mesh to expose lava plane
            if (currentState == CrackState.VergeOfCrumbling)
            {
                if (tileRenderer != null)
                {
                    tileRenderer.enabled = false;
                }
            }
        }
        else
        {
            // Tile has completely crumbled - convert to lava
            ConvertToLava();
        }
    }

    void UpdateMaterialForState()
    {
        // Initial setup check
        if (currentState == CrackState.VergeOfCrumbling && tileRenderer != null)
        {
            tileRenderer.enabled = false;
        }
    }

    // Modify the ConvertToLava method
    void ConvertToLava()
    {
        // Remove RockTile component
        Destroy(this);

        // Add LavaTile component
        gameObject.AddComponent<LavaTile>();

        // Ensure mesh renderer is disabled (should already be done if it was crumbling)
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerStandingOn = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerStandingOn = false;
        }
    }

    void OnDestroy()
    {
        // Clean up material instance
        if (rockMaterial != null)
        {
            Destroy(rockMaterial);
        }
    }
}
