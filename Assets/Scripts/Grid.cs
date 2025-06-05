using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {
    public Transform player;
    public LayerMask unwalkableMask; 
    public Vector2 gridWordSize;
    public float nodeRadius;

    Node[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    private void Start() {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWordSize.x/nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWordSize.y/nodeDiameter);
        CreateGrid();
    }

    private void CreateGrid() {
        grid = new Node[gridSizeX,gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWordSize.x/2 - Vector3.forward * gridWordSize.y/2;
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius,unwalkableMask));
                grid[x, y] = new Node(walkable,worldPoint,x , y ); 
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

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 &&checkY < gridSizeY) {
                    neighbours.Add(grid[checkX,checkY]);
                }
            }
        }
        return neighbours;
    }
    public Node NodeFromWorldPoint(Vector3 worldPosition) {
        float percentX = (worldPosition.x + gridWordSize.x/2) / gridWordSize.x;
        float percentY = (worldPosition.z + gridWordSize.y/2) / gridWordSize.y;
        percentX = Mathf.Clamp01(percentX); 
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    public List<Node> path; 
    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform .position, new Vector3(gridWordSize.x, 1 , gridWordSize.y));
        if (grid != null) {
            Node playerNode = NodeFromWorldPoint(player.position);
            foreach (Node n in grid ) {
                Gizmos.color = (n.walkable)?Color.white:Color.red;
                if (path != null) {
                    if (path.Contains(n)) { 
                        Gizmos.color = Color.black;
                    }
                }
                if (playerNode == n) { 
                    Gizmos.color =Color.cyan;
                }
                Gizmos.DrawCube(n.wordPosition, Vector3.one * (nodeDiameter -.1f)); 
            }
        }
    }
}
