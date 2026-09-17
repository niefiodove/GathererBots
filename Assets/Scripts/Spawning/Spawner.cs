using UnityEngine;
using System.Collections.Generic;

public abstract class Spawner<T> : MonoBehaviour where T : Component
{
    [SerializeField] protected T _prefab;
    [SerializeField] protected int _initialPoolSize = 5;
    [SerializeField] protected int _maxPoolSize = 10;

    protected Queue<T> _objectPool = new Queue<T>();
    protected int _totalObjectsCreated = 0;

    protected virtual void Awake()
    {
        InitializePool();
    }

    public void ReturnToPool(T objToReturn)
    {
        if (objToReturn == null)
            return;

        if (objToReturn.gameObject.activeSelf)
            objToReturn.gameObject.SetActive(false);

        _objectPool.Enqueue(objToReturn);
    }

    protected void InitializePool()
    {
        int sizeToCreate = _initialPoolSize;
        if (_maxPoolSize > 0 && _initialPoolSize > _maxPoolSize)
            sizeToCreate = _maxPoolSize;

        for (int i = 0; i < sizeToCreate; i++)
        {
            T obj = InstantiateObject();
            obj.gameObject.SetActive(false);
            _objectPool.Enqueue(obj);
            _totalObjectsCreated++;
        }
    }

    protected T GetFromPool()
    {
        if (_objectPool.Count > 0)
        {
            return _objectPool.Dequeue();
        }
        else
        {
            if (_maxPoolSize > 0 && _totalObjectsCreated >= _maxPoolSize)
                return null;

            _totalObjectsCreated++;
            return InstantiateObject();
        }
    }

    private T InstantiateObject()
    {
        return Instantiate(_prefab);
    }
}