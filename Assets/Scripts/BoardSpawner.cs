using UnityEngine;

public class BoardSpawner : MonoBehaviour
{
    [Header("Assign your BoardRoot prefab here")]
    public GameObject boardRootPrefab;

    private GameObject spawnedBoard;

    /// <summary>
    /// Spawn the board at the given position and rotation.
    /// Will only spawn once.
    /// </summary>
    public void SpawnBoard(Vector3 position, Quaternion rotation)
    {
        if (spawnedBoard != null) return; // already spawned

        spawnedBoard = Instantiate(boardRootPrefab, position, rotation);
    }
}
