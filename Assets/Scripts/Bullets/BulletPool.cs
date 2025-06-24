using UnityEngine;
using System.Collections.Generic;
public class BulletPool : MonoBehaviour {
    [Header("Configuración")]
    public GameObject bulletPrefab;
    public int poolSize = 20;

    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    private void Awake() {
        for (int i = 0; i < poolSize; i++) {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet() {
        if (bulletPool.Count > 0) {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        else {
            GameObject bullet = Instantiate(bulletPrefab);
            return bullet;
        }
    }

    public void ReturnBullet(GameObject bullet) {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}
