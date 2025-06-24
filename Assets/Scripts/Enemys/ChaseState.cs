using UnityEngine;

public class ChaseState : IEnemyState {
    private Unit unit;
    private EnemyShooter shooter;

    public void EnterState(EnemyController controller, EnemyManager manager) {
        Debug.Log("Enemy: Entra en persecución");
        unit ??= controller.GetComponent<Unit>();
        shooter ??= controller.GetComponent<EnemyShooter>();

        if (unit != null && manager.currentTarget != null) {
            unit.StartFollowing(manager.currentTarget);
        }
    }

    public void UpdateState(EnemyController controller, EnemyManager manager) {
        if (manager.currentTarget == null) {
            controller.TransitionToState(new PatrolState());
            return;
        }

        unit?.StartFollowing(manager.currentTarget);
        shooter?.TryShoot();
    }

    public void ExitState(EnemyController controller, EnemyManager manager) {
        unit?.StopFollowing();
    }
}