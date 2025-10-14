using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling
{
    private Queue<GameObject> pool;
    private GameObject prefab;
    private Transform parent;

    public ObjectPooling(GameObject prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        pool = new Queue<GameObject>();

        // Preload objects
        for (int i = 0; i < initialSize; i++)
            AddGameObjectoPool();
    }

    private void AddGameObjectoPool()
    {
        GameObject newObj = Object.Instantiate(prefab, parent);
        newObj.gameObject.SetActive(false);
        pool.Enqueue(newObj);
    }

    public GameObject GetFromPool()
    {
        if (pool.Count == 0)
            AddGameObjectoPool();

        GameObject obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(parent);
        pool.Enqueue(obj);
    }
}