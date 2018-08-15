using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    //Singleton
    public static UIManager instance;
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
        informationMenuBuilding = InformationHolder.transform.GetChild(0);
        informationMenuProduct = InformationHolder.transform.GetChild(1);
        LoadProduceMenu();
    }

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

            GameObject go = Instantiate(entityItemUI);
            go.transform.SetParent(producePanel.transform);
            produceMenuItems.Add(go);

            LoadBuilding(go, bEntity[i]);

            Buildings build = obj.GetComponent<Buildings>();
            if (entities.ContainsKey(build._name))
                continue;
            Entity[] ent = new Entity[build.products.Length];
            for (int j = 0; j < build.products.Length; j++)
            {
                ent[j] = build.products[j].GetComponent<Entity>();
            }
            entities.Add(build._name, ent);

        }
        entities.Add("Buildings", bEntity);
    }

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

    public void ShowBuildings()
    {
        Entity[] items = entities["Buildings"];

        for (int i = 0; i < produceMenuItems.Count; i++)
        {
            if (i >= items.Length)
            {
                produceMenuItems[i].SetActive(false);
            }
            else
            {
                LoadBuilding(produceMenuItems[i], items[i]);
                produceMenuItems[i].SetActive(true);
            }
        }

        producePanel.gameObject.SetActive(true);
        InformationHolder.gameObject.SetActive(false);
        showSelectedBuildingProduct = false;
    }

    public void ShowSelectedBuildingProduce(Entity building)
    {
        selectedBuilding = building;

        if (entities.ContainsKey(building._name))
        {
            Entity[] items = entities[building._name];

            if (produceMenuItems.Count < items.Length)
            {
                for (int i = 0; i < items.Length - produceMenuItems.Count; i++)
                {
                    GameObject go = Instantiate(entityItemUI);
                    go.transform.SetParent(producePanel.transform);
                    produceMenuItems.Add(go);
                }
            }

            for (int i = 0; i < produceMenuItems.Count; i++)
            {
                if (i >= items.Length)
                {
                    produceMenuItems[i].SetActive(false);
                }
                else
                {
                    LoadProduct(produceMenuItems[i], items[i]);
                }
            }

        }

        producePanel.gameObject.SetActive(true);
        SetContentInformation(building);

        showSelectedBuildingProduct = true;
    }

    public void SelectBuilding(GameObject building)
    {
        Entity build = building.GetComponent<Entity>();
        SetContentInformation(build);
        InputManager.instance.SelectedBuilding(build);
    }

    public void SelectProduct(GameObject product)
    {
        Entity build = product.GetComponent<Entity>();
        SetContentInformation(build);
        InputManager.instance.CreateSoldier(product);
    }

    void SetContentSizes()
    {
        float width = producePanel.rect.width;
        Vector2 newSize = new Vector2(width * 0.4f, width * 0.4f);
        group.cellSize = newSize;
        InformationHolder.cellSize = new Vector2(width, width);
        InformationHolder.gameObject.SetActive(false);
    }

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
