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

    public Transform spine; 

    Vector2 moveInput;
    Vector2 lookInput;
    Vector2 lastLookDriection;

    float yaw;
    float forwardSpeed;

    Animator anim;
    CharacterController controller;

    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();

    void Awake() {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        yaw = transform.eulerAngles.y;
    }

    void Start() {
        SetCursorLocked(true);
    }

    private void LateUpdate() {
        lastLookDriection += new Vector2(-lookInput.y * ySensitivity, lookInput.x * xSensitivity);
        lastLookDriection.x = Mathf.Clamp(lastLookDriection.x, -30f, 30f);
        lastLookDriection.y = Mathf.Clamp(lastLookDriection.y, -45f, 45f);

        spine.localEulerAngles = lastLookDriection;
    }
    void Update() {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) SetCursorLocked(false);
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) SetCursorLocked(true);

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
    }

    static void SetCursorLocked(bool locked) {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
