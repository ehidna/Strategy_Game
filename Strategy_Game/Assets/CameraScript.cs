using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{

    #region Foldouts

#if UNITY_EDITOR

    public int lastTab = 0;

    public bool movementSettingsFoldout;
    public bool mapLimitSettingsFoldout;
    public bool inputSettingsFoldout;

#endif

    #endregion

    private Transform m_Transform; //camera tranform
    public bool useFixedUpdate = false; //use FixedUpdate() or Update()

    #region Movement

    public float keyboardMovementSpeed = 5f; //speed with keyboard movement
    public float screenEdgeMovementSpeed = 3f; //spee with screen edge movement

    #endregion

    #region MapLimits

    public bool limitMap = true;
    public float limitX = 50f; //x limit of map
    public float limitY = 50f; //z limit of map

    #endregion


    #region Input

    public bool useScreenEdgeInput = true;
    public float screenEdgeBorder = 25f;

    public bool useKeyboardInput = true;
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";

    public bool useKeyboardZooming = true;

    private Vector2 KeyboardInput
    {
        get { return useKeyboardInput ? new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis)) : Vector2.zero; }
    }

    private Vector2 MouseInput
    {
        get { return Input.mousePosition; }
    }

    private Vector2 MouseAxis
    {
        get { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); }
    }

    #endregion

    #region Unity_Methods

    private void Start()
    {
        m_Transform = transform;
    }

    private void Update()
    {
        if (!useFixedUpdate)
            CameraUpdate();
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate)
            CameraUpdate();
    }

    #endregion

    #region RTSCamera_Methods

    /// <summary>
    /// update camera movement and rotation
    /// </summary>
    private void CameraUpdate()
    {
        Move();
        LimitPosition();
    }

    /// <summary>
    /// move camera with keyboard or with screen edge
    /// </summary>
    private void Move()
    {
        if (useKeyboardInput)
        {
            Vector3 desiredMove = new Vector3(KeyboardInput.x, KeyboardInput.y, 0f);

            desiredMove *= keyboardMovementSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0f, 0f, transform.eulerAngles.y)) * desiredMove;
            desiredMove = m_Transform.InverseTransformDirection(desiredMove);

            m_Transform.Translate(desiredMove, Space.Self);
        }

        if (useScreenEdgeInput)
        {
            Vector3 desiredMove = new Vector3();

            Rect leftRect = new Rect(Screen.width * 0.25f, 0, screenEdgeBorder, Screen.height);
            Rect rightRect = new Rect(Screen.width * 0.75f - screenEdgeBorder, 0, screenEdgeBorder, Screen.height);
            Rect upRect = new Rect(0, Screen.height - screenEdgeBorder, Screen.width, screenEdgeBorder);
            Rect downRect = new Rect(0, 0, Screen.width, screenEdgeBorder);

            desiredMove.x = leftRect.Contains(MouseInput) ? -1 : rightRect.Contains(MouseInput) ? 1 : 0;
            desiredMove.y = upRect.Contains(MouseInput) ? 1 : downRect.Contains(MouseInput) ? -1 : 0;

            desiredMove *= screenEdgeMovementSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;
            desiredMove = m_Transform.InverseTransformDirection(desiredMove);

            m_Transform.Translate(desiredMove, Space.Self);
        }
    }

    /// <summary>
    /// limit camera position
    /// </summary>
    private void LimitPosition()
    {
        if (!limitMap)
            return;

        m_Transform.position = new Vector3(Mathf.Clamp(m_Transform.position.x, 0, limitX),
                                           Mathf.Clamp(m_Transform.position.y, 0, limitY),
                                           m_Transform.position.z);
    }

    #endregion
}