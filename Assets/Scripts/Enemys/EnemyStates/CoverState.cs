using UnityEngine;

public class CoverState : IEnemyState
{
    float _timer;

    public void Enter(EnemyManager m) {
        _timer = m.coverDuration;

        if (m.currentCover == null) {
            m.GoToPatrol();
            return;
        }

        // Usamos el sistema de follow que ya tienes
        Vector3 dest = m.currentCover.Position;
        m.FollowPoint(dest);
    }

    public void Update(EnemyManager m) {
        _timer -= Time.deltaTime;
        if (_timer <= 0f) {
            m.ReleaseCover();

            if (m.currentTarget != null)
                m.GoToChase();
            else
                m.GoToPatrol();
        }
    }

    public void Exit(EnemyManager m) {
        m.unit?.StopFollowing();
    }
}
