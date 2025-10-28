using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida que flota sobre el objeto (Canvas en World Space).
/// Requiere una Image (fill) tipo Filled ? Horizontal.
/// </summary>
public class HealthBarWorld : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Canvas en World Space que contiene la barra.")]
    public Canvas worldCanvas;

    [Tooltip("Imagen de relleno (Fill) para la barra.")]
    public Image fillImage;

    [Header("Ajustes")]
    [Tooltip("Ocultar la barra cuando esté al 100% de vida.")]
    public bool hideWhenFull = true;

    [Tooltip("Que la barra mire siempre a la cámara activa.")]
    public bool billboardToCamera = true;

    [Tooltip("Desfase vertical respecto al pivot del personaje.")]
    public Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    Camera _cam;
    Health _health;

    void Awake() {
        _health = GetComponent<Health>();
        if (!_health) enabled = false;

        if (!worldCanvas) {
            Debug.LogWarning($"{name}: asigna el Canvas World-Space a HealthBarWorld.");
        }
        if (!fillImage) {
            Debug.LogWarning($"{name}: asigna la Image de relleno a HealthBarWorld.");
        }
    }

    void OnEnable() {
        if (_health != null) {
            _health.OnHealthChanged += HandleHealthChanged;
            // Inicializa UI con estado actual
            HandleHealthChanged(_health.CurrentHealth, _health.maxHealth);
        }
        _cam = Camera.main;
    }

    void OnDisable() {
        if (_health != null) {
            _health.OnHealthChanged -= HandleHealthChanged;
        }
    }

    void LateUpdate() {
        if (!worldCanvas) return;

        // Posicionar la barra sobre la cabeza
        worldCanvas.transform.position = transform.position + worldOffset;

        // Billboard hacia la cámara
        if (billboardToCamera) {
            if (!_cam) _cam = Camera.main;
            if (_cam) {
                worldCanvas.transform.rotation = Quaternion.LookRotation(
                    worldCanvas.transform.position - _cam.transform.position
                );
            }
        }
    }

    void HandleHealthChanged(float current, float max) {
        if (!fillImage) return;

        float t = (max > 0f) ? current / max : 0f;
        fillImage.fillAmount = Mathf.Clamp01(t);

        if (hideWhenFull && t >= 0.999f) {
            if (worldCanvas) worldCanvas.enabled = false;
        }
        else {
            if (worldCanvas) worldCanvas.enabled = true;
        }
    }
}
