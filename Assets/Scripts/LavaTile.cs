using UnityEngine;

public class LavaTile : MonoBehaviour
{
    [Header("FX")]
    public GameObject bloodFxPrefab;   // Assign in inspector or at runtime

    private void OnTriggerEnter(Collider other)
    {
        // Only react to player
        if (!other.CompareTag("Player"))
            return;

        // Spawn blood FX at player's position
        if (bloodFxPrefab != null)
        {
            Instantiate(
                bloodFxPrefab,
                other.transform.position,
                Quaternion.identity
            );
        }

        Debug.Log("[LavaTile] Player touched lava → LOSE (debug)");

        // (Optional) You can trigger player death logic here:
        // Destroy(other.gameObject);
        // GameManager.Instance.GameOver();
    }
}
