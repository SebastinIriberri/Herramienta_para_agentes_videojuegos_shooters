using UnityEngine;
using System.Collections;
using System;
public class Node : IHeapItem<Node> {
    public bool walkable;
    public Vector3 wordPosition;
    public int gridX;
    public int gridY;
    public int movementPenalty;

    public int gCost;
    public int hCost;
    public Node parent;
    int heapIndex;
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty) {
        walkable = _walkable;
        wordPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;
        gCost = int.MaxValue;
    }
    public int fCost {
        get { return gCost + hCost; }
    }
    public int HeapIndex {
        get { 
            return heapIndex;
        }
        set { 
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare) {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0) { 
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return - compare;
    }
}
