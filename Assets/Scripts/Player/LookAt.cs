using UnityEngine;

public class LookAt : MonoBehaviour {
    [Header("Referencia visual")]
    public GameObject crosshair; 
    public float depth = 3f;     

    private Camera mainCam;
    private Vector3 worldPosition;
    private Vector3 screenPosition;

    private void Start() {
        mainCam = Camera.main;

        if (!crosshair) {
            Debug.LogWarning($"{name}: No se asignó un crosshair. Se ocultará el cursor, pero no se mostrará punto de mira.");
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void FixedUpdate() {
        if (!mainCam) return;

      
        screenPosition = Input.mousePosition;
        screenPosition.z = depth; 

       
        worldPosition = mainCam.ScreenToWorldPoint(screenPosition);

        
        transform.position = worldPosition;

        
        if (crosshair)
            crosshair.transform.position = Input.mousePosition;
    }
}
