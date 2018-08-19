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
            Destroy(this.gameObject);
        }
    }

    public Grid grid;
    public GridMap gridMap;
    public TileBase ground;
    public TileBase redGround;
    public TileBase flag;
    private SpriteRenderer spriteRenderer;
    private Color storedColor = Color.white;

    bool isBuildingPicked;

    public int mapSizeX;
    public int mapSizeY;

    [HideInInspector]
    public Tilemap tilemap;
    private Tilemap foreGroundTM;

    public GameObject draggingPreb;
    private GameObject buildingPreb;
    private Transform selectedBuilding;

    Vector3 offs = new Vector3();

    // Use this for initialization
    void Start () {
        Application.targetFrameRate = 60;
        Camera.main.transform.position = new Vector3(mapSizeX / 2, mapSizeY / 2, -10);

        tilemap = grid.GetComponentInChildren<Tilemap>();
        foreGroundTM = grid.transform.GetChild(1).GetComponent<Tilemap>();

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
            for (int j = 0; j < mapSizeY; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), ground);
                tilesMap[i, j] = true;
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
        mousePos.z = 0;
        mousePos.x = Mathf.RoundToInt(mousePos.x);
        mousePos.y = Mathf.RoundToInt(mousePos.y);
        Vector3Int gridPos = grid.WorldToCell(mousePos);
        // No building selected
        if (!isBuildingPicked)
        {
            // Produce soldier to best nearest fit place to the barrack
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null && hit.transform.parent.CompareTag("Building"))
                {
                    selectedBuilding = hit.transform.parent;
                    UIManager.instance.ShowSelectedBuildingProduce(selectedBuilding.GetComponent<Entity>());
                }
                else
                {
                    foreGroundTM.ClearAllTiles();
                    UIManager.instance.ShowBuildings();
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                foreGroundTM.ClearAllTiles();
                selectedBuilding.GetComponent<Barrack>().spawnTarget = gridPos;
                foreGroundTM.SetTile(gridPos, flag);
            }
        }
        else
        {
            // stay bottom left of to the building for mouse
            bool isplaceable = CheckNeighbor(gridPos);

            gridPos += new Vector3Int(
                Mathf.FloorToInt(offs.x),
                Mathf.FloorToInt(offs.y),
                0
            );

            // Checking for available places to buildings
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
                    isBuildingPicked = false;
                    draggingPreb.SetActive(isBuildingPicked);
                    spriteRenderer.color = storedColor;
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
        Vector3Int buildingPos = grid.WorldToCell(selectedBuilding.position);
        Vector3 flagTarget = selectedBuilding.GetComponent<Barrack>().spawnTarget;

        Node node = new Node(false, buildingPos.x, buildingPos.y);
        gridMap.GetValidNeighbour(ref node);
        buildingPos = new Vector3Int(node.gridX, node.gridY, 0);

        gridMap.nodes[buildingPos.x, buildingPos.y].isWalkable = false;
        tilemap.SetTile(buildingPos, redGround);

        GameObject obj = Instantiate(soldier, buildingPos, Quaternion.identity);
        MovementSoldier movementSoldier = obj.GetComponent<MovementSoldier>();
        movementSoldier.target = flagTarget;
        movementSoldier.StartMovement();

        obj.transform.parent = grid.transform.GetChild(1);
    }

    // GameObject obj, Vector3Int pos, int width, int height
    void CreateBarrack(Vector3Int pos)
    {
        Entity entity = buildingPreb.GetComponent<Entity>();
        GameObject _obj = Instantiate(buildingPreb, pos, Quaternion.identity);
        _obj.transform.parent = grid.transform.GetChild(1);

        Barrack barrack = entity.GetComponent<Barrack>();
        if (barrack != null)
        {
            barrack.spawnTarget = -Vector3.one;
        }
        pos -= new Vector3Int(
                Mathf.FloorToInt(offs.x),
                Mathf.FloorToInt(offs.y),
                0
            );

        for (int i = 0; i < entity.size.x; i++)
        {
            for (int j = 0; j < entity.size.y; j++)
            {
                tilemap.SetTile(new Vector3Int(pos.x + i, pos.y + j, 0), redGround);
                gridMap.nodes[pos.x + i, pos.y + j].isWalkable = false;
            }
        }

        UIManager.instance.InformationHolder.gameObject.SetActive(false); // Close Information UI

    }

    //Checking to square of origin if it is true available to build
    bool CheckNeighbor(Vector3Int bottomleft)
    {
        Entity entity = buildingPreb.GetComponent<Entity>();
        int x = bottomleft.x;
        int y = bottomleft.y;
        if (
            x <= 0 || y <= 0 ||
            x + entity.size.x - 2 > mapSizeX || y + entity.size.y - 2 > mapSizeY
            ||
            !gridMap.nodes[x, y].isWalkable
        )
            return false;
        for (int i = 0; i < entity.size.x; i++)
        {
            for (int j = 0; j < entity.size.y; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                if (!gridMap.nodes[x + i, y + j].isWalkable)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void SelectedBuilding(Entity building)
    {

        buildingPreb = building.gameObject;
        offs = new Vector3(building.size.x / 2, building.size.y / 2, 0);
        spriteRenderer.sprite = building.sprite;
        spriteRenderer.size = building.size;


        building.GetComponentInChildren<BoxCollider2D>().size = building.size;

        TMPro.TextMeshPro text = draggingPreb.GetComponentInChildren<TMPro.TextMeshPro>();
        text.rectTransform.sizeDelta = building.size;
        text.text = building._name;

        storedColor = building.GetComponentInChildren<SpriteRenderer>().color;
        isBuildingPicked = true;

        draggingPreb.SetActive(isBuildingPicked);

    }

    public void CheckFlag(Entity building)
    {
        if (buildingPreb != null)
        {
            foreGroundTM.ClearAllTiles();
            Barrack barrack = building.GetComponent<Barrack>();
            if (barrack != null)
            {
                if (barrack.spawnTarget != -Vector3.one)
                {
                    Vector3Int vector3Int = new Vector3Int(
                        Mathf.RoundToInt(barrack.spawnTarget.x),
                        Mathf.RoundToInt(barrack.spawnTarget.y),
                        0);
                    foreGroundTM.SetTile(vector3Int, flag);
                }
            }
        }
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
