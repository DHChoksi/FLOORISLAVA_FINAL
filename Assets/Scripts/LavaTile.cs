// 12/6/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class LavaTile : MonoBehaviour
{
    [Header("Animation Settings")]
    public float bubbleSpeed = 1.0f;
    public float bubbleIntensity = 0.05f;
    public float glowSpeed = 2.0f;
    public float minEmissionIntensity = 1.5f;
    public float maxEmissionIntensity = 3.0f;

    private Material lavaMaterial;
    private Vector3 originalScale;
    private float bubbleOffset;

    void Start()
    {
        // Get the material instance
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            lavaMaterial = renderer.material; // Creates a unique instance
            renderer.enabled = false; // Hide the tile to expose the lava plane below
        }

        originalScale = transform.localScale;
        bubbleOffset = Random.Range(0f, 100f); // Random start offset for variation
    }

    void Update()
    {
        if (lavaMaterial != null)
        {
            // Animate emission intensity for glowing effect
            float glowIntensity = Mathf.Lerp(minEmissionIntensity, maxEmissionIntensity, 
                (Mathf.Sin(Time.time * glowSpeed + bubbleOffset) + 1f) / 2f);
            
            // Apply emission intensity if material supports it
            if (lavaMaterial.HasProperty("_EmissionColor"))
            {
                Color baseColor = lavaMaterial.GetColor("_BaseColor");
                lavaMaterial.SetColor("_EmissionColor", baseColor * glowIntensity);
            }
        }

        // Subtle bubbling animation - slight vertical scale variation
        float bubbleEffect = Mathf.Sin(Time.time * bubbleSpeed + bubbleOffset) * bubbleIntensity;
        transform.localScale = new Vector3(
            originalScale.x,
            originalScale.y * (1f + bubbleEffect),
            originalScale.z
        );
    }

    void OnDestroy()
    {
        // Clean up material instance
        if (lavaMaterial != null)
        {
            Destroy(lavaMaterial);
        }
    }
}
