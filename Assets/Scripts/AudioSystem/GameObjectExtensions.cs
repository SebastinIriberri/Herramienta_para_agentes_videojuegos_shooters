using UnityEngine;

namespace AudioSystem {
    public static class GameObjectExtensions {
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component { 
            T componet = gameObject.GetComponent<T>();
            if(!componet)componet = gameObject.AddComponent<T>();
            return componet;
        }
    }
}
