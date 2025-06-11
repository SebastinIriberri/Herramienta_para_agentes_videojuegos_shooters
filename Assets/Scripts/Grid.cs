using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Grid : MonoBehaviour {
    public Transform player;
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWordSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    LayerMask walkableMask;
    Dictionary<int,int> walkableRegionsDictionary = new Dictionary<int, int>();

    Node[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    private void Awake() {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWordSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWordSize.y / nodeDiameter);
        foreach (TerrainType region in walkableRegions) {
            walkableMask.value |= region.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value,2), region.terrainPenalty);
        }
        CreateGrid();
    }
    public int MaxSize {
        get { 
            return gridSizeX * gridSizeY;
        }
    }

    private void CreateGrid() {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWordSize.x / 2 - Vector3.forward * gridWordSize.y / 2;
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                int movementPenalty = 0;
                if (walkable) { 
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray,out hit,100,walkableMask)) {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer,out movementPenalty);
                    }
                }
                grid[x, y] = new Node(walkable, worldPoint, x, y,movementPenalty);
            }
        }
    }
    public List<Node> GetNeighbours(Node node) {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0)
                    continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }
    public Node NodeFromWorldPoint(Vector3 worldPosition) {
        float percentX = (worldPosition.x + gridWordSize.x / 2) / gridWordSize.x;
        float percentY = (worldPosition.z + gridWordSize.y / 2) / gridWordSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWordSize.x, 1, gridWordSize.y));
            if (grid != null && displayGridGizmos) {
                foreach (Node n in grid) {
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    Gizmos.DrawCube(n.wordPosition, Vector3.one * (nodeDiameter - .1f));
                }
            }
    }
    [System.Serializable]
    public class TerrainType {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}

//private void OnDrawGizmos() {
//    Gizmos.DrawWireCube(transform.position, new Vector3(gridWordSize.x, 1, gridWordSize.y));
//    if (onlyDisplayPathGizmos) {
//        if (path != null) {
//            foreach (Node n in path) {
//                Gizmos.color = Color.black;
//                Gizmos.DrawCube(n.wordPosition, Vector3.one * (nodeDiameter - .1f));
//            }
//        }
//    }
//    else {
//        if (grid != null) {
//            Node playerNode = NodeFromWorldPoint(player.position);
//            foreach (Node n in grid) {
//                Gizmos.color = (n.walkable) ? Color.white : Color.red;
//                if (path != null) {
//                    if (path.Contains(n)) {
//                        Gizmos.color = Color.black;
//                    }
//                }
//                Gizmos.DrawCube(n.wordPosition, Vector3.one * (nodeDiameter - .1f));
//                if (playerNode == n) {
//                    Gizmos.color = Color.cyan;
//                }

//            }
//        }
//    }
//}
