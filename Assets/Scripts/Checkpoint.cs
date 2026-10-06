using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Where the player reappears. Defaults to this transform.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Visuals")]
    [SerializeField] private Renderer indicator;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    public Transform SpawnPoint => spawnPoint != null ? spawnPoint : transform;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        SetActive(GameManager.Instance.CurrentCheckpoint == this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
            GameManager.Instance.SetCheckpoint(this);
    }

    public void SetActive(bool active)
    {
        if (indicator != null)
            indicator.material.color = active ? activeColor : inactiveColor;
    }

    private void OnDrawGizmos()
    {
        Transform point = SpawnPoint;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(point.position + Vector3.up * 0.5f, 0.5f);
        Gizmos.DrawRay(point.position + Vector3.up * 0.5f, point.forward);
    }
}
