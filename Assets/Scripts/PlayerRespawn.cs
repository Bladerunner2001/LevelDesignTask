using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerRespawn : MonoBehaviour
{
    [Tooltip("The player respawns automatically when falling below this height.")]
    [SerializeField] private float killHeight = -20f;

    private PlayerController player;
    private InputAction respawnAction;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        startPosition = transform.position;
        startRotation = transform.rotation;

        respawnAction = new InputAction("Respawn", InputActionType.Button);
        respawnAction.AddBinding("<Keyboard>/r");
        respawnAction.AddBinding("<Gamepad>/select");
    }

    private void OnEnable() => respawnAction.Enable();
    private void OnDisable() => respawnAction.Disable();
    private void OnDestroy() => respawnAction.Dispose();

    private void Update()
    {
        if (transform.position.y < killHeight || respawnAction.WasPressedThisFrame())
            Respawn();
    }

    public void Respawn()
    {
        Checkpoint checkpoint = GameManager.Instance.CurrentCheckpoint;
        if (checkpoint != null)
        {
            Transform point = checkpoint.SpawnPoint;
            Quaternion yawOnly = Quaternion.Euler(0f, point.eulerAngles.y, 0f);
            player.Teleport(point.position, yawOnly);
        }
        else
        {
            player.Teleport(startPosition, startRotation);
        }
    }
}
