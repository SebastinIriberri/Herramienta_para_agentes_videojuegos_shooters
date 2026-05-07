using UnityEngine;
using System;
using System.Collections.Generic;

#if !UNITY_WEBGL || UNITY_EDITOR
using System.Threading;
using System.Collections.Concurrent;
#endif

public class PathRequestManager : MonoBehaviour {
    // Resultados que vuelven al hilo principal
    private readonly Queue<PathResult> results = new Queue<PathResult>();

#if !UNITY_WEBGL || UNITY_EDITOR
    // Cola de trabajo en background (solo PC/Editor)
    static ConcurrentQueue<PathRequest> workQueue = new ConcurrentQueue<PathRequest>();
    static AutoResetEvent workSignal = new AutoResetEvent(false);
    static Thread worker;
    static volatile bool running;
#endif

    static PathRequestManager instance;
    Pathfinding pathfinding;

    void Awake() {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();

#if !UNITY_WEBGL || UNITY_EDITOR
        // Worker para PC/Editor
        running = true;
        worker = new Thread(WorkerLoop) {
            IsBackground = true,
            Name = "AStarWorker"
        };
        worker.Start();
#else
        Debug.Log("[PathRequestManager] WebGL mode: A* correrá en hilo principal (sin threads).");
#endif
    }

    void OnDestroy() {
#if !UNITY_WEBGL || UNITY_EDITOR
        running = false;
        workSignal.Set();
        try { worker?.Join(200); } catch { /* ignore */ }
#endif
    }

    void Update() {
        // Ejecutar callbacks SIEMPRE en hilo principal
        lock (results) {
            while (results.Count > 0) {
                var r = results.Dequeue();
                r.callback?.Invoke(r.path, r.success);
            }
        }
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    static void WorkerLoop() {
        while (running) {
            if (!workQueue.TryDequeue(out var req)) {
                workSignal.WaitOne(10);
                continue;
            }

            try {
                // Ejecuta A* fuera del main thread (PC/Editor)
                instance.pathfinding.FindPath(req, instance.FinishedProcessingPath);
            }
            catch {
                // Evitar logs de Unity desde thread secundario
                if (instance != null) {
                    instance.FinishedProcessingPath(
                        new PathResult(Array.Empty<Vector3>(), false, req.callback)
                    );
                }
            }
        }
    }
#endif

    public static void RequestPath(PathRequest request) {
        if (instance == null || instance.pathfinding == null) {
            Debug.LogWarning("[PathRequestManager] No hay instancia/pathfinding activo.");
            request.callback?.Invoke(Array.Empty<Vector3>(), false);
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        // ? WebGL: calcular path de forma síncrona en el hilo principal
        instance.pathfinding.FindPath(request, instance.FinishedProcessingPath);
#else
        // ? PC/Editor: usar worker thread
        workQueue.Enqueue(request);
        workSignal.Set();
#endif
    }

    public void FinishedProcessingPath(PathResult result) {
        lock (results) {
            results.Enqueue(result);
        }
    }
}

// Tu PathRequest / PathResult
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