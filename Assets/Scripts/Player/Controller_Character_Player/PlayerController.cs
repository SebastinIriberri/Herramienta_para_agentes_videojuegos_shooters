using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    public float moveSpeed = 4f;
    public float rotationSpeed = 12f;

    public float mouseSensitivity = 0.12f;
    public Transform cameraRoot;
    public float minPitch = -35f;
    public float maxPitch = 70f;

    public string forwardParam = "ForwardSpeed";
    public float maxForwardSpeed = 8f;

    public float modelYawOffset = 180f;

    const float groundAccel = 5f;
    const float groundDecel = 10f;

    Vector2 moveInput;
    Vector2 lookInput;

    float desiredForwardSpeed;
    float forwardSpeed;

    float yaw;
    float pitch;

    Animator anim;
    CharacterController controller;

    bool IsMoveInput => moveInput.sqrMagnitude > 0.0001f;

    public void OnMove(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context) {
        lookInput = context.ReadValue<Vector2>();
    }

    void Awake() {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        yaw = transform.eulerAngles.y;

        if (cameraRoot != null)
            pitch = cameraRoot.localEulerAngles.x;
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        yaw += lookInput.x * mouseSensitivity;
        pitch -= lookInput.y * mouseSensitivity;
        pitch = ClampAngle(pitch, minPitch, maxPitch);

        if (cameraRoot != null)
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 moveDirWorld = yawRot * inputDir;

        controller.Move(moveDirWorld * moveSpeed * Time.deltaTime);

        Quaternion targetRot = Quaternion.Euler(0f, yaw + modelYawOffset, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        desiredForwardSpeed = inputDir.magnitude * maxForwardSpeed;
        float acceleration = IsMoveInput ? groundAccel : groundDecel;
        forwardSpeed = Mathf.MoveTowards(forwardSpeed, desiredForwardSpeed, acceleration * Time.deltaTime);

        if (anim != null)
            anim.SetFloat(forwardParam, forwardSpeed);
    }

    static float ClampAngle(float angle, float min, float max) {
        if (angle > 180f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
