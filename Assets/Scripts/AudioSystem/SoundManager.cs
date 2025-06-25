using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace AudioSystem {
    public partial class SoundManager : PersistentSingleton<SoundManager> {
        IObjectPool<SoundEmitter> soundEmitterPool;
        readonly List<SoundEmitter> activeSoundEmitters = new();
        public readonly Queue<SoundEmitter>FrequentSoundEmitters = new();

        [SerializeField] SoundEmitter soundEmitterPrebab;
        [SerializeField] bool collectionCheck = true;
        [SerializeField] int defaultCapacity = 10;
        [SerializeField] int maxPoolSize = 100;
        [SerializeField] int maxSoundInstances = 30;

        private void Start() {
            InitializedPool();
        }
        public SoundBuilder CreateSound() => new SoundBuilder(this);
        public bool CanPlaySound(SoundData data) {
            if (!data.frequentSound) return true ;
            if (FrequentSoundEmitters.Count >= maxSoundInstances && FrequentSoundEmitters.TryDequeue(out var soundEmitter)) {
                try {
                    soundEmitter.Stop();
                    return true ;
                }
                catch {
                    Debug.Log("SoundEmitter is already released");
                }
                return false ;
            }
            return true;
        }
        public SoundEmitter Get() {
            return soundEmitterPool.Get();
        }
        public void ReturnToPool(SoundEmitter soundEmitter) {
            soundEmitterPool.Release(soundEmitter);
        }
        void OnDestroyPoolObject(SoundEmitter soundEmitter) {
            Destroy(soundEmitter.gameObject);
        }
        void OnReturnedPool(SoundEmitter soundEmitter) {
            soundEmitter.gameObject.SetActive(false);
            activeSoundEmitters.Add(soundEmitter);
        }
        void OnTakeFromPool(SoundEmitter soundEmitter) {
            soundEmitter.gameObject.SetActive(true);
            activeSoundEmitters.Add(soundEmitter);
        }
        SoundEmitter CreateSoundEmitter() {
            var soundEmitter = Instantiate(soundEmitterPrebab);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }
        private void InitializedPool() {
            soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedPool,
                OnDestroyPoolObject,
                collectionCheck,
                defaultCapacity,
                maxPoolSize
                );
        }

    }


}
