using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
public class PathRequestManager : MonoBehaviour {
    // Resultados que vuelven al hilo principal
    Queue<PathResult> results = new Queue<PathResult>();

    // Cola de trabajo en background
    static ConcurrentQueue<PathRequest> workQueue = new ConcurrentQueue<PathRequest>();
    static AutoResetEvent workSignal = new AutoResetEvent(false);
    static Thread worker;
    static volatile bool running;

    static PathRequestManager instance;
    Pathfinding pathfinding;

    void Awake() {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();

        // Lanza 1 worker dedicado (puedes abrir más si lo necesitas)
        running = true;
        worker = new Thread(WorkerLoop) { IsBackground = true, Name = "AStarWorker" };
        worker.Start();
    }

    void OnDestroy() {
        running = false;
        workSignal.Set();
        try { worker?.Join(200); } catch { /* ignore */ }
    }

    void Update() {
        // Desencolar resultados y ejecutar callbacks en el hilo principal
        if (results.Count > 0) {
            int n = results.Count;
            lock (results) {
                for (int i = 0; i < n; i++) {
                    var r = results.Dequeue();
                    r.callback?.Invoke(r.path, r.success);
                }
            }
        }
    }

    static void WorkerLoop() {
        while (running) {
            if (!workQueue.TryDequeue(out var req)) {
                workSignal.WaitOne(10); // espera señal o 10 ms
                continue;
            }

            try {
                // Ejecuta A* fuera del main thread
                instance.pathfinding.FindPath(req, instance.FinishedProcessingPath);
            }
            catch (Exception e) {
                Debug.LogError($"A* worker exception: {e}");
            }
        }
    }

    public static void RequestPath(PathRequest request) {
        // Encola la solicitud y despierta al worker
        workQueue.Enqueue(request);
        workSignal.Set();
    }

    public void FinishedProcessingPath(PathResult result) {
        lock (results) { results.Enqueue(result); }
    }
}

// Tu PathRequest / PathResult originales
public struct PathResult {
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback) {
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}

public struct PathRequest {
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<Vector3[], bool> callback;

    public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> cb) {
        pathStart = start;
        pathEnd = end;
        callback = cb;
    }
}
