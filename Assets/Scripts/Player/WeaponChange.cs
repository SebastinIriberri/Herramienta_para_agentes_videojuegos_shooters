using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponChange : MonoBehaviour {
    public TwoBoneIKConstraint leftHand;
    public TwoBoneIKConstraint rightHand;
    public RigBuilder rig;
    public Transform[] leftTargets;
    public Transform[] rightTargets;
    public GameObject[] weapons;
    [SerializeField]private int weaponNumber = 0;

    private void Update() {
        if (Input.GetMouseButtonDown(1)) {
            Debug.Log("Cambio");
            weaponNumber++;
            if (weaponNumber > weapons.Length -1 ) { weaponNumber = 0; }
            for (int i = 0; i < weapons.Length; i++) {
                weapons[i].SetActive(false);
            }
            weapons[weaponNumber].SetActive(true);
            leftHand.data.target = leftTargets[weaponNumber];
            rightHand.data.target = rightTargets[weaponNumber];
            rig.Build();
        }
    }
}
