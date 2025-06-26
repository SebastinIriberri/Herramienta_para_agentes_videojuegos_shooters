using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float moveSpeed = 3.5f;
    public float rotateSpeed = 100f;
    public float jumpForce = 1200f;
    private Rigidbody rb;
    private Animator animator;

    void Start() { 
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }
    void FixedUpdate() {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

        Vector3 rotateY = new Vector3(0,Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime,0);
        if (movement != Vector3.zero) { 
            rb.MoveRotation(rb.rotation * Quaternion.Euler(rotateY));
        }
       
        rb.MovePosition(rb.position + transform.forward * Input.GetAxis("Vertical") * 
        moveSpeed * Time.deltaTime + transform.right * Input.GetAxis("Horizontal") * moveSpeed *Time.deltaTime);

        animator.SetFloat("BlendV", Input.GetAxis("Vertical"));
        animator.SetFloat("BlendH", Input.GetAxis("Horizontal"));
    }

    private void Update() {
        if (Input.GetButtonDown("Jump")) {
            rb.AddForce(Vector3.up * jumpForce * Time.deltaTime, ForceMode.VelocityChange);
        }
    }
}
