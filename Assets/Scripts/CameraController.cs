using UnityEngine;
using UnityEngine.InputSystem;

// Keeps the camera where it was placed in the Scene view relative to the target,
// and lets the player orbit around that placement with the mouse / right stick.
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [Tooltip("Point on the target the camera orbits around (e.g. head height).")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float followSmoothTime = 0.05f;

    [Header("Orbit")]
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float gamepadSensitivity = 180f;
    [SerializeField] private bool invertY = false;
    [Tooltip("How many degrees the player can tilt the view up from where the camera was placed.")]
    [SerializeField] private float maxLookUp = 40f;
    [Tooltip("How many degrees the player can tilt the view down from where the camera was placed.")]
    [SerializeField] private float maxLookDown = 50f;

    [Header("Zoom")]
    [SerializeField] private bool allowZoom = true;
    [Tooltip("Smallest zoom, as a multiple of the placed distance.")]
    [SerializeField] private float minZoom = 0.5f;
    [Tooltip("Largest zoom, as a multiple of the placed distance.")]
    [SerializeField] private float maxZoom = 2f;
    [SerializeField] private float zoomSpeed = 0.002f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionRadius = 0.25f;

    [Header("Cursor")]
    [SerializeField] private bool lockCursor = true;

    private InputAction lookMouseAction;
    private InputAction lookGamepadAction;
    private InputAction zoomAction;

    private Vector3 placedOffset;
    private Quaternion placedRotation = Quaternion.identity;
    private float yaw;
    private float pitch;
    private float zoom = 1f;
    private Vector3 pivot;
    private Vector3 pivotVelocity;

    private void Awake()
    {
        lookMouseAction = new InputAction("LookMouse", InputActionType.Value, "<Mouse>/delta");
        lookGamepadAction = new InputAction("LookGamepad", InputActionType.Value, "<Gamepad>/rightStick");
        zoomAction = new InputAction("Zoom", InputActionType.Value, "<Mouse>/scroll/y");

        if (target != null)
        {
            yaw = target.eulerAngles.y;
            pivot = target.position + targetOffset;

            Quaternion inverseYaw = Quaternion.Inverse(Quaternion.Euler(0f, yaw, 0f));
            placedOffset = inverseYaw * (transform.position - pivot);
            placedRotation = inverseYaw * transform.rotation;
        }
    }

    private void OnEnable()
    {
        lookMouseAction.Enable();
        lookGamepadAction.Enable();
        zoomAction.Enable();
        if (lockCursor)
            SetCursorLocked(true);
    }

    private void OnDisable()
    {
        lookMouseAction.Disable();
        lookGamepadAction.Disable();
        zoomAction.Disable();
        SetCursorLocked(false);
    }

    private void OnDestroy()
    {
        lookMouseAction.Dispose();
        lookGamepadAction.Dispose();
        zoomAction.Dispose();
    }

    private void Update()
    {
        if (!lockCursor)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetCursorLocked(false);
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            SetCursorLocked(true);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        float dt = Time.deltaTime;

        Vector2 look = Vector2.zero;
        if (!lockCursor || Cursor.lockState == CursorLockMode.Locked)
            look += lookMouseAction.ReadValue<Vector2>() * mouseSensitivity;
        look += lookGamepadAction.ReadValue<Vector2>() * gamepadSensitivity * dt;

        yaw += look.x;
        pitch += invertY ? look.y : -look.y;
        pitch = Mathf.Clamp(pitch, -maxLookUp, maxLookDown);

        if (allowZoom)
            zoom = Mathf.Clamp(zoom - zoomAction.ReadValue<float>() * zoomSpeed, minZoom, maxZoom);

        pivot = Vector3.SmoothDamp(pivot, target.position + targetOffset, ref pivotVelocity, followSmoothTime);

        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = orbit * (placedOffset * zoom);

        float offsetLength = offset.magnitude;
        if (offsetLength > 0.01f)
        {
            Vector3 direction = offset / offsetLength;
            if (Physics.SphereCast(pivot, collisionRadius, direction, out RaycastHit hit, offsetLength, collisionMask, QueryTriggerInteraction.Ignore)
                && !hit.transform.IsChildOf(target))
            {
                offset = direction * hit.distance;
            }
        }

        transform.SetPositionAndRotation(pivot + offset, orbit * placedRotation);
    }

    public void SnapBehindTarget()
    {
        if (target == null)
            return;

        yaw = target.eulerAngles.y;
        pitch = 0f;
        pivot = target.position + targetOffset;
        pivotVelocity = Vector3.zero;
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
