using UnityEngine;

public class EnemyAnimator : MonoBehaviour {
    private Animator animator;
    private Unit unit;


    private void Awake() {
        animator = GetComponent<Animator>();
        unit = GetComponent<Unit>();
    }
        
    private void Update() {
        float speed = unit !=null ? unit.CurrentSpeed : 0f;
        animator.SetFloat("Speed", speed);
    }

    public void SetBool(string parameter, bool value) {
        animator.SetBool(parameter, value);
    }

    public void SetTrigger(string triggerName) {
        animator.SetTrigger(triggerName);
    }
}

