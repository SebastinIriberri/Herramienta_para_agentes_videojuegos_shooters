using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatrolState :IEnemyState {
    private Unit unit;
    private bool isWaiting = false;
    private EnemyManager cachedManager;

    public void EnterState(EnemyController controller, EnemyManager manager) {
        Debug.Log("Enemy: Entra en estado de patrullaje");

        // Obtener referencia al componente de movimiento (Unit)
        if (unit == null) {
            unit = controller.GetComponent<Unit>();
        }
        cachedManager = manager;
        // Validar que haya puntos y se haya asignado Unit
        if (unit != null && manager.patrolPoints.Length > 0) {
            // Iniciar movimiento hacia el primer punto de patrulla
            Transform patrolTarget = manager.patrolPoints[manager.currentPatrolIndex];
            unit.StartFollowing(patrolTarget);
        }
    }

    public void UpdateState(EnemyController controller, EnemyManager manager) {
        if (manager.patrolPoints.Length == 0 || unit == null) { 
            return; 
        }

        // Si el jugador está dentro del rango de detección, cambia al estado de persecución
        if (manager.player != null) { 
            float distanceToPlayer = Vector3.Distance(controller.transform.position, manager.player.position);
            if (distanceToPlayer < manager.detectionRange) {
                controller.TransitionToState(new ChaseState());
                return;
            }
        }

        // Verificar si ya llegó al punto de patrullaje actual
        Transform patrolTarget = manager.patrolPoints[manager.currentPatrolIndex];
        float distanceToTarget = Vector3.Distance(controller.transform.position, patrolTarget.position);

        if (unit.HasReachedDestination && !isWaiting ) {
            // Avanzar al siguiente punto de patrulla
            isWaiting = true;
            controller.StartCoroutine(WaitAndGoToNext(controller));
        }
    }
    private IEnumerator WaitAndGoToNext(EnemyController controller) {
        yield return new WaitForSeconds(cachedManager.timeToChanguedPatrolPoint);
       
        // Cambia al siguiente punto
        cachedManager.currentPatrolIndex = (cachedManager.currentPatrolIndex + 1) % cachedManager.patrolPoints.Length;
        Transform nextPoint = cachedManager.patrolPoints[cachedManager.currentPatrolIndex];
        unit.StartFollowing(nextPoint);
        isWaiting = false;
        Debug.Log("Cambio de punto ");

    }
    public void ExitState(EnemyController controller, EnemyManager manager) {
        // Detener el movimiento cuando salimos del estado de patrulla
        if (unit != null) {
            unit.StopFollowing();
        }
        isWaiting = false;
    }
}
