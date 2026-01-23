using UnityEngine;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Health health;                 // Player Health
    [SerializeField] TextMeshProUGUI healthText;    // TMP text (UI)

    [Header("Format")]
    [SerializeField] string format = "{0}/{1}";
    [SerializeField] string prefix = "HP ";

    [Header("Options")]
    [SerializeField] bool updateEveryFrame = false; // recomendado: false (usa evento)

    void Awake()
    {
        if (healthText == null) healthText = GetComponent<TextMeshProUGUI>();

        // Intento auto: si no lo asignas, busca Health en escena (por tag Player o por objeto con Health)
        if (health == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) health = player.GetComponentInParent<Health>();
        }
    }

    void OnEnable()
    {
        if (health != null)
        {
            health.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(health.CurrentHealth, health.maxHealth); // init
        }
        else
        {
            RenderMissing();
        }
    }

    void OnDisable()
    {
        if (health != null)
            health.OnHealthChanged -= HandleHealthChanged;
    }

    void Update()
    {
        if (!updateEveryFrame) return;

        if (health == null)
        {
            RenderMissing();
            return;
        }

        HandleHealthChanged(health.CurrentHealth, health.maxHealth);
    }

    void HandleHealthChanged(float current, float max)
    {
        if (healthText == null) return;

        int cur = Mathf.CeilToInt(current);
        int mx = Mathf.CeilToInt(max);

        healthText.text = prefix + string.Format(format, cur, mx);
    }

    void RenderMissing()
    {
        if (healthText != null)
            healthText.text = "HP --/--";
    }
}
