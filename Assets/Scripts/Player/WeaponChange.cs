using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponChange : MonoBehaviour {
    [Header("Rig / IK")]
    public TwoBoneIKConstraint leftHand;
    public TwoBoneIKConstraint rightHand;
    public RigBuilder rig;

    [Header("Sets")]
    public Transform[] leftTargets;
    public Transform[] rightTargets;
    public GameObject[] weapons;

    [SerializeField] private int weaponIndex = 0;

    void Start() {
        ApplyWeapon(weaponIndex, rebuildRig: true);
    }

    void Update() {
        if (Input.GetMouseButtonDown(1)) {
            NextWeapon();
        }
    }

    public void NextWeapon() {
        if (weapons == null || weapons.Length == 0) return;
        weaponIndex = (weaponIndex + 1) % weapons.Length;
        ApplyWeapon(weaponIndex, rebuildRig: true);
    }

    void ApplyWeapon(int index, bool rebuildRig) {
        // seguridad: longitudes iguales
        if (leftTargets == null || rightTargets == null ||
            leftTargets.Length != weapons.Length || rightTargets.Length != weapons.Length) {
            Debug.LogWarning("WeaponChange: arregla los arrays (mismo largo).");
        }

        for (int i = 0; i < weapons.Length; i++) {
            if (weapons[i]) weapons[i].SetActive(i == index);
        }

        if (leftHand && index < leftTargets.Length && leftTargets[index]) {
            leftHand.data.target = leftTargets[index];
        }
        if (rightHand && index < rightTargets.Length && rightTargets[index]) {
            rightHand.data.target = rightTargets[index];
        }

        if (rebuildRig && rig) rig.Build();
    }
}
