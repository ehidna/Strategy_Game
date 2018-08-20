using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPoolItem
{
    public GameObject objectToPool;
    public string poolName;
    public int amountToPool;
    public bool shouldExpand = true;
}

public class ObjectPooler : MonoBehaviour
{
    public const string DefaultRootObjectPoolName = "Pooled Objects";

    public static ObjectPooler Instance;
    public string rootPoolName = DefaultRootObjectPoolName;
    public List<GameObject> pooledObjects;
    public List<ObjectPoolItem> itemsToPool;

    void Awake()
    {
        Instance = this;
        if (string.IsNullOrEmpty(rootPoolName))
            rootPoolName = DefaultRootObjectPoolName;

        GameObject root = new GameObject
        {
            name = rootPoolName
        };
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < itemsToPool.Count; i++)
        {
            for (int j = 0; j < itemsToPool[i].amountToPool; j++)
            {
                CreatePooledObject(itemsToPool[i]);
            }
        }
    }

    /// <summary>
    /// Get parent pool object for transform.parent ing 
    /// </summary>
    /// <param name="objectPoolName">Getting name inside of objects entity name parameter</param>
    private GameObject GetParentPoolObject(string objectPoolName)
    {
        objectPoolName = string.Format(objectPoolName + "Pool");
        
        GameObject parentObject = GameObject.Find(objectPoolName);

        // Create the parent object if necessary
        if (parentObject == null)
        {

            parentObject = new GameObject
            {
                name = objectPoolName
            };
            // Add sub pools to the root object pool if necessary
            if (objectPoolName != rootPoolName)
            {
                parentObject.transform.parent = GameObject.Find(rootPoolName).transform;
            }
        }

        return parentObject;
    }
    /// <summary>
    /// Get object if pool isn't empty, or Create new one 
    /// </summary>
    /// <param name="_name">Getting name inside of objects entity name parameter</param>
    public GameObject GetPooledObject(string _name)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy && pooledObjects[i].name == _name)
                return pooledObjects[i];
        }
        for (int j = 0; j < itemsToPool.Count; j++)
        {
            if (itemsToPool[j].objectToPool.name == _name)
            {
                if (itemsToPool[j].shouldExpand)
                {
                    return CreatePooledObject(itemsToPool[j]);
                }
            }
        }

        return null;
    }
    /// <summary>
    /// Create Pooled object and set parent
    /// </summary>
    /// <param name="item">Getting name inside of objects entity name parameter</param>
    public GameObject CreatePooledObject(ObjectPoolItem item)
    {
        GameObject obj = Instantiate<GameObject>(item.objectToPool);
        obj.name = item.poolName;
        // Get the parent for this pooled object and assign the new object to it
        var parentPoolObject = GetParentPoolObject(item.poolName);
        RectTransform ui = obj.GetComponent<RectTransform>();
        if(ui == null)
            obj.transform.parent = parentPoolObject.transform;
        
        obj.SetActive(false);
        pooledObjects.Add(obj);
        return obj;
    }
}