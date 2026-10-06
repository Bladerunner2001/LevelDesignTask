using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Goal : MonoBehaviour
{
    [Tooltip("Optional object that spins to make the goal stand out.")]
    [SerializeField] private Transform spinningVisual;
    [SerializeField] private float spinSpeed = 60f;
    [SerializeField] private AudioClip reachSound;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        if (spinningVisual != null)
            spinningVisual.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null || GameManager.Instance.IsLevelComplete)
            return;

        player.InputEnabled = false;
        if (reachSound != null)
            AudioSource.PlayClipAtPoint(reachSound, transform.position);

        GameManager.Instance.CompleteLevel();
    }
}
