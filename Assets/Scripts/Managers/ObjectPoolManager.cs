using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    readonly Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    public void Initialize() { }

    public GameObject Get(string key, GameObject prefab, Transform parent = null)
    {
        EnsurePool(key);

        if (pools[key].Count > 0)
        {
            GameObject pooled = pools[key].Dequeue();
            pooled.transform.SetParent(parent);
            pooled.SetActive(true);
            return pooled;
        }

        return Instantiate(prefab, parent);
    }

    public void Return(string key, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        EnsurePool(key);
        pools[key].Enqueue(obj);
    }

    public void PreWarm(string key, GameObject prefab, int count, Transform parent = null)
    {
        EnsurePool(key);

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, parent != null ? parent : transform);
            obj.SetActive(false);
            pools[key].Enqueue(obj);
        }
    }

    void EnsurePool(string key)
    {
        if (!pools.ContainsKey(key))
            pools[key] = new Queue<GameObject>();
    }
}
