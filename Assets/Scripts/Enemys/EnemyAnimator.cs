using UnityEngine;

public class EnemyAnimator : MonoBehaviour {
    Animator animator;
    Unit unit;

    int hashSpeed;
    int hashReload;
    int hashMelee;

    void Awake() {
        animator = GetComponent<Animator>();
        unit = GetComponent<Unit>();

        hashSpeed = Animator.StringToHash("ForwardSpeed");
        hashReload = Animator.StringToHash("Reload");
        hashMelee = Animator.StringToHash("Melee");
    }

    void Update() {
        float speed = unit != null ? unit.CurrentSpeed : 0f;
        animator.SetFloat(hashSpeed, speed);
    }

    public void SetBool(string parameter, bool value) {
        animator.SetBool(parameter, value);
    }

    public void SetTrigger(string triggerName) {
        animator.SetTrigger(triggerName);
    }

    public void PlayReload() {
        animator.SetTrigger(hashReload);
    }

    public void PlayMelee() {
        animator.SetTrigger(hashMelee);
    }

    public void OnReloadFinished() {
        var shooter = GetComponent<EnemyShooter>();
        if (shooter != null) shooter.ForceInstantReload();
    }

    public void OnMeleeHit() {
        var m = GetComponent<EnemyManager>();
        if (m != null) m.OnMeleeHitEvent();
    }

    public void OnMeleeFinished() {
        var m = GetComponent<EnemyManager>();
        if (m != null) m.OnMeleeFinishedEvent();
    }

    public void AnimEvent_MeleeHit() {
        SendMessage("OnMeleeHit", SendMessageOptions.DontRequireReceiver);
    }

    public void AnimEvent_MeleeEnd() {
        SendMessage("OnMeleeEnd", SendMessageOptions.DontRequireReceiver);

        var m = GetComponent<EnemyManager>();
        if (m != null) {
            m.BlockShooting(m.postMeleeShootBlockSeconds);
        }
    }

    public void AnimEvent_ReloadEnd() {
        SendMessage("OnReloadFinished", SendMessageOptions.DontRequireReceiver);
    }
}
