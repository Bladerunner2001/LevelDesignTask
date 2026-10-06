using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [Tooltip("Offset from the target's pivot that the camera orbits around (e.g. head height).")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float followSmoothTime = 0.05f;

    [Header("Orbit")]
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float gamepadSensitivity = 180f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Distance")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 12f;
    [SerializeField] private float zoomSpeed = 0.01f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask = ~0;
    [SerializeField] private float collisionRadius = 0.25f;

    [Header("Cursor")]
    [SerializeField] private bool lockCursor = true;

    private InputAction lookMouseAction;
    private InputAction lookGamepadAction;
    private InputAction zoomAction;

    private float yaw;
    private float pitch = 20f;
    private Vector3 pivot;
    private Vector3 pivotVelocity;

    private void Awake()
    {
        lookMouseAction = new InputAction("LookMouse", InputActionType.Value, "<Mouse>/delta");
        lookGamepadAction = new InputAction("LookGamepad", InputActionType.Value, "<Gamepad>/rightStick");
        zoomAction = new InputAction("Zoom", InputActionType.Value, "<Mouse>/scroll/y");

        yaw = transform.eulerAngles.y;
        if (target != null)
            pivot = target.position + targetOffset;
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
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        distance = Mathf.Clamp(distance - zoomAction.ReadValue<float>() * zoomSpeed, minDistance, maxDistance);

        pivot = Vector3.SmoothDamp(pivot, target.position + targetOffset, ref pivotVelocity, followSmoothTime);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 direction = rotation * Vector3.back;

        float actualDistance = distance;
        if (Physics.SphereCast(pivot, collisionRadius, direction, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore)
            && !hit.transform.IsChildOf(target))
        {
            actualDistance = hit.distance;
        }

        transform.SetPositionAndRotation(pivot + direction * actualDistance, rotation);
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
