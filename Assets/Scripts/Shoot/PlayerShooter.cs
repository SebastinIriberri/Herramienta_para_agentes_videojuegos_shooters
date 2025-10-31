using UnityEngine;

public class PlayerShooter : ShooterBase{
    [Header("Input")]
    [Tooltip("Nombre del botón configurado en Input Manager (por defecto: Fire1).")]
    public string fireButton = "Fire1";

    [Header("Opcional")]
    [Tooltip("Pequeña dispersión aleatoria en grados (0 = sin dispersión).")]
    [Range(0f, 10f)] public float spreadDegrees = 0f;

    [Tooltip("Dibuja un rayo corto para verificar la dirección del firePoint.")]
    public bool debugGizmo = true;

    protected override void Update() {
        base.Update();

        // Seguridad básica
        if (firePoint == null)
            return;

        // Verificar si el jugador presionó el botón de disparo
        bool wantsFire = Input.GetButton(fireButton);
        if (!wantsFire)
            return;

        // Comprobar cooldown
        if (!CanShoot())
            return;

        // Dirección base = hacia donde apunta el firePoint
        Vector3 dir = firePoint.forward;

        // Si hay dispersión, aplicarla
        if (spreadDegrees > 0f) {
            dir = ApplySpread(dir, spreadDegrees);
        }

        // Disparar y reiniciar temporizador
        Fire(dir, transform);
        ResetShootTimer();

        // Dibuja línea visual de depuración
        if (debugGizmo) {
            Debug.DrawRay(firePoint.position, dir * 1.5f, Color.cyan, 0.1f);
        }
    }

    /// <summary>
    /// Aplica una leve desviación angular aleatoria al vector base.
    /// </summary>
    Vector3 ApplySpread(Vector3 baseDir, float degrees) {
        Quaternion randomYaw = Quaternion.AngleAxis(Random.Range(-degrees, degrees), Vector3.up);
        Quaternion randomPitch = Quaternion.AngleAxis(Random.Range(-degrees, degrees), Vector3.right);
        return (randomYaw * randomPitch) * baseDir;
    }
}
