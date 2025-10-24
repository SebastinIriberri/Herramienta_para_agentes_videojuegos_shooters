using UnityEngine;

public class PlayerShooter : ShooterBase{
    [Header("Input")]
    public string fireButton = "Fire1";
    public bool automatic = true;

    [Header("Apuntado por cámara")]
    public Camera aimCamera;
    public float rayDistance = 200f;

    protected override void Update() {
        base.Update();

        bool wantsFire = automatic ? Input.GetButton(fireButton) : Input.GetButtonDown(fireButton);
        if (!wantsFire || !CanShoot() || !firePoint) return;

        Vector3 dir = GetAimDirection();
        Fire(dir, transform);
        ResetShootTimer();
    }

    Vector3 GetAimDirection() {
        if (aimCamera) {
            Ray r = aimCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(r, out var hit, rayDistance, ~0, QueryTriggerInteraction.Ignore)) {
                
                return (hit.point - firePoint.position).normalized;
            }
            else {
               
                return aimCamera.transform.forward;
            }
        }

       
        return firePoint.forward;
    }
}
