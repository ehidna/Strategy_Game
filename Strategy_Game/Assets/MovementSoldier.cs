using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSoldier : MonoBehaviour {

    private Deneme deneme;
    Grid grid;

    public float moveSpeed;
    public float rotSpeed;
    public float waitTime = 4f;
    public int range = 3;
    private float waiting;

    [SerializeField]
    private bool isWandering;

    [SerializeField]
    private bool isReachedDestination;

    [SerializeField]
    private bool isRotating;

    Vector3 destination;
    Vector3 direction;

    Transform myTransform;

    List<Vector2Int> pathList;

	// Use this for initialization
	void Start () {
        myTransform = transform;
        deneme = FindObjectOfType<Deneme>();
        waiting = waitTime;
        grid = deneme.grid;
	}

    private void OnDrawGizmos()
    {
        if (pathList != null && pathList.Count != 0)
        {
            Gizmos.color = Color.red;
            Vector2Int v2 = pathList[pathList.Count - 1];
            Gizmos.DrawLine(myTransform.position, new Vector3(v2.x, v2.y, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (isWandering)
        {
            if (isReachedDestination)
            {
                direction = Vector3.zero;
                destination = Vector3.zero;
                isWandering = false;
                pathList = null;
                return;
            }
            Wander();
        }
        else
        {
            waiting -= Time.deltaTime;
            if (waiting <= 0)
            {
                isWandering = true;
                waiting = waitTime;
                isReachedDestination = false;
            }
        }

    }

    void Wander()
    {
        Vector3Int gridPos = grid.WorldToCell(myTransform.position);
        if (pathList == null)
        {
            Debug.Log("wanderstart");
            deneme.gridMap.nodes[gridPos.x, gridPos.y].walkable = true;
            deneme.tilemap.SetTile(new Vector3Int(gridPos.x, gridPos.y, 0), deneme.ground);
            pathList = PathFinding.FindPath(deneme.gridMap,
                                            new Vector2Int(gridPos.x, gridPos.y),
                                            SetWanderSpot(new Vector2Int(gridPos.x, gridPos.y))
                                           );
            if (pathList != null && pathList.Count > 0)
            {
                Vector2Int first = pathList[0];
                deneme.gridMap.nodes[first.x, first.y].walkable = false;
                deneme.tilemap.SetTile(new Vector3Int(first.x, first.y, 0), deneme.redGround);
                direction = new Vector3(first.x, first.y, 0);
                isRotating = true;
                Vector2Int last = pathList[pathList.Count - 1];
                destination = new Vector3(last.x, last.y, 0);
            }
        }
        else
        {
            if (Vector3.Distance(myTransform.position, direction) <= 0f)
            {
                if (pathList.Count > 1)
                {
                    // Clear last passed tile to walkable
                    deneme.gridMap.nodes[pathList[0].x, pathList[0].y].walkable = true;
                    deneme.tilemap.SetTile(new Vector3Int(pathList[0].x, pathList[0].y, 0), deneme.ground);
                    pathList.RemoveAt(0);

                    if (deneme.gridMap.nodes[pathList[0].x, pathList[0].y].walkable)
                    {
                        Vector2 v = pathList[0];
                        deneme.gridMap.nodes[pathList[0].x, pathList[0].y].walkable = false;
                        deneme.tilemap.SetTile(new Vector3Int(pathList[0].x, pathList[0].y, 0), deneme.redGround);
                        direction = new Vector3(v.x, v.y, 0);
                    }
                    else
                    {
                        pathList = PathFinding.FindPath(deneme.gridMap,
                                            new Vector2Int(gridPos.x, gridPos.y),
                                                        new Vector2Int((int)destination.x, (int)destination.y)
                                           );
                        if (pathList!=null && pathList.Count > 0)
                        {
                            Vector2Int v = pathList[0];
                            direction = new Vector3(v.x, v.y, 0);
                            deneme.gridMap.nodes[v.x, v.y].walkable = true;
                            deneme.tilemap.SetTile(new Vector3Int(v.x, v.y, 0), deneme.ground);
                        }
                    }
                }
                else
                {
                    Debug.Log("isreached destination");
                    isReachedDestination = true;
                    if (deneme.gridMap.nodes[gridPos.x, gridPos.y].walkable)
                    {
                        Debug.Log("boom");
                    }
                    deneme.gridMap.nodes[gridPos.x, gridPos.y].walkable = false;
                    deneme.tilemap.SetTile(new Vector3Int(gridPos.x, gridPos.y, 0), deneme.redGround);
                    return;
                }
            }
            myTransform.position = Vector3.MoveTowards(myTransform.position, direction, moveSpeed * Time.deltaTime);
        }

    }


    Vector2Int SetWanderSpot(Vector2Int pos)
    {
        int i = 0, j = 0;

        int validCount = 10;
        while (validCount > 0)
        {
            validCount--;
            //Searching for around to barrack
            i = Random.Range(-range, range + 1);
            j = Random.Range(-range, range + 1);

            if ((i == 0 && j == 0) || i + pos.x < 0 || j + pos.y < 0 || i + pos.x >= deneme.mapSizeX || j + pos.y >= deneme.mapSizeY)
                continue;
            if (deneme.gridMap.nodes[i + pos.x, j + pos.y].walkable)
            {
                validCount = 0;
                break;
            }
            if(validCount == 0)
            {
                i = 0;
                j = 0;
                Debug.LogError("Didn't found any valid spot");
            }
        }

        return new Vector2Int(i + pos.x, j + pos.y);
    }
}
