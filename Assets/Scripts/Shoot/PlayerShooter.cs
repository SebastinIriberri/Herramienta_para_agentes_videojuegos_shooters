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

    [Header("Ruido")]
    public float noiseRadius = 18f;

    protected override void Update() {
        base.Update();

        if (firePoint == null)
            return;

        bool wantsFire = Input.GetButton(fireButton);
        if (!wantsFire)
            return;

        if (!CanShoot())
            return;

        Vector3 dir = firePoint.forward;

        if (spreadDegrees > 0f) {
            dir = ApplySpread(dir, spreadDegrees);
        }

        Fire(dir, transform);
        ResetShootTimer();

        NoiseSystem.EmitNoise(firePoint.position, noiseRadius, NoiseType.Gunshot, transform);

        if (debugGizmo) {
            Debug.DrawRay(firePoint.position, dir * 1.5f, Color.cyan, 0.1f);
        }
    }

    Vector3 ApplySpread(Vector3 baseDir, float degrees) {
        Quaternion randomYaw = Quaternion.AngleAxis(Random.Range(-degrees, degrees), Vector3.up);
        Quaternion randomPitch = Quaternion.AngleAxis(Random.Range(-degrees, degrees), Vector3.right);
        return (randomYaw * randomPitch) * baseDir;
    }
}
