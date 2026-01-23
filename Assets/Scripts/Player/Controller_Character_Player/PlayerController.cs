using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour {
    [Header("Movement")]
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float rotationSpeed = 12f;

    [Header("Mouse Look")]
    [SerializeField] float mouseSensitivity = 0.12f;
    [SerializeField] float xSensitivity = 0.5f;
    [SerializeField] float ySensitivity = 0.5f;

    [Header("Animator")]
    [SerializeField] string forwardParam = "ForwardSpeed";
    [SerializeField] float maxForwardSpeed = 8f;
    [SerializeField] float accel = 10f;

    [Header("Laser Configuration")]
    [SerializeField] LineRenderer laser;
    [SerializeField] bool laserActive;
    [SerializeField] GameObject crosshair;

    [Header("Bones")]
    [SerializeField] Transform spine;

    [Header("Shooter Ref")]
    [SerializeField] PlayerShooter shooter;

    [Header("Shooting")]
    [SerializeField] float fireRate = 10f;
    [SerializeField] int damage = 10;
    [SerializeField] float maxShootDistance = 200f;
    [SerializeField] LayerMask shootMask = ~0;

    [Header("Shooting Animation")]
    [SerializeField] string fireTrigger = "Fire";
    [SerializeField] int attackLayerIndex = 1;


    Vector2 moveInput;
    Vector2 lookInput;
    Vector2 spineAngles;
    int fireTriggerHash;
    int attackingLayer;
    float yaw;
    float forwardSpeed;

    bool cursorLocked = true;
    bool isFiring;

    float nextFireTime;

    Animator anim;
    CharacterController controller;

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();

    public void OnFire(InputAction.CallbackContext context) {
        if (shooter == null) {
            Debug.LogWarning("[PlayerController] OnFire: shooter es NULL (no está asignado en el inspector).");
            return;
        }

        if (context.performed) {
            Debug.Log("[PlayerController] OnFire PERFORMED -> shooter.SetFiring(true)");
            shooter.SetFiring(true);
        }

        if (context.canceled) {
            Debug.Log("[PlayerController] OnFire CANCELED -> shooter.SetFiring(false)");
            shooter.SetFiring(false);
        }
    }

    public void OnESC(InputAction.CallbackContext context) {
        if (!context.performed) return;
        cursorLocked = false;
        ApplyCursorState();
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (shooter == null) return;

        shooter.RequestReload();
    }

    void Awake() {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        yaw = transform.eulerAngles.y;

        fireTriggerHash = Animator.StringToHash(fireTrigger);
        attackingLayer = anim.GetLayerIndex("Attacking");
    }

    void Start() {
        cursorLocked = true;
        ApplyCursorState();
        nextFireTime = 0f;
    }

    void Update() {
        if (!cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
            cursorLocked = true;
            ApplyCursorState();
        }

        if (!cursorLocked) return;

        yaw += lookInput.x * mouseSensitivity;

        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        Vector3 moveDirWorld = Quaternion.Euler(0f, yaw, 0f) * inputDir;
        controller.Move(moveDirWorld * moveSpeed * Time.deltaTime);

        Quaternion targetRot = Quaternion.Euler(0f, yaw, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        float desiredForward = inputDir.magnitude * maxForwardSpeed;
        forwardSpeed = Mathf.MoveTowards(forwardSpeed, desiredForward, accel * Time.deltaTime);
        anim.SetFloat(forwardParam, forwardSpeed);

        UpdateLaser();

        if (shooter != null) {
            Vector3 dir = laser != null ? laser.transform.forward : transform.forward;
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                Debug.Log("[PlayerController] Update: TickShoot se llamaría (F presionada) - prueba.");
            shooter.TickShoot(dir, transform);
        }
    }

    void LateUpdate() {
        if (!cursorLocked || spine == null) return;

        spineAngles += new Vector2(-lookInput.y * ySensitivity, lookInput.x * xSensitivity);
        spineAngles.x = Mathf.Clamp(spineAngles.x, -30f, 30f);
        spineAngles.y = Mathf.Clamp(spineAngles.y, -45f, 45f);

        spine.localEulerAngles = new Vector3(spineAngles.x, spineAngles.y, 0f);
    }

    void TryShoot() {
        if (Time.time < nextFireTime) return;

        float interval = fireRate <= 0f ? 0.1f : (1f / fireRate);
        nextFireTime = Time.time + interval;

        if (attackingLayer != -1) {
            //anim.ResetTrigger(fireTriggerHash);    
            anim.SetTrigger(fireTriggerHash);
        }

        Vector3 origin = laser != null ? laser.transform.position : transform.position + Vector3.up;
        Vector3 dir = laser != null ? laser.transform.forward : transform.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxShootDistance, shootMask, QueryTriggerInteraction.Ignore)) {
            if (hit.collider != null && hit.collider.CompareTag("Enemy")) {
                if (hit.collider.TryGetComponent<EnemyHealth>(out var hp)) {
                    hp.TakeDamage(damage);
                }
            }
        }
    }

    void UpdateLaser() {
        if (laser == null) return;

        if (!laserActive) {
            laser.gameObject.SetActive(false);
            if (crosshair != null) crosshair.SetActive(false);
            return;
        }

        laser.gameObject.SetActive(true);
        if (crosshair != null) crosshair.SetActive(true);

        Vector3 origin = laser.transform.position;
        Vector3 dir = laser.transform.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxShootDistance, shootMask, QueryTriggerInteraction.Ignore)) {
            laser.positionCount = 2;
            laser.SetPosition(0, Vector3.zero);
            laser.SetPosition(1, laser.transform.InverseTransformPoint(hit.point));

            if (crosshair != null)
                crosshair.transform.position = Camera.main.WorldToScreenPoint(hit.point);
        }
        else {
            laser.positionCount = 2;
            laser.SetPosition(0, Vector3.zero);
            laser.SetPosition(1, new Vector3(0f, 0f, maxShootDistance));

            if (crosshair != null) crosshair.SetActive(false);
        }
    }

    void ApplyCursorState() {
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !cursorLocked;
    }
}
