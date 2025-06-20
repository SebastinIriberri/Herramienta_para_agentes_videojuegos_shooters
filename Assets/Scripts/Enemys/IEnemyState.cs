using UnityEngine;

public interface IEnemyState{
    void EnterState(EnemyController enemyController,EnemyManager enemyManager);
    void UpdateState(EnemyController enemyController ,EnemyManager enemyManager);
    void ExitState(EnemyController enemyController, EnemyManager enemyManager);
}
