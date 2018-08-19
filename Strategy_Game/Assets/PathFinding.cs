using System.Collections.Generic;
using UnityEngine;

public static class PathFinding
{
    /// <summary>
    /// Find a path between two points.
    /// </summary>
    /// <param name="gridMap">Grid to search.</param>
    /// <param name="startPos">Starting position.</param>
    /// <param name="targetPos">Ending position.</param>
    /// <returns>List of points that represent the path to walk.</returns>
    public static void FindPath(GridMap gridMap, Vector3Int startPos, Vector3Int targetPos,ref List<Vector3Int> vector3Ints)
    {
        // find path
        List<Node> nodes_path = new List<Node>();
        _ImpFindPath(gridMap, startPos, targetPos, ref nodes_path);
        // convert to a list of points and return
        List<Vector3Int> ret = new List<Vector3Int>();
        if (nodes_path == null)
            vector3Ints = null;
        for (int i = 0; i < nodes_path.Count; i++)
        {
            ret.Add(new Vector3Int(nodes_path[i].gridX, nodes_path[i].gridY, 0));
        }

        vector3Ints = ret;
    }

    /// <summary>
    /// Internal function that implements the path-finding algorithm.
    /// </summary>
    /// <param name="gridMap">Grid to search.</param>
    /// <param name="startPos">Starting position.</param>
    /// <param name="targetPos">Ending position.</param>
    /// <param name="nodes">List of grid nodes that represent the path to walk.</param>
    private static void _ImpFindPath(GridMap gridMap, Vector3Int startPos, Vector3Int targetPos, ref List<Node> nodes)
    {
        Node startNode = gridMap.nodes[startPos.x, startPos.y];
        Node targetNode = gridMap.nodes[targetPos.x, targetPos.y];

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        Node currentNode;
        List<Node> neighbour = new List<Node>();

        while (openSet.Count > 0)
        {
            currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode, ref nodes);
                return;
            }

            gridMap.GetNeighbours(currentNode, ref neighbour);
            for (int i = 0; i < neighbour.Count; i++)
            {
                if (!neighbour[i].isWalkable || closedSet.Contains(neighbour[i]))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour[i]);
                if (newMovementCostToNeighbour < neighbour[i].gCost || !openSet.Contains(neighbour[i]))
                {
                    neighbour[i].gCost = newMovementCostToNeighbour;
                    neighbour[i].hCost = GetDistance(neighbour[i], targetNode);
                    neighbour[i].parent = currentNode;

                    if (!openSet.Contains(neighbour[i]))
                        openSet.Add(neighbour[i]);
                }
            }
        }
    }

    /// <summary>
    /// Retrace path between two points.
    /// </summary>
    /// <param name="startNode">Starting node.</param>
    /// <param name="endNode">Ending (target) node.</param>
    /// <param name="nodes">Retraced path between nodes.</param>
    private static void RetracePath(Node startNode, Node endNode, ref List<Node> nodes)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        nodes = path;
    }

    /// <summary>
    /// Get distance between two nodes.
    /// </summary>
    /// <param name="nodeA">First node.</param>
    /// <param name="nodeB">Second node.</param>
    /// <returns>Distance between nodes.</returns>
    private static int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = System.Math.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = System.Math.Abs(nodeA.gridY - nodeB.gridY);
        return (dstX > dstY) ?
            14 * dstY + 10 * (dstX - dstY) :
            14 * dstX + 10 * (dstY - dstX);
    }
}

