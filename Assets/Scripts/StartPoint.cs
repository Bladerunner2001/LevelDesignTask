using UnityEngine;

public class StartPoint : MonoBehaviour
{
    [Tooltip("Where the player appears when the level starts. Defaults to this transform. " +
             "This is the player's centre, so keep it about 1 unit above the floor.")]
    [SerializeField] private Transform spawnPoint;

    public Transform SpawnPoint => spawnPoint != null ? spawnPoint : transform;

    private void OnDrawGizmos()
    {
        Transform point = SpawnPoint;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(point.position, 0.5f);
        Gizmos.DrawRay(point.position, point.forward * 1.5f);
    }
}
