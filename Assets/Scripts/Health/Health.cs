using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
public class Health : MonoBehaviour {
    [Header("Vida")]
    public float maxHealth = 100f;
    public float startHealth = 0f;

    [SerializeField] private bool isDead = false;

    [Header("Invulnerabilidad")]
    [Min(0f)] public float invulnerabilitySeconds = 0.2f;

    [Header("Auto-regeneración (opcional)")]
    public bool autoRegen = false;
    [Min(0f)] public float regenDelay = 3f;
    [Min(0f)] public float regenRate = 5f;

    [Header("Comportamiento al morir")]
    public bool deactivateOnDeath = true;
    [Min(0f)] public float deathDeactivateDelay = 1.0f;

    [Header("Pooling (opcional para enemigos)")]
    public bool usePooling = false;
    public EnemyPool enemyPool;

    [Header("Eventos (UnityEvents)")]
    public UnityEvent onDamaged;
    public UnityEvent onHealed;
    public UnityEvent onDied;

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
        if (isDead || _invulnTimer > 0f) return;

        float before = CurrentHealth;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - Mathf.Max(0f, info.amount));
        _sinceLastDamage = 0f;
        _invulnTimer = invulnerabilitySeconds;

        if (CurrentHealth < before) {
            onDamaged?.Invoke();
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        if (CurrentHealth <= 0f) Die(info);
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
        if (isDead) return;
        isDead = true;
        onDied?.Invoke();
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (usePooling && enemyPool != null) {
            if (deathDeactivateDelay > 0f)
                StartCoroutine(ReturnToPoolAfterDelay());
            else
                enemyPool.Despawn(gameObject);
        }
        else if (deactivateOnDeath) {
            if (deathDeactivateDelay > 0f)
                Invoke(nameof(DeactivateSelf), deathDeactivateDelay);
            else
                DeactivateSelf();
        }
    }

    IEnumerator ReturnToPoolAfterDelay() {
        yield return new WaitForSeconds(deathDeactivateDelay);
        if (enemyPool != null) {
            enemyPool.Despawn(gameObject);
        }
        else {
            gameObject.SetActive(false);
        }
    }

    void DeactivateSelf() {
        gameObject.SetActive(false);
    }

    public float GetHealth01() => (maxHealth > 0f) ? (CurrentHealth / maxHealth) : 0f;

    public void ResetFullHealth() {
        isDead = false;
        CurrentHealth = maxHealth;
        _invulnTimer = 0f;
        _sinceLastDamage = 0f;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
