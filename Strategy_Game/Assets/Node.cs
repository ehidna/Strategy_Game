public class Node {

    // is this node walkable?
    public bool isWalkable;
    public int gridX;
    public int gridY;

    // calculated values while finding path
    public int gCost;
    public int hCost;
    public Node parent;

    /// <summary>
    /// Create the grid node.
    /// </summary>
    /// <param name="_walkable">Is this tile walkable?</param>
    /// <param name="_gridX">Node x index.</param>
    /// <param name="_gridY">Node y index.</param>
    public Node(bool _walkable, int _gridX, int _gridY)
    {
        isWalkable = _walkable;
        gridX = _gridX;
        gridY = _gridY;
    }

    /// <summary>
    /// Updates the grid node.
    /// </summary>
    /// <param name="_walkable">Is this tile walkable?</param>
    /// <param name="_gridX">Node x index.</param>
    /// <param name="_gridY">Node y index.</param>
    public void Update(bool _walkable, int _gridX, int _gridY)
    {
        isWalkable = _walkable;
        gridX = _gridX;
        gridY = _gridY;
    }

    /// <summary>
    /// Get fCost of this node.
    /// </summary>
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
}
