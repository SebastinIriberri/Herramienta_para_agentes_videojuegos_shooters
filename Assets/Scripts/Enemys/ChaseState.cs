using UnityEngine;

public class ChaseState : IEnemyState {
    private Unit unit;
    private EnemyShooter shooter;
    public void EnterState(EnemyController controller, EnemyManager manager) {
        Debug.Log("Enemy: Entra en estado de persecución");

        if (unit == null) {
            unit = controller.GetComponent<Unit>();
        }
        if (shooter == null) {
            shooter = controller.GetComponent<EnemyShooter>();
        }
        if (unit != null && manager.player != null) {
            unit.StartFollowing(manager.player);
        }
    }

    public void UpdateState(EnemyController controller, EnemyManager manager) {
        if (manager.player == null) { 
            return; 
        }
        if (Vector3.Distance(controller.transform.position, manager.player.position) > manager.detectionRange) {
            controller.TransitionToState(new PatrolState());
            return;
        }

        shooter?.TryShoot();

    }

    public void ExitState(EnemyController controller, EnemyManager manager) {
        if (unit == null) {
            unit = controller.GetComponent<Unit>();
        }

        if (unit != null) {
            unit.StopFollowing();
        }
    }
}