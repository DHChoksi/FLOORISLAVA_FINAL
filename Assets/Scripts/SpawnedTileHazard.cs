using UnityEngine;

public class SpawnedTileHazard : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private bool isLava = true;

    [Header("FX")]
    [SerializeField] private GameObject bloodFxPrefab;

    /// <summary>
    /// Called by WaveSpawnController right after instantiation
    /// to set up whether this tile is lava or safe, and which FX to use.
    /// </summary>
    public void Configure(bool lava, GameObject bloodFx)
    {
        isLava = lava;
        bloodFxPrefab = bloodFx;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (isLava)
        {
            // Player hit lava – spawn blood FX and debug “lose”
            if (bloodFxPrefab != null)
            {
                Instantiate(bloodFxPrefab, transform.position, Quaternion.identity);
            }

            Debug.Log("[SpawnedTileHazard] Player hit LAVA tile – LOSE (debug).");
        }
        else
        {
            // Safe brick
            Debug.Log("[SpawnedTileHazard] Player stepped on SAFE BRICK tile (debug).");
        }
    }
}
