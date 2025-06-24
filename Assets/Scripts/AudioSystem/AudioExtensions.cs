using UnityEngine;

namespace AudioSystem {
    public static  class AudioExtensions {

        public static float ToLogarithmicVolume(float sliderValue) {
            return Mathf.Log10(Mathf.Max(sliderValue,0.0001f))*20;
        }

        public static float ToLogarithmicFraction(float fraction) {
            return Mathf.Log10(1 + 9 * fraction) / Mathf.Log10(10);
        }
    }
}
