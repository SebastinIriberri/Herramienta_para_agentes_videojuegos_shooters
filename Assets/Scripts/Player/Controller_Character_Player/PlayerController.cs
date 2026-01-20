using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour {
    Vector2 moveDirection;
    public float moveSpeed = 2; 

    public void OnMove(InputAction.CallbackContext context) { 
        moveDirection = context.ReadValue<Vector2>();
    }

    void Move(Vector2 direction) {
        transform.Translate(direction.x * moveSpeed * Time.deltaTime,0,direction.y * moveSpeed * Time.deltaTime);
    }

    private void Update() {
        Move(moveDirection);
    }
}
