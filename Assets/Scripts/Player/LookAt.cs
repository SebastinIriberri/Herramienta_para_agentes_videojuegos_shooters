using UnityEngine;

public class LookAt : MonoBehaviour {
    private Vector3 worldPosition;
    private Vector3 screenPosition;
    public GameObject crosshair;

    private void Start() {
        Cursor.visible = false;
    }
    private void FixedUpdate() {
        screenPosition = Input.mousePosition;
        screenPosition.z = 3f;
        

        worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        transform.position = worldPosition;

        crosshair.transform.position = Input.mousePosition;
    }
}
