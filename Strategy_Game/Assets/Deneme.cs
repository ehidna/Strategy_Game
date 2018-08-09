using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Deneme : MonoBehaviour {
    
    public Grid grid;
    public TileBase tbase;

    bool selected;

    public int size;

    public float EPSILON { get; private set; }
    Tilemap tilemap;

    public GameObject barrackPreb;
    private GameObject draggingPreb;

    // 0 for ground, 1 for empty or has collider
    private byte[,] cells;
    // Storing last known position
    Vector3Int storeLKP = new Vector3Int();

    // Use this for initialization
    void Start () {
        tilemap = grid.GetComponentInChildren<Tilemap>();
        draggingPreb = Instantiate(barrackPreb);
        Destroy(draggingPreb.GetComponentInChildren<Collider2D>());
        draggingPreb.SetActive(false);
        cells = new byte[size, size];
        tilemap.ClearAllTiles();
        SetTiles();
	}

    void SetTiles()
    {
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
	void Update () {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = grid.WorldToCell(mousePos);
        mousePos.z = 0;


        //Right mouse button
        if (Input.GetMouseButtonUp(1)){
            selected = true;
            draggingPreb.transform.position = gridPos;
            draggingPreb.SetActive(selected);
        }
        // Checking for available places to barrack
        bool isplaceable = CheckNeighbor(gridPos);
        if (selected && HasMouseMoved() && isplaceable)
        {
            draggingPreb.transform.position = gridPos;
            storeLKP = gridPos;
        }
        // Left mouse button
        if (selected && Input.GetMouseButtonDown(0))
        {
            if (CheckNeighbor(storeLKP))
            {
                GameObject obj = Instantiate(barrackPreb, storeLKP, Quaternion.identity);
                obj.transform.parent = grid.transform.GetChild(1);
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        cells[storeLKP.x + i + size/2, storeLKP.y + j + size/2] = 1;
                    }
                }
            }
            selected = false;
            draggingPreb.SetActive(selected);
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
                if (i + size/2 >= 0 && j + size/2 >= 0)
                    if (cells[i + size/2, j + size/2] == 1)
                        return false;
            }
        }
        return true;
    }

    bool HasMouseMoved()
    {
        return (System.Math.Abs(Input.GetAxis("Mouse X")) > EPSILON) || (System.Math.Abs(Input.GetAxis("Mouse Y")) > EPSILON);
    }
}
