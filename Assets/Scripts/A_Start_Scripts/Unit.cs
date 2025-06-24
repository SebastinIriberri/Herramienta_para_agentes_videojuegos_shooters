using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour {
    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;

    public Transform target;
    public float speed = 20;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 5;
    Path path;

    public System.Action OnDestinationReached;
    public float CurrentSpeed { 
        get;  set; 
    }
    public bool HasReachedDestination { get; private set; }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful) {
        if (pathSuccessful) { 
            path = new Path(waypoints,transform.position,turnDst,stoppingDst);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }
    IEnumerator UpdatePath() {
        if (Time.timeSinceLevelLoad < .3f) { 
            yield return new WaitForSeconds(.3f);
        }
        PathRequestManager.RequestPath(new PathRequest( transform.position, target.position, OnPathFound));
        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;

        while (true) {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold) {
                PathRequestManager.RequestPath(new PathRequest (transform.position,target.position,OnPathFound));
                targetPosOld = target.position;
            }
        }
    }
    IEnumerator FollowPath() {
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);
        float speedPercent = 1;

        while (true) {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) {
                if (pathIndex == path.finishLineIndex) {
                    followingPath = false;
                    break;
                }
                else {
                    pathIndex++;
                }
            }
            if (followingPath) {
                HasReachedDestination = false;
                if (pathIndex >= path.slowDownIndex && stoppingDst > 0) {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                    if (speedPercent < 0.1f || Vector3.Distance(transform.position, path.lookPoints[path.finishLineIndex]) <= stoppingDst) {
                        followingPath = false;
                        HasReachedDestination = true;
                    }
                }
                // Rotación solo en plano XZ (evita rotar en Y)
                Vector3 direction = path.lookPoints[pathIndex] - transform.position;
                direction.y = 0f;

                //if (direction != Vector3.zero) {
                //    Quaternion targetRotation = Quaternion.LookRotation(direction);
                //    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                //}
                if (direction.sqrMagnitude > 0.01f) {
                    // Solo rotación sobre Y
                    Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                    Vector3 currentEuler = transform.eulerAngles;
                    float yRotation = Mathf.LerpAngle(currentEuler.y, targetRotation.eulerAngles.y, Time.deltaTime * turnSpeed);
                    transform.rotation = Quaternion.Euler(0, yRotation, 0); // solo rotar en Y
                }
                // Movimiento hacia adelante solo en plano
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
                CurrentSpeed = speed * speedPercent;
              
               
            }
            else {
                HasReachedDestination = true;
                CurrentSpeed = 0f;
            }
            yield return null;  
        }
    }

    public void OnDrawGizmos() {
        if (path != null) {
            path.DrawWithGizmos();
        }
    }

    public void StartFollowing(Transform newTarget) {
        target = newTarget;
        HasReachedDestination = false;
        StopAllCoroutines();
        StartCoroutine(UpdatePath());
    }
    public void StopFollowing() {
        StopAllCoroutines();
        CurrentSpeed = 0f;
        HasReachedDestination = true;
    }
}
