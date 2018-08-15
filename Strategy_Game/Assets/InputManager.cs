using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour {

    public static InputManager instance;

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    
    public Grid grid;
    public GridMap gridMap;
    public TileBase ground;
    public TileBase redGround;
    private SpriteRenderer spriteRenderer;
    private Color storeColor = Color.white;

    bool isSelected;

    public int mapSizeX;
    public int mapSizeY;

    [HideInInspector]
    public Tilemap tilemap;

    public GameObject draggingPreb;
    private GameObject buildingPreb;
    private Transform selectedBuilding;

    // Use this for initialization
    void Start () {
        Application.targetFrameRate = 60;
        Camera.main.transform.position = new Vector3(mapSizeX / 2, mapSizeY / 2, -10);

        tilemap = grid.GetComponentInChildren<Tilemap>();

        draggingPreb.SetActive(false);
        spriteRenderer = draggingPreb.GetComponentInChildren<SpriteRenderer>();

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
        // If mouse around UI simply return
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = grid.WorldToCell(mousePos);
        mousePos.z = 0;

        // No building selected
        if (!isSelected)
        {
            if(Input.GetMouseButtonDown(1))
            {
                UIManager.instance.ShowBuildings();
            }
            // Produce soldier to best nearest fit place to the barrack
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null && hit.transform.parent.CompareTag("Building"))
                {
                    selectedBuilding = hit.transform.parent;
                    UIManager.instance.ShowSelectedBuildingProduce(selectedBuilding.GetComponent<Entity>());
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

    public void CreateSoldier(GameObject soldier)
    {
        //TODO: need spawn point and rally point

        Vector2 emptySpot = selectedBuilding.transform.position;
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
                    //if (k >= 0 && l >= 0)
                    if (k > 1 && l > 1 && k < mapSizeX && l < mapSizeY)
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
        GameObject obj = Instantiate(soldier, emptySpot, Quaternion.identity);
        obj.transform.parent = grid.transform.GetChild(1);
    }

    // GameObject obj, Vector3Int pos, int width, int height
    void CreateBarrack(Vector3Int pos)
    {
        //TODO: need to change dynamick sizes and get rid off foreach loop
        GameObject _obj = Instantiate(buildingPreb, pos, Quaternion.identity);
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

        UIManager.instance.InformationHolder.gameObject.SetActive(false); // Close Information UI

    }

    //Checking to square of origin if it is true available to build
    bool CheckNeighbor(Vector3Int origin)
    {
        int x = origin.x;
        int y = origin.y;
        if (
            x < 0         || y < 0 ||
            x >= mapSizeX || y >= mapSizeY
            ||
            !gridMap.nodes[x, y].walkable
           )
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

    public void SelectedBuilding(Entity building)
    {
        buildingPreb = building.gameObject;

        spriteRenderer.sprite = building.sprite;
        spriteRenderer.size = building.size;

        building.GetComponentInChildren<BoxCollider2D>().size = building.size;

        TMPro.TextMeshPro text = draggingPreb.GetComponentInChildren<TMPro.TextMeshPro>();
        text.rectTransform.sizeDelta = building.size;
        text.text = building._name;

        storeColor = building.GetComponentInChildren<SpriteRenderer>().color;
        isSelected = true;

        draggingPreb.SetActive(isSelected);

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
