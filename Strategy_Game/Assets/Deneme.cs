using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Deneme : MonoBehaviour {
    
    public Grid grid;
    public GridMap gridMap;
    public TileBase ground;
    public TileBase redGround;
    private SpriteRenderer spriteRenderer;
    private Color storeColor;

    bool isSelected;

    public int mapSizeX;
    public int mapSizeY;

    public Tilemap tilemap;

    public GameObject SoldierPreb;
    public GameObject barrackPreb;
    private GameObject draggingPreb;

    // Use this for initialization
    void Start () {
        Application.targetFrameRate = 60;
        Camera.main.transform.position = new Vector3(mapSizeX / 2, mapSizeY / 2, -10);

        tilemap = grid.GetComponentInChildren<Tilemap>();

        draggingPreb = Instantiate(barrackPreb);
        Destroy(draggingPreb.GetComponentInChildren<Collider2D>());
        draggingPreb.SetActive(false);
        spriteRenderer = draggingPreb.GetComponentInChildren<SpriteRenderer>();
        storeColor = spriteRenderer.color;

        tilemap.ClearAllTiles();
        SetTiles();
    }

    void SetTiles()
    {
        // set true all tiles to walkable
        bool[,] tilesMap = new bool[mapSizeX, mapSizeY];
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), ground);
                tilesMap[i, j] = true;
            }
        }
        gridMap = new GridMap(tilesMap);
    }

    // Update is called once per frame
    void Update()
    {      

        if (Input.GetKeyDown(KeyCode.Space))
        {
            MovementSoldier[] soldiers = FindObjectsOfType<MovementSoldier>();
            foreach (var item in soldiers)
            {
                item.enabled = false;
            }

            for (int i = 0; i < mapSizeX; i++)
            {
                for (int j = 0; j < mapSizeY; j++)
                {
                    if (!gridMap.nodes[i, j].walkable)
                        tilemap.SetTile(new Vector3Int(i, j, 0), redGround);
                    else
                        tilemap.SetTile(new Vector3Int(i, j, 0), ground);

                }
            }

        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = grid.WorldToCell(mousePos);
        mousePos.z = 0;

        // No building selected
        if (!isSelected)
        {
            //Right mouse button for selecting barrack
            if (Input.GetMouseButtonDown(1))
            {
                isSelected = true;
                draggingPreb.transform.position = gridPos;
                draggingPreb.SetActive(isSelected);
            }

            // Produce soldier to best nearest fit place to the barrack
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null && hit.transform.parent.CompareTag("Building"))
                {
                    CreateSoldier(hit);
                }
            }
        }
        else
        {
            // Checking for available places to buildings
            bool isplaceable = CheckNeighbor(gridPos);
            if (isplaceable)
            {
                if (HasMouseMoved())
                {
                    draggingPreb.transform.position = gridPos;
                    spriteRenderer.color = Color.green;
                }
                if (Input.GetMouseButtonDown(0))
                {
                    CreateBarrack(gridPos);
                    isSelected = false;
                    draggingPreb.SetActive(isSelected);
                    spriteRenderer.color = storeColor;
                }
            }
            else
            {
                spriteRenderer.color = Color.red;
            }
        }
    }

    void CreateSoldier(RaycastHit2D hit)
    {
        Vector2 emptySpot = hit.transform.position;
        int x = Mathf.RoundToInt(emptySpot.x), y = Mathf.RoundToInt(emptySpot.y), i = 1, j = 1;

        bool isLooping = true;
        while (isLooping)
        {
            //Searching for around to barrack
            //gridMap.GetNeighbours(gridMap.nodes[x, y]);
            for (int k = x - i; k < x + i + 1; k++)
            {
                for (int l = y - j; l < y + j + 1; l++)
                {
                    if (k >= 0 && l >= 0)
                    if (gridMap.nodes[k, l].walkable)
                        {
                            emptySpot = new Vector2(k, l);
                            isLooping = false;
                            break;
                        }
                }
            }

            i++;
            j++;

        }
        gridMap.nodes[Mathf.RoundToInt(emptySpot.x), Mathf.RoundToInt(emptySpot.y)].walkable = false;
        tilemap.SetTile(new Vector3Int(Mathf.RoundToInt(emptySpot.x), Mathf.RoundToInt(emptySpot.y), 0), redGround);
        GameObject obj = Instantiate(SoldierPreb, emptySpot, Quaternion.identity);
        obj.transform.parent = grid.transform.GetChild(1);
    }

    // GameObject obj, Vector3Int pos, int width, int height
    void CreateBarrack(Vector3Int pos)
    {
        GameObject _obj = Instantiate(barrackPreb, pos, Quaternion.identity);
        _obj.transform.parent = grid.transform.GetChild(1);

        List<Node> nodes = gridMap.GetNeighbours(gridMap.nodes[pos.x, pos.y]);
        gridMap.nodes[pos.x, pos.y].walkable = false;
        tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), redGround);
        // Neighbours
        foreach (var item in nodes)
        {
            tilemap.SetTile(new Vector3Int(item.gridX, item.gridY, 0), redGround);
            item.walkable = false;
        }
    }

    //Checking to square of origin if it is true available to build
    bool CheckNeighbor(Vector3Int origin)
    {
        int x = origin.x;
        int y = origin.y;
        if (x < 0 || y < 0 || x  >= mapSizeX || y  >= mapSizeY)
            return false;
        List<Node> nodes = gridMap.GetNeighbours(gridMap.nodes[x, y]);
        if (nodes.Count != 8)
            return false;
        foreach (var item in nodes)
        {
            if (!item.walkable)
                return false;
        }

        return true;
    }

    bool HasMouseMoved()
    {
        return 
            (System.Math.Abs(Input.GetAxis("Mouse X")) > 0) 
            ||
            (System.Math.Abs(Input.GetAxis("Mouse Y")) > 0)
            ;
    }
}
