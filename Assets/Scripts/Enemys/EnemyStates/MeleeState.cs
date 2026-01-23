using UnityEngine;

public class MeleeState : IEnemyState {
    float _failsafe;

    public void Enter(EnemyManager m) {
        m.SetMeleeLock(true);
        m.unit?.StopFollowing();
        m.BeginMeleeSwing();
        _failsafe = Mathf.Max(0.15f, m.meleeFailSafeSeconds);
        if (m.enemyAnimator != null) m.enemyAnimator.PlayMelee();
    }

    public void Update(EnemyManager m) {
        _failsafe -= Time.deltaTime;

        if (m.currentTarget) {
            Vector3 dir = m.currentTarget.position - m.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f) {
                Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
                float y = Mathf.LerpAngle(m.transform.eulerAngles.y, look.eulerAngles.y, Time.deltaTime * m.turnSpeed);
                m.transform.rotation = Quaternion.Euler(0f, y, 0f);
            }
        }

        if (_failsafe <= 0f) {
            m.OnMeleeFinishedEvent();
        }
    }

    public void Exit(EnemyManager m) {
        m.SetMeleeLock(false);
        m.unit?.StopFollowing();
    }
}
