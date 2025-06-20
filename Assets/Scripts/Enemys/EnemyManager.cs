using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public Transform player;
    [Header("Rangos de comportamiento ")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    [Header("Puntos de patrullaje")]
    public Transform[] patrolPoints;
    public float timeToChanguedPatrolPoint; 
    [HideInInspector] public int currentPatrolIndex = 0; 
    public float speed = 3f;
    public float health = 100f;
    [Header("Animator")]
    public EnemyAnimator enemyAnimator;

    private void Start() {
        enemyAnimator = GetComponent<EnemyAnimator>();  
    }

   
}
