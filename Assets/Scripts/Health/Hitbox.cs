using UnityEngine;
/// <summary>
/// Hitbox que reenvía daño al Health del dueño, aplicando un multiplicador.
/// Coloca este script en colliders hijos (isTrigger recomendado).
/// </summary>
public class Hitbox : MonoBehaviour {
    [Tooltip("Componente Health del dueño (si se deja vacío, busca en padres).")]
    public Health ownerHealth;

    [Tooltip("Multiplicador de daño para esta zona (ej: 2.0 = headshot).")]
    public float damageMultiplier = 1f;

    void Reset() {
        if (!ownerHealth) ownerHealth = GetComponentInParent<Health>();
    }

    public void ApplyHit(DamageInfo info) {
        if (!ownerHealth) ownerHealth = GetComponentInParent<Health>();
        if (!ownerHealth) return;

        info.amount *= Mathf.Max(0f, damageMultiplier);
        ownerHealth.ApplyDamage(info);
    }
}
