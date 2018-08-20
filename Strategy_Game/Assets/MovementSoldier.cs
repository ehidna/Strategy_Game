using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSoldier : Soldier {
    
    public float moveSpeed;
    public float rotSpeed;

    private bool isWandering;
    private bool isReachedDestination;
    private bool isRotating;

    Vector3 direction;

    Transform myTransform;

    List<Vector3Int> pathList;
    List<Node> neighbours = new List<Node>();

    Node node;

    public Vector3 target;
    private Vector3Int gridPos;


	// Use this for initialization
	void Start () {
        myTransform = transform;
	}
    /// <summary>
    /// drawing gizmos to pathway of soldiers for debugging.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (pathList != null && pathList.Count != 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(myTransform.position, pathList[pathList.Count -1]);
        }
    }

    private void OnEnable()
    {
        isReachedDestination = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isWandering)
        {
            if (target == -Vector3.one)
            {
                isWandering = false;
                return;
            }
            if (isReachedDestination)
            {
                direction = Vector3.zero;
                isWandering = false;
                pathList = null;
                target = -Vector3.one;
                InputManager.instance.gridMap.nodes[gridPos.x, gridPos.y].isWalkable = false;
                InputManager.instance.tilemap.SetTile(gridPos, InputManager.instance.redGround);
                this.enabled = false;
                return;
            }
            Wander();
        }
    }
    /// <summary>
    /// Start Movement with delay
    /// </summary>
    public void StartMovement()
    {
        StartCoroutine(ConstructWait());    
    }
    /// <summary>
    /// Delay for initialize
    /// </summary>
    IEnumerator ConstructWait()
    {
        yield return new WaitForSeconds(0.3f);
        isWandering = true;
        gridPos = InputManager.instance.grid.WorldToCell(myTransform.position);
        if (pathList != null && pathList.Count > 0)
        {
            InputManager.instance.gridMap.nodes[pathList[0].x, pathList[0].y].isWalkable = true;
            InputManager.instance.tilemap.SetTile(pathList[0], InputManager.instance.ground);
        }
        if (target != -Vector3.one)
            SetTarget(gridPos, new Vector3Int(
                Mathf.RoundToInt(target.x),
                Mathf.RoundToInt(target.y),
                0
            ));
    }
    /// <summary>
    /// Setting target for soldiers to the available tile.
    /// </summary>
    /// <param name="pos">Current position</param>
    /// <param name="targetPos">Target position</param>
    void SetTarget(Vector3Int pos, Vector3Int targetPos){
        node = InputManager.instance.gridMap.nodes[pos.x, pos.y];
        if (neighbours == null)
        {
            neighbours = new List<Node>();
        }
        InputManager.instance.gridMap.GetNeighbours(node, ref neighbours);
        if (neighbours.Count == 0)
            return;
        node = InputManager.instance.gridMap.nodes[targetPos.x, targetPos.y];
                       
        if(!node.isWalkable)
        {
            InputManager.instance.gridMap.GetValidNeighbour(ref node);
        }

        InputManager.instance.gridMap.nodes[pos.x, pos.y].isWalkable = true;
        InputManager.instance.tilemap.SetTile(pos, InputManager.instance.ground);

        targetPos = new Vector3Int(node.gridX, node.gridY, 0);
        PathFinding.FindPath(InputManager.instance.gridMap,
                                        pos,
                                        targetPos,
                                        ref pathList
                                       );
        

        if (pathList != null && pathList.Count > 0)
        {
            direction = pathList[0];
            InputManager.instance.gridMap.nodes[pathList[0].x, pathList[0].y].isWalkable = false;
            InputManager.instance.tilemap.SetTile(pathList[0], InputManager.instance.redGround);
            isRotating = true;
        }
        else
        {
            pathList = null;
            isReachedDestination = true;
        }
    }
    /// <summary>
    /// Movement for soldier. Traveling on pathlists.
    /// </summary>
    void Wander()
    {
        gridPos = InputManager.instance.grid.WorldToCell(myTransform.position);
        if (pathList == null)
        {
            SetTarget(gridPos, new Vector3Int(
                Mathf.RoundToInt(target.x),
                Mathf.RoundToInt(target.y),
                0
            ));
        }
        else
        {
            if ( Vector3.Distance(myTransform.position, direction) <= 0f)
            {
                if (pathList.Count > 1)
                {
                    node = InputManager.instance.gridMap.nodes[pathList[0].x, pathList[0].y];
                    if (neighbours == null)
                    {
                        neighbours = new List<Node>();
                    }
                    InputManager.instance.gridMap.GetNeighbours(node, ref neighbours);
                    if (neighbours.Count == 0)
                        return;
                    // Clear last passed tile to walkable
                    node.isWalkable = true;
                    InputManager.instance.tilemap.SetTile(pathList[0], InputManager.instance.ground);
                    pathList.RemoveAt(0);

                    node = InputManager.instance.gridMap.nodes[pathList[0].x, pathList[0].y];
                    if (node.isWalkable)
                    {
                        node.isWalkable = false;
                        InputManager.instance.tilemap.SetTile(pathList[0], InputManager.instance.redGround);
                        direction = pathList[0];
                    }
                    else
                        pathList = null;
                }
                else
                {
                    isReachedDestination = true;
                    return;
                }
            }// distance check
            myTransform.position = Vector2.MoveTowards(myTransform.position, direction, moveSpeed * Time.deltaTime);
        }

    }
}
