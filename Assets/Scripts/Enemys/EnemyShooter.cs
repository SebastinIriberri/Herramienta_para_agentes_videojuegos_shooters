using UnityEngine;
/// <summary>
/// Controla el disparo del enemigo, heredando de ShooterBase.
/// Gestiona el cooldown y dispara hacia el objetivo cuando se le indica.
/// </summary>
public class EnemyShooter : ShooterBase {

    public void TryShoot(Transform target) {
        if (!target || !firePoint) return;   
        if (!CanShoot()) return;             

        
        Vector3 toTarget = (target.position - firePoint.position);
        float dist = toTarget.magnitude;

        
        if (dist > fireRange) return;

       
        Fire(toTarget.normalized, transform);
        ResetShootTimer();
    }
}