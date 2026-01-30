using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    Animator animator;
    Unit unit;
    EnemyManager enemy;   // para leer moveSpeed

    int hashForward;
    int hashRight;
    int hashReload;
    int hashMelee;
    int hashDeathTrigger; 

    Vector3 _lastPos;
    float _smoothedForward;
    float _smoothedRight;

    [Header("Tuning")]
    [Tooltip("Qué tan rápido reacciona el blend tree a cambios de dirección/velocidad.")]
    public float speedSmooth = 10f;

    [Header("Normalization")]
    [Tooltip("Usa moveSpeed del EnemyManager/Unit como velocidad máxima. Recomendada.")]
    public bool normalizeToMaxSpeed = true;

    [Tooltip("Si NO hay EnemyManager/Unit, usa este fallback como velocidad máxima (m/s).")]
    public float fallbackMaxSpeed = 3.5f;

    [Tooltip("Si tu locomotion necesita 'más punch', multiplica el input normalizado. Ej: 1.2")]
    public float inputGain = 1.0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        unit = GetComponent<Unit>();
        enemy = GetComponent<EnemyManager>();

        hashForward = Animator.StringToHash("ForwardSpeed");
        hashRight = Animator.StringToHash("RightSpeed");

        hashReload = Animator.StringToHash("Reload");
        hashMelee = Animator.StringToHash("Melee");

        hashDeathTrigger = Animator.StringToHash("Death");
        _lastPos = transform.position;
    }

    void Update()
    {
        var h = GetComponent<Health>();
        if (h != null && h.IsDead)
        {
            animator.speed = 1f;
            return;
        }

        Vector3 delta = (transform.position - _lastPos);
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 worldVel = delta / dt;

        Vector3 localVel = transform.InverseTransformDirection(worldVel);
        float maxSpeed = fallbackMaxSpeed;

        if (normalizeToMaxSpeed)
        {
            if (enemy != null) maxSpeed = enemy.moveSpeed;
        
            else if (unit != null) maxSpeed = unit.speed;
        }

        maxSpeed = Mathf.Max(0.01f, maxSpeed); // evitar división por cero

       
        float targetForward = Mathf.Clamp((localVel.z / maxSpeed) * inputGain, -1f, 1f);
        float targetRight = Mathf.Clamp((localVel.x / maxSpeed) * inputGain, -1f, 1f);

        _smoothedForward = Mathf.Lerp(_smoothedForward, targetForward, Time.deltaTime * speedSmooth);
        _smoothedRight = Mathf.Lerp(_smoothedRight, targetRight, Time.deltaTime * speedSmooth);

        animator.SetFloat(hashForward, _smoothedForward);
        animator.SetFloat(hashRight, _smoothedRight);

        _lastPos = transform.position;

        float planarSpeed = new Vector2(worldVel.x, worldVel.z).magnitude;
        animator.speed = Mathf.Lerp(0.9f, 1.6f, Mathf.InverseLerp(0f, maxSpeed, planarSpeed));
    }

    public void SetBool(string parameter, bool value) => animator.SetBool(parameter, value);
    public void SetTrigger(string triggerName) => animator.SetTrigger(triggerName);

    public void PlayReload() => animator.SetTrigger(hashReload);
    public void PlayMelee() => animator.SetTrigger(hashMelee);
    public void PlayDeath() => animator.SetTrigger(hashDeathTrigger);

    public void OnReloadFinished()
    {
        var shooter = GetComponent<EnemyShooter>();
        if (shooter != null) shooter.ForceInstantReload();
    }

    public void OnMeleeHit()
    {
        var m = GetComponent<EnemyManager>();
        if (m != null) m.OnMeleeHitEvent();
    }

    public void OnMeleeFinished()
    {
        var m = GetComponent<EnemyManager>();
        if (m != null) m.OnMeleeFinishedEvent();
    }

    public void AnimEvent_MeleeHit() => SendMessage("OnMeleeHit", SendMessageOptions.DontRequireReceiver);

    public void AnimEvent_MeleeEnd()
    {
        SendMessage("OnMeleeEnd", SendMessageOptions.DontRequireReceiver);

        var m = GetComponent<EnemyManager>();
        if (m != null) m.OnMeleeFinishedEvent();
    }

    public void OnDeath_SetupAnimator()
    {
        // Apaga capa Attacking
        int attackingLayer = animator.GetLayerIndex("Attacking");
        if (attackingLayer != -1)
            animator.SetLayerWeight(attackingLayer, 0f);

        // Limpia triggers típicos para que no se mezcle nada
        animator.ResetTrigger("Reload");
        animator.ResetTrigger("Melee");
        animator.ResetTrigger("Fire"); // si existe

        // (opcional) congela locomotion params
        animator.SetFloat(hashForward, 0f);
        animator.SetFloat(hashRight, 0f);

        // (opcional) evita que tu código vuelva a tocar speed
        animator.speed = 1f;
    }

    public void AnimEvent_ReloadEnd() => SendMessage("OnReloadFinished", SendMessageOptions.DontRequireReceiver);
}


