using UnityEngine;

public enum NoiseType {
    Gunshot,
    Footstep,
    Impact
}

public struct NoiseInfo {
    public Vector3 position;
    public float radius;
    public NoiseType type;
    public Transform source;

    public NoiseInfo(Vector3 position, float radius, NoiseType type, Transform source) {
        this.position = position;
        this.radius = radius;
        this.type = type;
        this.source = source;
    }
}

public static class NoiseSystem {
    public static System.Action<NoiseInfo> OnNoiseEmitted;

    public static void EmitNoise(Vector3 position, float radius, NoiseType type, Transform source) {
        if (OnNoiseEmitted != null) {
            var info = new NoiseInfo(position, radius, type, source);
            OnNoiseEmitted.Invoke(info);
        }
    }
}