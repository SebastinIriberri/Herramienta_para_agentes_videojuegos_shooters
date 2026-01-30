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
    int hashDeadBool;   // opcional si usas bool "Dead"
    int hashDieTrigger; // opcional si usas trigger "Die"

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

        hashDeadBool = Animator.StringToHash("Dead");
        hashDieTrigger = Animator.StringToHash("Die");

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

        // 1) Velocidad real (mundo)
        Vector3 delta = (transform.position - _lastPos);
        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 worldVel = delta / dt;

        // 2) Convertimos a local (para separar forward/right)
        Vector3 localVel = transform.InverseTransformDirection(worldVel);
        // localVel.z = forward, localVel.x = right

        // 3) Elegimos velocidad máxima para normalizar (m/s)
        float maxSpeed = fallbackMaxSpeed;

        if (normalizeToMaxSpeed)
        {
            // Primero intentamos EnemyManager.moveSpeed (porque ahí tú lo editas)
            if (enemy != null) maxSpeed = enemy.moveSpeed;
            // Si no existe, intentamos Unit.speed
            else if (unit != null) maxSpeed = unit.speed;
        }

        maxSpeed = Mathf.Max(0.01f, maxSpeed); // evitar división por cero

        // 4) Normalizamos a -1..1 (y aplicamos ganancia)
        float targetForward = Mathf.Clamp((localVel.z / maxSpeed) * inputGain, -1f, 1f);
        float targetRight = Mathf.Clamp((localVel.x / maxSpeed) * inputGain, -1f, 1f);

        // 5) Suavizado (para no “temblar”)
        _smoothedForward = Mathf.Lerp(_smoothedForward, targetForward, Time.deltaTime * speedSmooth);
        _smoothedRight = Mathf.Lerp(_smoothedRight, targetRight, Time.deltaTime * speedSmooth);

        animator.SetFloat(hashForward, _smoothedForward);
        animator.SetFloat(hashRight, _smoothedRight);

        _lastPos = transform.position;

        // (Opcional) speed global del animator según velocidad real.
        // OJO: esto acelera TODO (incluye reload/death), úsalo si te gusta.
        float planarSpeed = new Vector2(worldVel.x, worldVel.z).magnitude;
        animator.speed = Mathf.Lerp(0.9f, 1.6f, Mathf.InverseLerp(0f, maxSpeed, planarSpeed));
    }

    public void SetBool(string parameter, bool value) => animator.SetBool(parameter, value);
    public void SetTrigger(string triggerName) => animator.SetTrigger(triggerName);

    public void PlayReload() => animator.SetTrigger(hashReload);
    public void PlayMelee() => animator.SetTrigger(hashMelee);

    public void PlayDeathBool(bool dead) => animator.SetBool(hashDeadBool, dead);
    public void PlayDeathTrigger() => animator.SetTrigger(hashDieTrigger);

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
        if (m != null) m.BlockShooting(m.postMeleeShootBlockSeconds);
    }

    public void AnimEvent_ReloadEnd() => SendMessage("OnReloadFinished", SendMessageOptions.DontRequireReceiver);
}



/*using UnityEngine;

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
}*/
