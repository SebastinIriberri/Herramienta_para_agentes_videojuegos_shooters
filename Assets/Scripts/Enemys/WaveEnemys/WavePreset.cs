using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ShooterAI/WavePreset", fileName = "WavePreset")]
public class WavePreset : ScriptableObject {
    [Serializable]
    public class SpawnBlock {
        public string enemyId = "grunt";
        public int count = 3;
        public float startDelay = 0f;
        public float duration = 5f;
    }

    public List<SpawnBlock> blocks = new List<SpawnBlock>();
}