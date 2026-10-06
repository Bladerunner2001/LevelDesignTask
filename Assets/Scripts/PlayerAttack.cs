using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [Tooltip("How far in front of the player the hit area is centred.")]
    [SerializeField] private float reach = 1.2f;
    [SerializeField] private float hitRadius = 1f;
    [SerializeField] private float cooldown = 0.4f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Feedback")]
    [Tooltip("Optional object shown briefly when attacking, e.g. a sword or swoosh child object.")]
    [SerializeField] private GameObject attackVisual;
    [SerializeField] private float visualDuration = 0.15f;
    [SerializeField] private AudioClip attackSound;

    private PlayerController controller;
    private InputAction attackAction;
    private float nextAttackTime;
    private float hideVisualTime;
    private readonly HashSet<IDamageable> hitThisSwing = new HashSet<IDamageable>();

    private Vector3 HitCenter => transform.position + transform.forward * reach;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();

        attackAction = new InputAction("Attack", InputActionType.Button);
        attackAction.AddBinding("<Mouse>/leftButton");
        attackAction.AddBinding("<Keyboard>/f");
        attackAction.AddBinding("<Gamepad>/buttonWest");

        if (attackVisual != null)
            attackVisual.SetActive(false);
    }

    private void OnEnable() => attackAction.Enable();
    private void OnDisable() => attackAction.Disable();
    private void OnDestroy() => attackAction.Dispose();

    private void Update()
    {
        if (attackVisual != null && attackVisual.activeSelf && Time.time >= hideVisualTime)
            attackVisual.SetActive(false);

        if (controller.InputEnabled && attackAction.WasPressedThisFrame() && Time.time >= nextAttackTime)
            Attack();
    }

    private void Attack()
    {
        nextAttackTime = Time.time + cooldown;

        if (attackVisual != null)
        {
            attackVisual.SetActive(true);
            hideVisualTime = Time.time + visualDuration;
        }
        if (attackSound != null)
            AudioSource.PlayClipAtPoint(attackSound, transform.position);

        hitThisSwing.Clear();
        Collider[] hits = Physics.OverlapSphere(HitCenter, hitRadius, hitMask, QueryTriggerInteraction.Ignore);
        foreach (Collider hit in hits)
        {
            if (hit.transform.IsChildOf(transform))
                continue;

            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target != null && hitThisSwing.Add(target))
                target.TakeDamage(damage, transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(HitCenter, hitRadius);
    }
}
