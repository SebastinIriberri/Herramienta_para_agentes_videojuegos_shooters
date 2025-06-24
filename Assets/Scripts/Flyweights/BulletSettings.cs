using UnityEngine;

[CreateAssetMenu(fileName = "BulletSettings", menuName = "Flyweights/BulletSettings")]
public class BulletSettings : ScriptableObject {
    [Header("Atributos de la bala")]
    public float speed = 20f;
    public float lifeTime = 2f;
    public float damage = 10f;
}
