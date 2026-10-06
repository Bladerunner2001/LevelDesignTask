using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    [SerializeField] private int value = 1;

    [Header("Animation")]
    [SerializeField] private float spinSpeed = 120f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Feedback")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;

    private Vector3 startPosition;
    private float bobOffset;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        startPosition = transform.position;
        bobOffset = Random.value * Mathf.PI * 2f;
        GameManager.Instance.RegisterCoin();
    }

    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        float bob = Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
        transform.position = startPosition + Vector3.up * bob;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        GameManager.Instance.CollectCoin(value);

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        gameObject.SetActive(false);
    }
}
