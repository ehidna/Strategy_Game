using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    //Singleton
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

    Vector3 InvalidPosition = Vector3.negativeInfinity;

    public Grid grid;
    public GridMap gridMap;
    public TileBase ground;
    public TileBase redGround;
    public TileBase flag;


    bool isBuildingPicked;
    private bool SelectedObject;

    public int mapSizeX;
    public int mapSizeY;

    [HideInInspector]
    public Tilemap tilemap;
    private Tilemap foreGroundTM;

    public GameObject draggingPreb;
    private GameObject buildingPreb;
    private Transform selectedBuilding;

    public Material defaultMaterial;
    public Material selectionMaterial;
    private SpriteRenderer spriteRenderer;
    private Color storedColor = Color.white;

    Vector3 offs = new Vector3();

    private List<MovementSoldier> takingOrderSoldiers;

    // Use this for initialization
    void Start()
    {
        Application.targetFrameRate = 60;
        CameraScript cameraS = Camera.main.transform.GetComponent<CameraScript>();
        cameraS.limitX = mapSizeX;
        cameraS.limitY = mapSizeY;
        cameraS.transform.position = new Vector3(mapSizeX / 2, mapSizeY / 2, -10);
        tilemap = grid.GetComponentInChildren<Tilemap>();
        foreGroundTM = grid.transform.GetChild(1).GetComponent<Tilemap>();

        draggingPreb.SetActive(false);
        spriteRenderer = draggingPreb.GetComponentInChildren<SpriteRenderer>();

        tilemap.ClearAllTiles();
        SetTiles();
    }
    /// <summary>
    /// Setting all tiles on grid with mapsizes.
    /// </summary>
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
            if (Input.GetMouseButtonDown(0))
            {
                LeftMouseClick(mousePos);
            }
            if (Input.GetMouseButtonDown(1))
            {
                RightMouseClick(gridPos);
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
                    Vector3 calPos = gridPos;
                    calPos += new Vector3(
                            offs.x % 1,
                            offs.y % 1,
                            0
                        );
                    draggingPreb.transform.position = calPos;
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
    /// <summary>
    /// Creating soldiers on selected barrack.
    /// </summary>
    /// <param name="soldier">Soldier gameobject.</param>
    public void CreateSoldier(GameObject soldier)
    {
        Vector3Int buildingPos = grid.WorldToCell(selectedBuilding.position);
        Vector3 flagTarget = selectedBuilding.GetComponent<Barrack>().spawnTarget;

        Node node = new Node(false, buildingPos.x, buildingPos.y);
        gridMap.GetValidNeighbour(ref node);
        buildingPos = new Vector3Int(node.gridX, node.gridY, 0);

        gridMap.nodes[buildingPos.x, buildingPos.y].isWalkable = false;
        tilemap.SetTile(buildingPos, redGround);

        GameObject obj = ObjectPooler.Instance.GetPooledObject(soldier.name);
        obj.transform.position = buildingPos;
        obj.SetActive(true);
        MovementSoldier movementSoldier = obj.GetComponent<MovementSoldier>();
        movementSoldier.target = flagTarget;
        movementSoldier.StartMovement();
    }

    /// <summary>
    /// Create selected barrack on buildings menu
    /// </summary>
    /// <param name="pos">World position to gridmap position.</param>
    void CreateBarrack(Vector3Int pos)
    {
        Entity entity = buildingPreb.GetComponent<Entity>();

        // because of origin issue 2 and folds
        Vector3 calPos = pos;
        calPos += new Vector3(
                offs.x % 1,
                offs.y % 1,
                0
        );

        GameObject _obj = ObjectPooler.Instance.GetPooledObject(buildingPreb.name);
        _obj.transform.position = calPos;
        _obj.SetActive(true);

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

    /// <summary>
    /// Checking to square of origin if it is true available to build
    /// </summary>
    /// <param name="bottomleft">bottom left to the building position</param>
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
    /// <summary>
    /// Calculating position offsets and assigning parameters 
    /// </summary>
    /// <param name="building">Entity component of building</param>
    public void SelectedBuilding(Entity building)
    {
        buildingPreb = building.gameObject;

        offs = new Vector3(building.size.x / 2, building.size.y / 2, 0);

        // because of origin issue 2 and folds
        if ((int)building.size.x % 2 == 0)
        {
            offs.x -= 0.5f;
        }
        else
            offs.x = Mathf.FloorToInt(offs.x);
        if ((int)building.size.y % 2 == 0)
        {
            offs.y -= 0.5f;
        }
        else
            offs.y = Mathf.FloorToInt(offs.y);
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
    /// <summary>
    /// Adding rally point for barracks
    /// </summary>
    /// <param name="building">Entity component of building</param>
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
    /// <summary>
    /// Checking to mouse has moved or not
    /// </summary>
    bool HasMouseMoved()
    {
        return
            (System.Math.Abs(Input.GetAxis("Mouse X")) > 0)
            ||
            (System.Math.Abs(Input.GetAxis("Mouse Y")) > 0)
            ;
    }

    /// <summary>
    /// Checking inside of map to the mouse events
    /// </summary>
    public bool MouseInBounds()
    {
        //Screen coordinates start in the lower-left corner of the screen
        //not the top-left of the screen like the drawing coordinates do
        Vector3 mousePos = Input.mousePosition;
        // Map is between %25 and %75 of width
        bool insideWidth = mousePos.x >= Screen.width * 0.25f && mousePos.x <= Screen.width * 0.75f;
        bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height;
        return insideWidth && insideHeight;
    }
    /// <summary>
    /// Left mouse click. Checking hit points of collider, is soldier or building?
    /// </summary>
    /// <param name="mousePos">Mouse position on grid map</param>
    private void LeftMouseClick(Vector2 mousePos)
    {
        if (MouseInBounds())
        {
            GameObject hitObject = FindHitObject(mousePos);
            Vector3 hitPoint = FindHitPoint(mousePos);
            if (takingOrderSoldiers != null && takingOrderSoldiers.Count > 0)
                for (int i = 0; i < takingOrderSoldiers.Count; i++)
                    takingOrderSoldiers[i].GetComponentInChildren<SpriteRenderer>().material = defaultMaterial;

            takingOrderSoldiers = null;
            if (hitObject && hitPoint != InvalidPosition && hitObject.CompareTag("Soldier"))
            {
                MovementSoldier soldierMovement = hitObject.GetComponent<MovementSoldier>();
                if (soldierMovement)
                {   
                    if(takingOrderSoldiers == null)
                        takingOrderSoldiers = new List<MovementSoldier>();
                    takingOrderSoldiers.Add(soldierMovement);
                    soldierMovement.GetComponentInChildren<SpriteRenderer>().material = selectionMaterial;
                }
            }
            else if (hitObject && hitPoint != InvalidPosition && hitObject.CompareTag("Building"))
            {
                selectedBuilding = hitObject.transform;
                UIManager.instance.ShowSelectedBuildingProduce(selectedBuilding.GetComponent<Entity>());
                return;
            }

            foreGroundTM.ClearAllTiles();
            UIManager.instance.ShowBuildings();
            selectedBuilding = null;
        }
    }

    /// <summary>
    /// Mouse Right Click. if building selected than set rally point,
    /// If soldiers selected than set target position and enable Movement of soldiers.
    /// </summary>
    /// <param name="mousePos">Mouse position on grid map</param>
    private void RightMouseClick(Vector3Int mousePos)
    {
        if (MouseInBounds())
        {
            if ((takingOrderSoldiers == null || takingOrderSoldiers.Count == 0) && selectedBuilding != null)
            {
                foreGroundTM.ClearAllTiles();
                selectedBuilding.GetComponent<Barrack>().spawnTarget = mousePos;
                foreGroundTM.SetTile(mousePos, flag);
            }
            else
            {
                foreGroundTM.ClearAllTiles();
                if ((takingOrderSoldiers != null && takingOrderSoldiers.Count > 0))
                {
                    
                    for (int i = 0; i < takingOrderSoldiers.Count; i++)
                    {
                        takingOrderSoldiers[i].target = mousePos;
                        takingOrderSoldiers[i].enabled = true;
                        takingOrderSoldiers[i].StartMovement();
                    }
                    foreGroundTM.SetTile(mousePos, redGround);
                }
            }
        }
    }
    /// <summary>
    /// Finding hit point 
    /// </summary>
    /// <param name="mousePos">Mouse position on grid map</param>
    private Vector3 FindHitPoint(Vector2 mousePos)
    {

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider != null)
        {
            return hit.point;
        }
        return InvalidPosition;
    }
    /// <summary>
    /// Finding hit point of object.
    /// </summary>
    /// <param name="mousePos">Mouse position on grid map</param>
    private GameObject FindHitObject(Vector2 mousePos)
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider != null)
        {
            return hit.collider.transform.parent.gameObject;
        }
        return null;
    }
}
