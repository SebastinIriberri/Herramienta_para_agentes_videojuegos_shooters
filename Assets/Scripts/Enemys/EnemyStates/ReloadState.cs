using UnityEngine;

public class ReloadState : IEnemyState
{
    float _fallbackTimer;

    public void Enter(EnemyManager enemy) {
        enemy.unit?.StopFollowing();

        _fallbackTimer = 0.2f;

        if (enemy.shooter && enemy.shooter.useAmmo) {
            enemy.shooter.StartReload();
        }
    }

    public void Update(EnemyManager enemy) {
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

        bool reloadingByShooter = enemy.shooter && enemy.shooter.useAmmo;

        if (reloadingByShooter) {
            if (enemy.shooter.IsReloading)
                return;
        }
        else {
            _fallbackTimer -= Time.deltaTime;
            if (_fallbackTimer > 0f)
                return;
        }

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
    }
}
