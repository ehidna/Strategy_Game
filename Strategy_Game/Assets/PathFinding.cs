using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    /// <summary>
    /// Find a path between two points.
    /// </summary>
    /// <param name="gridMap">Grid to search.</param>
    /// <param name="startPos">Starting position.</param>
    /// <param name="targetPos">Ending position.</param>
    /// <returns>List of points that represent the path to walk.</returns>
    public static List<Vector2Int> FindPath(GridMap gridMap, Vector2Int startPos, Vector2Int targetPos)
    {
        if (startPos == targetPos)
            return null;
        // find path
        List<Node> nodes_path = _ImpFindPath(gridMap, startPos, targetPos);

        // convert to a list of points and return
        List<Vector2Int> ret = new List<Vector2Int>();
        if (nodes_path != null)
        {
            foreach (Node node in nodes_path)
            {
                ret.Add(new Vector2Int(node.gridX, node.gridY));
            }
        }
        return ret;
    }

    /// <summary>
    /// Internal function that implements the path-finding algorithm.
    /// </summary>
    /// <param name="gridMap">Grid to search.</param>
    /// <param name="startPos">Starting position.</param>
    /// <param name="targetPos">Ending position.</param>
    /// <returns>List of grid nodes that represent the path to walk.</returns>
    private static List<Node> _ImpFindPath(GridMap gridMap, Vector2Int startPos, Vector2Int targetPos)
    {
        Node startNode = gridMap.nodes[startPos.x, startPos.y];
        Node targetNode = gridMap.nodes[targetPos.x, targetPos.y];

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
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
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbour in gridMap.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Retrace path between two points.
    /// </summary>
    /// <param name="startNode">Starting node.</param>
    /// <param name="endNode">Ending (target) node.</param>
    /// <returns>Retraced path between nodes.</returns>
    private static List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
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

