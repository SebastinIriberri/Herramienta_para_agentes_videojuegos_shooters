using UnityEngine;

public interface IEnemyState {
    void Enter(EnemyManager m);
    void Update(EnemyManager m);
    void Exit(EnemyManager m);
}

