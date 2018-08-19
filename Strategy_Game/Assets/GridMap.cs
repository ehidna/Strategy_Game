using System.Collections.Generic;

public class GridMap{

    // nodes in grid
    public Node[,] nodes;

    // grid size
    int gridSizeX, gridSizeY;

    /// <summary>
    /// Create a new grid, eg with just walkable / unwalkable tiles.
    /// </summary>
    /// <param name="walkable_tiles">A 2d array representing which tiles are walkable and which are not.</param>
    public GridMap(bool[,] walkable_tiles)
    {
        // create nodes
        CreateNodes(walkable_tiles.GetLength(0), walkable_tiles.GetLength(1));

        // init nodes
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                nodes[x, y] = new Node(walkable_tiles[x, y], x, y);
            }
        }
    }

    /// <summary>
    /// Create the nodes grid and set size.
    /// </summary>
    /// <param name="width">Nodes grid width.</param>
    /// <param name="height">Nodes grid height.</param>
    private void CreateNodes(int width, int height)
    {
        gridSizeX = width;
        gridSizeY = height;
        nodes = new Node[gridSizeX, gridSizeY];
    }

    /// <summary>
    /// Updates the already created grid, eg with just walkable / unwalkable tiles.
    /// </summary>
    /// <returns><c>true</c>, if grid was updated, <c>false</c> otherwise.</returns>
    /// <param name="walkable_tiles">Walkable tiles.</param>
    public void UpdateGrid(bool[,] walkable_tiles)
    {
        // check if need to re-create grid
        if (nodes == null ||
            gridSizeX != walkable_tiles.GetLength(0) ||
            gridSizeY != walkable_tiles.GetLength(1))
        {
            CreateNodes(walkable_tiles.GetLength(0), walkable_tiles.GetLength(1));
        }

        // update grid
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                nodes[x, y].Update(walkable_tiles[x, y], x, y);
            }
        }
    }

    /// <summary>
    /// Get all the neighbors of a given tile in the grid.
    /// </summary>
    /// <param name="node">Node to get neighbors for.</param>
    /// <returns>List of node neighbors.</returns>
    public void GetNeighbours(Node node, ref List<Node> neighbours)
    {
        int x = 0, y = 0;

        for (x = -1; x <= 1; x++)
        {
            for (y = -1; y <= 1; y++)
            {
                AddNodeNeighbour(x, y, node, neighbours);
            }
        }
    }

    /// <summary>
    /// Get valid neighbor of a given tile in the grid.
    /// </summary>
    /// <param name="node">Node to get neighbor for.</param>
    public void GetValidNeighbour(ref Node node)
    {
        List<Node> neighbours = new List<Node>();
        GetNeighbours(node, ref neighbours);

        for (int k = 0; k < neighbours.Count; k++)
            if (neighbours[k].isWalkable)
            {
                node = neighbours[k];
                return;
            }
        float x =1, y = 1;
        int count = 2;

        int size;
        if (gridSizeX > gridSizeY)
            size = gridSizeX;
        else
            size = gridSizeY;
        
        for (int i = count; i < size; i++)
        {
            for (int m = -count; m <= count; m++)
            {
                for (int n = -count; n <= count; n++)
                {
                    if (m < 0)
                        x *= -m;
                    if (n < 0)
                        y *= -m;
                    if ((x + y) < count)
                        continue;
                    if (
                        (m + node.gridX) < 0 || (n + node.gridY) < 0
                    ||
                        (m + node.gridX) >= gridSizeX || (n + node.gridY) >= gridSizeY
                    )
                        continue;
                    if (nodes[m + node.gridX, n + node.gridY].isWalkable)
                    {
                        
                        node = nodes[m + node.gridX, n + node.gridY];
                        return;
                    }
                }
            }
            count++;
        }
    }

    /// <summary>
    /// Adds the node neighbour.
    /// </summary>
    /// <returns><c>true</c>, if node neighbour was added, <c>false</c> otherwise.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="node">Node.</param>
    /// <param name="neighbours">Neighbours.</param>
    private bool AddNodeNeighbour(int x, int y, Node node, List<Node> neighbours)
    {
        if (x == 0 && y == 0)
            return false;

        int checkX = node.gridX + x;
        int checkY = node.gridY + y;

        if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
        {
            neighbours.Add(nodes[checkX, checkY]);
            return true;
        }

        return false;
    }
}
