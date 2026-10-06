using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Movement is relative to this transform. Defaults to Camera.main.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float groundAcceleration = 60f;
    [SerializeField] private float airAcceleration = 25f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -25f;
    [Tooltip("Gravity multiplier while falling, for a snappier arc.")]
    [SerializeField] private float fallGravityMultiplier = 1.8f;
    [Tooltip("Gravity multiplier when jump is released early, for variable jump height.")]
    [SerializeField] private float lowJumpGravityMultiplier = 2.5f;
    [SerializeField] private float maxFallSpeed = 30f;
    [Tooltip("Seconds after leaving a ledge during which a jump is still allowed.")]
    [SerializeField] private float coyoteTime = 0.12f;
    [Tooltip("Seconds a jump press is remembered before landing.")]
    [SerializeField] private float jumpBufferTime = 0.12f;

    private CharacterController controller;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float lastGroundedTime = float.NegativeInfinity;
    private float lastJumpPressedTime = float.NegativeInfinity;

    public bool IsGrounded => controller.isGrounded;
    public Vector3 Velocity => horizontalVelocity + Vector3.up * verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        moveAction.AddBinding("<Gamepad>/leftStick");

        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        sprintAction = new InputAction("Sprint", InputActionType.Button);
        sprintAction.AddBinding("<Keyboard>/leftShift");
        sprintAction.AddBinding("<Gamepad>/leftStickPress");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
        jumpAction.Dispose();
        sprintAction.Dispose();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        bool grounded = controller.isGrounded;

        if (grounded)
            lastGroundedTime = Time.time;
        if (jumpAction.WasPressedThisFrame())
            lastJumpPressedTime = Time.time;

        UpdateHorizontal(grounded, dt);
        UpdateVertical(grounded, dt);

        Vector3 motion = (horizontalVelocity + Vector3.up * verticalVelocity) * dt;
        CollisionFlags flags = controller.Move(motion);

        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            verticalVelocity = 0f;
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        // CharacterController overrides transform changes while enabled.
        controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        controller.enabled = true;

        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;
        lastGroundedTime = float.NegativeInfinity;
        lastJumpPressedTime = float.NegativeInfinity;
    }

    private void UpdateHorizontal(bool grounded, float dt)
    {
        Vector2 input = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;
        if (cameraTransform != null)
        {
            forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        }

        Vector3 moveDir = forward * input.y + right * input.x;
        float speed = sprintAction.IsPressed() ? sprintSpeed : moveSpeed;
        Vector3 targetVelocity = moveDir * speed;

        float accel = grounded ? groundAcceleration : airAcceleration;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, accel * dt);

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * dt);
        }
    }

    private void UpdateVertical(bool grounded, float dt)
    {
        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // keeps the controller pressed onto the ground so isGrounded stays reliable

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;

        if (canJump && jumpBuffered)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastGroundedTime = float.NegativeInfinity;
            lastJumpPressedTime = float.NegativeInfinity;
            return;
        }

        float gravityScale = 1f;
        if (verticalVelocity < 0f)
            gravityScale = fallGravityMultiplier;
        else if (verticalVelocity > 0f && !jumpAction.IsPressed())
            gravityScale = lowJumpGravityMultiplier;

        verticalVelocity += gravity * gravityScale * dt;
        verticalVelocity = Mathf.Max(verticalVelocity, -maxFallSpeed);
    }
}
