using UnityEngine;

public class AttackState : IEnemyState {
    public void EnterState(EnemyController controller, EnemyManager manager) {
        Debug.Log("Enemy: Entra en estado de persecución");
    }

    public void UpdateState(EnemyController controller, EnemyManager manager) {
        controller.transform.position = Vector3.MoveTowards(
            controller.transform.position,
            manager.player.position,
            manager.speed * Time.deltaTime
        );

        float distance = Vector3.Distance(controller.transform.position, manager.player.position);

        if (distance < manager.attackRange) {
            controller.TransitionToState(new AttackState());
        }
        else if (distance > manager.detectionRange + 5f) {
            controller.TransitionToState(new PatrolState());
        }
    }

    public void ExitState(EnemyController controller, EnemyManager manager) {
        Debug.Log("Enemy: Sale del estado de persecución");
    }
}