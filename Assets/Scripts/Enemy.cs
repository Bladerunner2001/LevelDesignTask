using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Enemy : MonoBehaviour, IDamageable
{
    private enum State { Idle, Chase, Attack, Return }

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float hitKnockback = 7f;
    [SerializeField] private float hitStunTime = 0.35f;
    [SerializeField] private GameObject deathEffect;

    [Header("Detection")]
    [Tooltip("Starts chasing when the player gets this close.")]
    [SerializeField] private float detectionRange = 8f;
    [Tooltip("Gives up and walks home when the player is farther than this. Keep it larger than Detection Range.")]
    [SerializeField] private float loseRange = 12f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float returnSpeed = 2.5f;
    [SerializeField] private float turnSpeed = 360f;
    [SerializeField] private float gravity = -25f;
    [Tooltip("Won't walk off ledges that drop more than this.")]
    [SerializeField] private float maxDropHeight = 1.5f;

    [Header("Idle")]
    [SerializeField] private float idleLookInterval = 3f;
    [SerializeField] private float idleLookAngle = 90f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private int attackDamage = 1;
    [Tooltip("Delay between starting an attack and the hit landing, so the player can react.")]
    [SerializeField] private float attackWindup = 0.45f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Colors")]
    [SerializeField] private Color windupColor = Color.red;
    [SerializeField] private Color hitFlashColor = Color.white;
    [SerializeField] private float hitFlashTime = 0.1f;

    private CharacterController controller;
    private Transform player;
    private PlayerHealth playerHealth;
    private Material bodyMaterial;
    private Color baseColor;

    private State state = State.Idle;
    private int health;
    private Vector3 homePosition;
    private Quaternion homeRotation;
    private float verticalVelocity;
    private Vector3 knockbackVelocity;
    private float stunnedUntil;
    private float nextAttackTime;
    private float attackHitTime;
    private float flashUntil;
    private float idleTargetYaw;
    private float nextIdleLookTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = maxHealth;
        homePosition = transform.position;
        homeRotation = transform.rotation;
        idleTargetYaw = transform.eulerAngles.y;
        nextIdleLookTime = Time.time + idleLookInterval;

        Renderer body = GetComponentInChildren<Renderer>();
        if (body != null)
        {
            bodyMaterial = body.material;
            baseColor = bodyMaterial.color;
        }
    }

    private void Start()
    {
        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            player = playerController.transform;
            playerHealth = playerController.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        bool playerAvailable = player != null && !GameManager.Instance.IsLevelComplete;
        float distance = playerAvailable ? Vector3.Distance(transform.position, player.position) : float.PositiveInfinity;
        bool stunned = Time.time < stunnedUntil;

        Vector3 moveVelocity = Vector3.zero;
        if (!stunned)
        {
            switch (state)
            {
                case State.Idle:
                    if (distance <= detectionRange)
                        state = State.Chase;
                    else
                        LookAround();
                    break;

                case State.Chase:
                    if (distance > loseRange)
                    {
                        state = State.Return;
                    }
                    else if (distance <= attackRange)
                    {
                        FaceTowards(player.position);
                        if (Time.time >= nextAttackTime)
                        {
                            state = State.Attack;
                            attackHitTime = Time.time + attackWindup;
                        }
                    }
                    else
                    {
                        moveVelocity = VelocityTowards(player.position, chaseSpeed);
                    }
                    break;

                case State.Attack:
                    if (distance > loseRange)
                    {
                        state = State.Return;
                        break;
                    }
                    FaceTowards(player.position);
                    if (Time.time >= attackHitTime)
                    {
                        if (distance <= attackRange * 1.25f && playerHealth != null)
                            playerHealth.TakeDamage(attackDamage, transform.position);
                        nextAttackTime = Time.time + attackCooldown;
                        state = State.Chase;
                    }
                    break;

                case State.Return:
                    if (distance <= detectionRange)
                    {
                        state = State.Chase;
                    }
                    else if (HorizontalDistance(transform.position, homePosition) < 0.3f)
                    {
                        state = State.Idle;
                        idleTargetYaw = homeRotation.eulerAngles.y;
                        nextIdleLookTime = Time.time + idleLookInterval;
                    }
                    else
                    {
                        moveVelocity = VelocityTowards(homePosition, returnSpeed);
                    }
                    break;
            }
        }

        Move(moveVelocity);
        UpdateColor();
    }

    public void TakeDamage(int amount, Vector3 sourcePosition)
    {
        if (health <= 0)
            return;

        health -= amount;
        flashUntil = Time.time + hitFlashTime;

        if (health <= 0)
        {
            Die();
            return;
        }

        Vector3 away = Vector3.ProjectOnPlane(transform.position - sourcePosition, Vector3.up).normalized;
        knockbackVelocity = away * hitKnockback;
        stunnedUntil = Time.time + hitStunTime;

        // Getting hit interrupts the attack and makes the enemy fight back.
        nextAttackTime = Mathf.Max(nextAttackTime, Time.time + hitStunTime + 0.2f);
        if (player != null)
            state = State.Chase;
    }

    private void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void LookAround()
    {
        if (Time.time >= nextIdleLookTime)
        {
            idleTargetYaw = homeRotation.eulerAngles.y + Random.Range(-idleLookAngle, idleLookAngle);
            nextIdleLookTime = Time.time + idleLookInterval * Random.Range(0.7f, 1.3f);
        }

        Quaternion target = Quaternion.Euler(0f, idleTargetYaw, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * 0.3f * Time.deltaTime);
    }

    private Vector3 VelocityTowards(Vector3 destination, float speed)
    {
        Vector3 toTarget = Vector3.ProjectOnPlane(destination - transform.position, Vector3.up);
        if (toTarget.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 direction = toTarget.normalized;
        FaceTowards(destination);
        return HasGroundAhead(direction) ? direction * speed : Vector3.zero;
    }

    private void FaceTowards(Vector3 position)
    {
        Vector3 direction = Vector3.ProjectOnPlane(position - transform.position, Vector3.up);
        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    private bool HasGroundAhead(Vector3 direction)
    {
        if (!controller.isGrounded)
            return true;

        Bounds bounds = controller.bounds;
        Vector3 origin = bounds.center + direction * (bounds.extents.x + 0.3f);
        float rayLength = bounds.extents.y + maxDropHeight;
        return Physics.Raycast(origin, Vector3.down, rayLength, ~0, QueryTriggerInteraction.Ignore);
    }

    private void Move(Vector3 moveVelocity)
    {
        float dt = Time.deltaTime;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += gravity * dt;

        knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, 20f * dt);

        Vector3 velocity = moveVelocity + knockbackVelocity + Vector3.up * verticalVelocity;
        controller.Move(velocity * dt);

        if (transform.position.y < -50f)
            Destroy(gameObject);
    }

    private void UpdateColor()
    {
        if (bodyMaterial == null)
            return;

        if (Time.time < flashUntil)
        {
            bodyMaterial.color = hitFlashColor;
        }
        else if (state == State.Attack)
        {
            float progress = 1f - Mathf.Clamp01((attackHitTime - Time.time) / attackWindup);
            bodyMaterial.color = Color.Lerp(baseColor, windupColor, progress);
        }
        else
        {
            bodyMaterial.color = baseColor;
        }
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        return Vector3.ProjectOnPlane(a - b, Vector3.up).magnitude;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
