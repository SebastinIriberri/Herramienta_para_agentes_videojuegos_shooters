using UnityEngine;

public class EnemyController : MonoBehaviour {
    private IEnemyState currentState;
    public EnemyManager enemyManager;

    private void Awake() {
        enemyManager = GetComponent<EnemyManager>();
    }
    private void Start() {
        TransitionToState(new PatrolState());
    }

    private void Update() {
        if (currentState != null && enemyManager != null) {
            currentState.UpdateState(this, enemyManager);
        }
    }

    public void TransitionToState(IEnemyState newState) {
        if (currentState != null) {
            currentState.ExitState(this, enemyManager);
        }
        currentState = newState;
        if (currentState != null) {
            currentState.EnterState(this, enemyManager);
        }
    }
}
