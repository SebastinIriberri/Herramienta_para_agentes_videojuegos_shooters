using UnityEngine;
using System.Collections.Generic;
public class BulletPool : MonoBehaviour {
    [Header("Configuración")]
    public GameObject bulletPrefab;
    public int poolSize = 20;
    public bool expandable = true;

    private readonly Queue<GameObject> _pool = new Queue<GameObject>();

    private void Awake() {
        if (!bulletPrefab) {
            Debug.LogError($"{name}: BulletPool sin prefab asignado.");
            return;
        }

        for (int i = 0; i < poolSize; i++) {
            var b = CreateBullet();
            b.SetActive(false);
            _pool.Enqueue(b);
        }
    }

    GameObject CreateBullet() {
        GameObject go = Instantiate(bulletPrefab, transform);
        var bullet = go.GetComponent<Bullet>();
        if (!bullet) {
            bullet = go.AddComponent<Bullet>();
            Debug.LogWarning($"{name}: El prefab no tenía Bullet, se agregó automáticamente.");
        }
        // No lo activamos aquí; Awake/OnEnable se dispararán al usarse
        return go;
    }

    public GameObject GetBullet() {
        if (_pool.Count > 0) {
            var go = _pool.Dequeue();
            go.SetActive(true);
            return go;
        }

        if (expandable) {
            var go = CreateBullet();
            go.SetActive(true);
            return go;
        }

        // Si no es expandable, puedes:
        // - devolver null
        // - o reciclar el más antiguo (depende de tu diseño)
        Debug.LogWarning($"{name}: Pool agotado y no expandable.");
        return null;
    }

    public void ReturnBullet(GameObject bullet) {
        if (!bullet) return;
        bullet.SetActive(false);
        bullet.transform.SetParent(transform, false);
        _pool.Enqueue(bullet);
    }
}
