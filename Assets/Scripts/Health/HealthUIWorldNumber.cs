using UnityEngine;
using TMPro;

public class HealthUIWorldNumber : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Health health;                 
    [SerializeField] Canvas worldCanvas;            
    [SerializeField] TextMeshProUGUI hpText;        

    [Header("Behavior")]
    [SerializeField] bool billboardToCamera = true;
    [SerializeField] bool hideWhenFull = true;
    [SerializeField] bool hideWhenDead = true;

    [Header("Placement")]
    [SerializeField] Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    [Header("Format")]
    [SerializeField] string format = "{0}/{1}";
    [SerializeField] string prefix = ""; 

    Camera cam;

    void Awake()
    {
        if (worldCanvas == null) worldCanvas = GetComponentInChildren<Canvas>(true);
        if (hpText == null) hpText = GetComponentInChildren<TextMeshProUGUI>(true);

        
        if (health == null) health = GetComponentInParent<Health>();

        if (health == null)
        {
            Debug.LogWarning($"{name}: HealthUIWorldNumber no encontró Health en el padre.");
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        cam = Camera.main;

        health.OnHealthChanged += OnHealthChanged;
        OnHealthChanged(health.CurrentHealth, health.maxHealth);
    }

    void OnDisable()
    {
        if (health != null)
            health.OnHealthChanged -= OnHealthChanged;
    }

    void LateUpdate()
    {
        if (worldCanvas == null) return;

      
        Transform root = health.transform;
        worldCanvas.transform.position = root.position + worldOffset;

        if (billboardToCamera)
        {
            if (!cam) cam = Camera.main;
            if (cam)
            {
               
                worldCanvas.transform.rotation = Quaternion.LookRotation(
                    worldCanvas.transform.position - cam.transform.position
                );
            }
        }
    }

    void OnHealthChanged(float current, float max)
    {
        if (hpText == null) return;

        int cur = Mathf.CeilToInt(current);
        int mx = Mathf.CeilToInt(max);

        hpText.text = prefix + string.Format(format, cur, mx);

        if (hideWhenDead && health.IsDead)
        {
            if (worldCanvas) worldCanvas.enabled = false;
            return;
        }

        if (hideWhenFull && mx > 0 && cur >= mx)
        {
            if (worldCanvas) worldCanvas.enabled = false;
        }
        else
        {
            if (worldCanvas) worldCanvas.enabled = true;
        }
    }
}
