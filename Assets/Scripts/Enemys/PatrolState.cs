using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatrolState :IEnemyState {
    private Unit unit;
    private bool isWaiting = false;
    private EnemyManager cachedManager;

    public void EnterState(EnemyController controller, EnemyManager manager) {
        Debug.Log("Enemy: Entra en patrullaje");
        if (unit == null) unit = controller.GetComponent<Unit>();
        cachedManager = manager;

        if (unit != null && manager.patrolPoints.Length > 0) {
            Transform patrolTarget = manager.patrolPoints[manager.currentPatrolIndex];
            unit.StartFollowing(patrolTarget);
        }
    }

    public void UpdateState(EnemyController controller, EnemyManager manager) {
        if (unit == null || manager.patrolPoints.Length == 0) return;

        // Si hay un objetivo (jugador) en rango y en vista, transiciona a persecución
        if (manager.currentTarget != null) {
            controller.TransitionToState(new ChaseState());
            return;
        }

        Transform patrolTarget = manager.patrolPoints[manager.currentPatrolIndex];
        float distance = Vector3.Distance(controller.transform.position, patrolTarget.position);

        if (unit.HasReachedDestination && !isWaiting) {
            isWaiting = true;
            controller.StartCoroutine(WaitAndGoToNext(controller));
        }
    }

    private IEnumerator WaitAndGoToNext(EnemyController controller) {
        yield return new WaitForSeconds(cachedManager.timeToChanguedPatrolPoint);
        cachedManager.currentPatrolIndex = (cachedManager.currentPatrolIndex + 1) % cachedManager.patrolPoints.Length;
        Transform nextPoint = cachedManager.patrolPoints[cachedManager.currentPatrolIndex];
        unit.StartFollowing(nextPoint);
        isWaiting = false;
    }

    public void ExitState(EnemyController controller, EnemyManager manager) {
        unit?.StopFollowing();
        isWaiting = false;
    }
}
