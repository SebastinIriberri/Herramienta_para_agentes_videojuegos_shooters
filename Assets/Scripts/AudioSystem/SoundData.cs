using System;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem {
    [Serializable]
    public class SoundData : MonoBehaviour {
        public AudioClip clip;
        public AudioMixerGroup mixerGroup;
        public bool loop;
        public bool playOnAwake;
        public bool frequentSound;
    }

}
