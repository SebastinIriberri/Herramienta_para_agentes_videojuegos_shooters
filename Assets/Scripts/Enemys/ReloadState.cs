using UnityEngine;

public class ReloadState : IEnemyState
{
    float _reloadTimer;

    public void Enter(EnemyManager enemy) {
        // Se queda quieto: paramos cualquier seguimiento/navmesh
        enemy.unit?.StopFollowing();

        // Opcional: podrías disparar aquí un trigger de animación de recarga
        // enemy.enemyAnimator?.SetReload(true);  // si luego lo implementas

        float baseTime = enemy.shooter && enemy.shooter.useAmmo
            ? enemy.shooter.reloadSeconds
            : 0.1f;

        _reloadTimer = Mathf.Max(0.1f, baseTime);
    }

    public void Update(EnemyManager enemy) {
        // Mientras recarga no se mueve, solo puede rotar para mirar al jugador
        _reloadTimer -= Time.deltaTime;

        // Mirar hacia el target si existe
        if (enemy.currentTarget) {
            Vector3 dir = enemy.currentTarget.position - enemy.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f) {
                Vector3 forward = Vector3.Slerp(
                    enemy.transform.forward,
                    dir.normalized,
                    Time.deltaTime * enemy.turnSpeed
                );
                enemy.transform.forward = forward;
            }
        }

        if (_reloadTimer > 0f)
            return;

        // Fin de recarga ? rellenamos cargador
        if (enemy.shooter) {
            enemy.shooter.ReloadInstant();
        }

        // Decidimos a qué estado volver
        if (enemy.currentTarget) {
            float dist = Vector3.Distance(enemy.transform.position, enemy.currentTarget.position);

            if (dist <= enemy.attackRange &&
                enemy.HasLineOfSight(enemy.currentTarget, enemy.attackRange + 1f)) {
                enemy.GoToAttack();
            }
            else {
                enemy.GoToChase();
            }
        }
        else {
            enemy.GoToPatrol();
        }
    }

    public void Exit(EnemyManager enemy) {
        // Opcional: apagar flag de animación de recarga si la añades
        // enemy.enemyAnimator?.SetReload(false);
    }
}
