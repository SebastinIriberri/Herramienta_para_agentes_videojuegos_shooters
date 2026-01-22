using UnityEngine;

public class EnemyHealth : MonoBehaviour {
    [SerializeField] int maxHealth = 50;
    int currentHealth;

    void Awake() {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount) {
        if (amount <= 0) return;

        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void Die() {
        Destroy(gameObject);
    }
}
