using UnityEngine;

public class PlayerShooter : ShooterBase {
    [Header("Opcional")]
    [Range(0f, 10f)] public float spreadDegrees = 0f;

    [Header("Ruido")]
    public float noiseRadius = 18f;

    bool isFiring = false;

    public void SetFiring(bool firing) => isFiring = firing;

    public void TickShoot(Vector3 direction, Transform owner) {
        if (!isFiring) return;
        if (!firePoint) return;
        if (!CanShoot()) return;

        Vector3 dir = direction;

        if (spreadDegrees > 0f)
            dir = ApplySpread(dir, spreadDegrees);

        Fire(dir, owner);
        ResetShootTimer();

        NoiseSystem.EmitNoise(firePoint.position, noiseRadius, NoiseType.Gunshot, owner);
    }

    Vector3 ApplySpread(Vector3 baseDir, float degrees) {
        Quaternion randomYaw = Quaternion.AngleAxis(Random.Range(-degrees, degrees), Vector3.up);
        Quaternion randomPitch = Quaternion.AngleAxis(Random.Range(-degrees, degrees), Vector3.right);
        return (randomYaw * randomPitch) * baseDir.normalized;
    }
}
