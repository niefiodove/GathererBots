using UnityEngine;
using System.Collections.Generic;

public abstract class Spawner<T> : MonoBehaviour where T : Component
{
    [SerializeField] protected T Prefab;
    [SerializeField] protected int InitialPoolSize = 5;
    [SerializeField] protected int MaxPoolSize = 10;

    protected Queue<T> ObjectPool = new Queue<T>();
    protected int TotalObjectsCreated = 0;

    protected virtual void Awake()
    {
        InitializePool();
    }

    public void ReturnToPool(T objectToReturn)
    {
        if (objectToReturn == null)
            return;

        if (objectToReturn.gameObject.activeSelf)
            objectToReturn.gameObject.SetActive(false);

        ObjectPool.Enqueue(objectToReturn);
    }

    protected void InitializePool()
    {
        int sizeToCreate = InitialPoolSize;

        if (MaxPoolSize > 0 && InitialPoolSize > MaxPoolSize)
            sizeToCreate = MaxPoolSize;

        for (int i = 0; i < sizeToCreate; i++)
        {
            T spawnedObject = InstantiateObject();
            spawnedObject.gameObject.SetActive(false);
            ObjectPool.Enqueue(spawnedObject);
            TotalObjectsCreated++;
        }
    }

    protected T GetFromPool()
    {
        if (ObjectPool.Count > 0)
        {
            return ObjectPool.Dequeue();
        }
        else
        {
            if (MaxPoolSize > 0 && TotalObjectsCreated >= MaxPoolSize)
                return null;

            TotalObjectsCreated++;
            return InstantiateObject();
        }
    }

    private T InstantiateObject()
    {
        return Instantiate(Prefab);
    }
}