using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PoolManager : GameSingleton<PoolManager>
{
    [SerializeField] private List<Pool> poolList = new List<Pool>();

    private Dictionary<PoolKey, ObjectPooling> poolDict = new Dictionary<PoolKey, ObjectPooling>();

    public static event System.Action onFinishedCreatingPool;

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < poolList.Count; i++)
        {
            if (poolList[i].prefab == null)
                continue;

            CreatePool(poolList[i].poolKey, poolList[i].prefab, poolList[i].quantity);
        }

        onFinishedCreatingPool?.Invoke();
    }

    void CreatePool(PoolKey key, GameObject prefab, int initialSize)
    {
        if (!poolDict.ContainsKey(key))
        {
            Transform poolParent = new GameObject(key.ToString() + "Group").GetComponent<Transform>();
            poolParent.SetParent(this.transform);

            var pool = new ObjectPooling(prefab, initialSize, poolParent);
            poolDict.Add(key, pool);
        }
    }

    public GameObject GetFromPool(PoolKey key)
    {
        if (poolDict.TryGetValue(key, out var pool))
            return ((ObjectPooling)pool).GetFromPool();

        Debug.LogError($"No pool with key {key} exists!");
        return null;
    }

    public void ReturnToPool(PoolKey key, GameObject obj)
    {
        if (poolDict.TryGetValue(key, out var pool))
            ((ObjectPooling)pool).ReturnToPool(obj);
        else
            Debug.LogError($"No pool with key {key} exists!");
    }
}

public enum PoolKey
{
   enemy,
   damagePopup
}

[System.Serializable]
public struct Pool
{
    [field: SerializeField] public PoolKey poolKey;
    [field: SerializeField] public GameObject prefab;
    [field: SerializeField] public int quantity;
}