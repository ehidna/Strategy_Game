using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Deneme : MonoBehaviour {
    
    public Grid grid;
    public TileBase tbase;
    private SpriteRenderer spriteRenderer;
    private Color storeColor;

    bool isSelected;

    public int size;

    public float EPSILON { get; private set; }
    Tilemap tilemap;

    public GameObject SoldierPreb;
    public GameObject barrackPreb;
    private GameObject draggingPreb;

    // 0 for ground, 1 for empty or has collider
    private byte[,] cells;

    // Use this for initialization
    void Start () {
        Application.targetFrameRate = 60;

        tilemap = grid.GetComponentInChildren<Tilemap>();

        draggingPreb = Instantiate(barrackPreb);
        Destroy(draggingPreb.GetComponentInChildren<Collider2D>());
        draggingPreb.SetActive(false);
        spriteRenderer = draggingPreb.GetComponentInChildren<SpriteRenderer>();
        storeColor = spriteRenderer.color;

        cells = new byte[size, size];

        tilemap.ClearAllTiles();
        SetTiles();
	}

    void SetTiles()
    {
        // arrays cant be negative, we are adding half of the map size ( size/2 ) to solve this problem
        for (int i = -size / 2; i < size / 2; i++)
        {
            for (int j = -size / 2; j < size / 2; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), tbase);
                cells[i + size / 2, j + size / 2] = 0;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = grid.WorldToCell(mousePos);
        mousePos.z = 0;

        // No buildings selected
        if (!isSelected)
        {
            //Right mouse button for selecting barrack
            if (Input.GetMouseButtonUp(1))
            {
                isSelected = true;
                draggingPreb.transform.position = gridPos;
                draggingPreb.SetActive(isSelected);
            }

            // Produce soldier to best nearest fit place for barrack
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
            bool isplaceable = CheckNeighbor(gridPos);
            if (isplaceable && Input.GetMouseButtonDown(0))
            {
                //if (CheckNeighbor(gridPos))
                //{
                    CreateBarrack(gridPos);
                //}
                isSelected = false;
                draggingPreb.SetActive(isSelected);
                spriteRenderer.color = storeColor;
            }
            if(HasMouseMoved())
            {
                draggingPreb.transform.position = gridPos;
                // Checking for available places to barrack
                if (isplaceable)
                {
                    spriteRenderer.color = Color.green;
                }
                else
                {
                    spriteRenderer.color = Color.red;
                }
            }
        }
    }

    void CreateSoldier(RaycastHit2D hit)
    {
        Vector2 emptySpot = hit.transform.position;
        int x = (int)emptySpot.x + size / 2, y = (int)emptySpot.y + size / 2, i = 1, j = 1;

        bool isLooping = true;
        while (isLooping)
        {
            //Searching for around to barrack
            for (int k = x - i; k < x + i + 1; k++)
            {
                for (int l = y - j; l < y + j + 1; l++)
                {
                    if (k >= 0 && l >= 0)
                        if (cells[k, l] == 0)
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
        cells[(int)emptySpot.x, (int)emptySpot.y] = 1;
        emptySpot.x -= size / 2;
        emptySpot.y -= size / 2;
        GameObject obj = Instantiate(SoldierPreb, emptySpot, Quaternion.identity);
        obj.transform.parent = grid.transform.GetChild(1);
    }

    // GameObject obj, Vector3Int pos, int width, int height
    void CreateBarrack(Vector3Int pos)
    {
        GameObject _obj = Instantiate(barrackPreb, pos, Quaternion.identity);
        _obj.transform.parent = grid.transform.GetChild(1);
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                cells[pos.x + i + size / 2, pos.y + j + size / 2] = 1;
            }
        }
    }

    //Checking to square of origin if it is true available to build
    bool CheckNeighbor(Vector3Int origin)
    {
        int x = origin.x;
        int y = origin.y;

        for (int i = x - 1; i < x + 2; i++)
        {
            for (int j = y - 1; j < y + 2; j++)
            {
                if (i + size / 2 >= 0 && j + size / 2 >= 0)
                    if (cells[i + size / 2, j + size / 2] == 1)
                        return false;
            }
        }
        return true;
    }

    bool HasMouseMoved()
    {
        return 
            (System.Math.Abs(Input.GetAxis("Mouse X")) > EPSILON) 
            ||
            (System.Math.Abs(Input.GetAxis("Mouse Y")) > EPSILON)
            ;
    }
}
