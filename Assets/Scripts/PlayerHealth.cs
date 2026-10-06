using System;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 5;
    [Tooltip("Seconds the player can't be hurt after taking damage or respawning.")]
    [SerializeField] private float invulnerabilityTime = 1f;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackUpForce = 5f;
    [SerializeField] private float blinkInterval = 0.1f;

    public event Action<int, int> HealthChanged;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    private PlayerController controller;
    private PlayerRespawn respawn;
    private Renderer[] renderers;
    private float invulnerableUntil;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        respawn = GetComponent<PlayerRespawn>();
        renderers = GetComponentsInChildren<Renderer>();
        CurrentHealth = maxHealth;
    }

    private void Update()
    {
        bool visible = !IsInvulnerable || Mathf.Repeat(Time.time, blinkInterval * 2f) < blinkInterval;
        foreach (Renderer r in renderers)
            r.enabled = visible;
    }

    public void TakeDamage(int amount, Vector3 sourcePosition)
    {
        if (amount <= 0 || IsInvulnerable || CurrentHealth <= 0 || GameManager.Instance.IsLevelComplete)
            return;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth == 0)
        {
            if (respawn != null)
                respawn.Respawn();
            else
                ResetHealth();
            return;
        }

        invulnerableUntil = Time.time + invulnerabilityTime;
        Vector3 away = Vector3.ProjectOnPlane(transform.position - sourcePosition, Vector3.up).normalized;
        controller.ApplyKnockback(away * knockbackForce + Vector3.up * knockbackUpForce);
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        invulnerableUntil = Time.time + invulnerabilityTime;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
