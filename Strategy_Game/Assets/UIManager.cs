using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private const string strBuild = "Buildings";

    //Singleton
    public static UIManager instance;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    public bool showSelectedBuildingProduct;

    private Dictionary<string, Entity[]> entities;
    private List<GameObject> produceMenuItems;
    public Entity[] buildings;
    private Entity selectedBuilding;

    public GameObject entityItemUI;
    public RectTransform producePanel;
    public GridLayoutGroup InformationHolder;
    GridLayoutGroup group;

    private Transform informationMenuBuilding;
    private Transform informationMenuProduct;


    // Use this for initialization
    void Start()
    {
        ObjectPoolItem ObjectPoolItem = new ObjectPoolItem
        {
            objectToPool = entityItemUI,
            poolName = entityItemUI.name,
            amountToPool = 20,
            shouldExpand = true
        };
        for (int i = 0; i < ObjectPoolItem.amountToPool; i++)
        {
            ObjectPooler.Instance.CreatePooledObject(ObjectPoolItem);
            //obj.transform.SetParent(producePanel.transform);
        }

        ObjectPooler.Instance.itemsToPool.Add(ObjectPoolItem);

        informationMenuBuilding = InformationHolder.transform.GetChild(0);
        informationMenuProduct = InformationHolder.transform.GetChild(1);
        LoadProduceMenu();


    }
    /// <summary>
    /// First initializeing  Produce menu with buildings object
    /// </summary>
    void LoadProduceMenu()
    {
        entities = new Dictionary<string, Entity[]>();
        produceMenuItems = new List<GameObject>();

        //Get all building entities
        Entity[] bEntity = new Entity[buildings.Length];

        group = producePanel.GetComponent<GridLayoutGroup>();
        SetContentSizes();
        for (int i = 0; i < buildings.Length; i++)
        {
            GameObject obj = buildings[i].gameObject;
            bEntity[i] = obj.GetComponent<Entity>();

            GameObject go = ObjectPooler.Instance.GetPooledObject(entityItemUI.name);
            go.transform.SetParent(producePanel.transform);
            produceMenuItems.Add(go);
            go.SetActive(true);
            LoadBuilding(go, bEntity[i]);

            Buildings build = obj.GetComponent<Buildings>();
            if (entities.ContainsKey(build._name))
                continue;
            Entity[] ent = new Entity[build.products.Length];
            for (int j = 0; j < build.products.Length; j++)
                ent[j] = build.products[j].GetComponent<Entity>();
            entities.Add(build._name, ent);

        }
        entities.Add(strBuild, bEntity);
    }
    /// <summary>
    /// Setting parameters of button and adding click event for building
    /// </summary>
    /// <param name="uiObj">Ui button element</param>
    /// <param name="entity">Getting entity of object</param>
    void LoadBuilding(GameObject uiObj, Entity entity)
    {
        Button button = uiObj.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate
        {
            SelectBuilding(entity.gameObject);
        });

        uiObj.GetComponentInChildren<Image>().sprite = entity.sprite;
        uiObj.GetComponentInChildren<Text>().text = entity._name;
    }
    /// <summary>
    /// Setting parameters of button and adding click event for soldiers or products
    /// </summary>
    /// <param name="uiObj">Ui button element</param>
    /// <param name="entity">Getting entity of object</param>
    void LoadProduct(GameObject uiObj, Entity entity)
    {
        Button button = uiObj.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate
        {
            SelectProduct(entity.gameObject);
        });

        uiObj.GetComponentInChildren<Image>().sprite = entity.sprite;
        uiObj.GetComponentInChildren<Text>().text = entity._name;
    }
    /// <summary>
    /// Showing buildings on menu
    /// </summary>
    public void ShowBuildings()
    {
        if (selectedBuilding == null)
            return;
        Entity[] items = entities[strBuild];

        for (int i = 0; i < produceMenuItems.Count; i++)
            if (i >= items.Length)
                produceMenuItems[i].SetActive(false);
            else
            {
                LoadBuilding(produceMenuItems[i], items[i]);
                produceMenuItems[i].SetActive(true);
            }

        producePanel.gameObject.SetActive(true);
        InformationHolder.gameObject.SetActive(false);
        showSelectedBuildingProduct = false;
        selectedBuilding = null;
    }
    /// <summary>
    /// Showing selected building products
    /// </summary>
    /// <param name="building">Getting entity of object</param>
    public void ShowSelectedBuildingProduce(Entity building)
    {
        InputManager.instance.CheckFlag(building);
        if(selectedBuilding != null && string.Equals(selectedBuilding._name, building._name))
        {
            selectedBuilding = building;
            return;
        }
        selectedBuilding = building;

        if (entities.ContainsKey(building._name))
        {
            Entity[] items = entities[building._name];
            if (produceMenuItems.Count < items.Length)
                for (int i = 0; i < items.Length - produceMenuItems.Count; i++)
                {
                    GameObject go = ObjectPooler.Instance.GetPooledObject(entityItemUI.name);
                    go.transform.SetParent(producePanel.transform);
                    produceMenuItems.Add(go);
                }

            if (items.Length > 0)
            {
                for (int i = 0; i < produceMenuItems.Count; i++)
                    if (i >= items.Length)
                        produceMenuItems[i].SetActive(false);
                    else
                    {
                        produceMenuItems[i].SetActive(true);
                        LoadProduct(produceMenuItems[i], items[i]);
                    }

                producePanel.gameObject.SetActive(true);
                SetContentInformation(building);
                showSelectedBuildingProduct = true;
            }
            else
            {
                producePanel.gameObject.SetActive(false);
                showSelectedBuildingProduct = false;
                SetContentInformation(building);
            }
        }

    }
    /// <summary>
    /// Button click event for buildings
    /// </summary>
    /// <param name="building">Building object</param>
    public void SelectBuilding(GameObject building)
    {
        Entity build = building.GetComponent<Entity>();
        SetContentInformation(build);
        InputManager.instance.SelectedBuilding(build);
    }

    /// <summary>
    /// Button click event for products
    /// </summary>
    /// <param name="product">Product object</param>
    public void SelectProduct(GameObject product)
    {
        Entity build = product.GetComponent<Entity>();
        SetContentInformation(build);
        InputManager.instance.CreateSoldier(product);
    }
    /// <summary>
    /// Setting content sizes to different aspects. Main is 1080, 720.
    /// </summary>
    void SetContentSizes()
    {
        float width = producePanel.rect.width;
        Vector2 newSize = new Vector2(width * 0.4f, width * 0.4f);
        group.cellSize = newSize;
        InformationHolder.cellSize = new Vector2(width, width);
        InformationHolder.gameObject.SetActive(false);
    }
    /// <summary>
    /// Setting parameters of information menu
    /// </summary>
    /// <param name="item">Getting entity of object</param>
    void SetContentInformation(Entity item)
    {
        if (showSelectedBuildingProduct)
        {
            informationMenuProduct.GetComponentInChildren<Text>().text = item._name;
            informationMenuProduct.GetComponentInChildren<Image>().sprite = item.sprite;
            informationMenuProduct.gameObject.SetActive(true);
        }
        else
        {
            informationMenuProduct.gameObject.SetActive(false);
            GameObject buildingInformation = informationMenuBuilding.gameObject;
            Image buildingImage = buildingInformation.GetComponentInChildren<Image>();
            buildingImage.sprite = item.sprite;
            buildingInformation.GetComponentInChildren<Text>().text = item._name;
        }

        InformationHolder.gameObject.SetActive(true);
    }
}
