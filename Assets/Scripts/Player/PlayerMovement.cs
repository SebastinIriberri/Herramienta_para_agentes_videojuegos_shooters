using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(Animator))]
public class PlayerMovement : MonoBehaviour {
    [Header("Movimiento")]
    public float moveSpeed = 3.5f;
    public float rotateSpeed = 100f;
    public float jumpForce = 8f;

    [Header("Detección de suelo (SphereCollider)")]
    public SphereCollider groundCollider;
    [Tooltip("Margen extra para detección del suelo.")]
    public float groundSkin = 0.05f;

    [Header("Animator / Blend Tree 2D")]
    public Animator animator; 
    public string paramHorizontal = "BlendH";
    public string paramVertical = "BlendV";
    public float animDamp = 0.1f;
    public float deadzone = 0.001f;

    Rigidbody rb;
    float inputH, inputV;
    bool wantsJump;
    bool isGrounded;
    float lastJumpTime;
    [SerializeField] private float jumpCooldown = 0.2f;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        if (!animator) animator = GetComponent<Animator>();

        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        
        if (!groundCollider) {
            groundCollider = gameObject.AddComponent<SphereCollider>();
            groundCollider.isTrigger = true;
            groundCollider.radius = 0.25f;
            groundCollider.center = new Vector3(0, 0.1f, 0);
            Debug.Log($"{name}: SphereCollider agregado automáticamente ✅");
        }
    }

    void Update() {
       
        inputH = Input.GetAxis("Horizontal");
        inputV = Input.GetAxis("Vertical");
        if (Mathf.Abs(inputH) < deadzone) inputH = 0f;
        if (Mathf.Abs(inputV) < deadzone) inputV = 0f;

        
        float yaw = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
        if (Mathf.Abs(yaw) > Mathf.Epsilon) {
            Quaternion delta = Quaternion.Euler(0f, yaw, 0f);
            rb.MoveRotation(rb.rotation * delta);
        }

       
        if (animator) {
            animator.SetFloat(paramHorizontal, inputH, animDamp, Time.deltaTime);
            animator.SetFloat(paramVertical, inputV, animDamp, Time.deltaTime);
        }

        
        if (Input.GetButtonDown("Jump"))
            wantsJump = true;
    }

    void FixedUpdate() {
        
        Vector3 moveLocal = new Vector3(inputH, 0f, inputV);
        if (moveLocal.sqrMagnitude > 1f) moveLocal.Normalize();
        Vector3 moveWorld = transform.TransformDirection(moveLocal) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveWorld);

        
        isGrounded = CheckGrounded();

        
        if (wantsJump && isGrounded && Time.time >= lastJumpTime + jumpCooldown) {
            Vector3 v = rb.linearVelocity;
            v.y = jumpForce;
            rb.linearVelocity = v;
            lastJumpTime = Time.time;
        }
        wantsJump = false;
    }

    bool CheckGrounded() {
       
        Vector3 origin = transform.position + transform.TransformPoint(groundCollider.center) - transform.position;
        float radius = groundCollider.radius;

        bool grounded = Physics.CheckSphere(origin, radius + groundSkin, ~0, QueryTriggerInteraction.Ignore);
        return grounded;
    }

    void OnDrawGizmosSelected() {
        if (!groundCollider) return;
        Vector3 origin = transform.position + transform.TransformPoint(groundCollider.center) - transform.position;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin, groundCollider.radius + groundSkin);
    }

}
