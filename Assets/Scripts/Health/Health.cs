using UnityEngine;
using UnityEngine.Events;
using System;

/// <summary>
/// Componente de vida genérico para Player/Enemy/NPC.
/// Maneja daño, curación, i-frames, auto-regeneración y eventos.
/// </summary>
public class Health : MonoBehaviour {
    [Header("Vida")]
    [Tooltip("Vida máxima del personaje.")]
    public float maxHealth = 100f;

    [Tooltip("Vida actual al iniciar el juego (si 0 usa maxHealth).")]
    public float startHealth = 0f;

    [Tooltip("Si está muerto, se ignoran curaciones y daños adicionales.")]
    [SerializeField] private bool isDead = false;

    [Header("Invulnerabilidad")]
    [Min(0f)] public float invulnerabilitySeconds = 0.2f;

    [Header("Auto-regeneración (opcional)")]
    public bool autoRegen = false;
    [Min(0f)] public float regenDelay = 3f;
    [Min(0f)] public float regenRate = 5f;

    [Header("Eventos (UnityEvents)")]
    public UnityEvent onDamaged;
    public UnityEvent onHealed;
    public UnityEvent onDied;

    // === Señal C#: (current, max) ===
    public event Action<float, float> OnHealthChanged;

    public float CurrentHealth { get; private set; }
    public bool IsDead => isDead;

    float _invulnTimer = 0f;
    float _sinceLastDamage = 0f;

    void Awake() {
        CurrentHealth = (startHealth > 0f) ? Mathf.Min(startHealth, maxHealth) : maxHealth;
        isDead = CurrentHealth <= 0f;
        _invulnTimer = 0f;
        _sinceLastDamage = 0f;
        // Notifica estado inicial (útil para dibujar barra llena)
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void Update() {
        if (_invulnTimer > 0f) _invulnTimer -= Time.deltaTime;
        if (!isDead) _sinceLastDamage += Time.deltaTime;

        if (autoRegen && !isDead && _sinceLastDamage >= regenDelay && CurrentHealth < maxHealth) {
            float before = CurrentHealth;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + regenRate * Time.deltaTime);
            if (CurrentHealth > before) {
                onHealed?.Invoke();
                OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            }
        }
    }

    public void ApplyDamage(DamageInfo info) {
        if (isDead) return;
        if (_invulnTimer > 0f) return;

        float before = CurrentHealth;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - Mathf.Max(0f, info.amount));
        _sinceLastDamage = 0f;
        _invulnTimer = invulnerabilitySeconds;

        if (CurrentHealth < before) {
            onDamaged?.Invoke();
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        if (CurrentHealth <= 0f) {
            Die(info);
        }
    }

    public void Heal(float amount) {
        if (isDead || amount <= 0f) return;
        float before = CurrentHealth;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        if (CurrentHealth > before) {
            onHealed?.Invoke();
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }

    public void Kill(DamageInfo cause) {
        if (isDead) return;
        CurrentHealth = 0f;
        Die(cause);
    }

    void Die(DamageInfo cause) {
        isDead = true;
        onDied?.Invoke();
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

   
    public float GetHealth01() => (maxHealth > 0f) ? (CurrentHealth / maxHealth) : 0f;
}
