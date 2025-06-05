using UnityEngine;

public class Node {
    public bool walkable;
    public Vector3 wordPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public Node parent; 
    public Node(bool _walkable, Vector3 _worldPos, int _gridX , int _gridY) { 
        walkable = _walkable;
        wordPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
    public int fCost{ 
        get{ return gCost + hCost; }
    }
}
