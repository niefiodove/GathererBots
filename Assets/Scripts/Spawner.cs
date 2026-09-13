using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject _prefabToSpawn;
    [SerializeField] private Transform _baseObject;
    [SerializeField] private BoxCollider _groundCube;
    [SerializeField] private float _spawnInterval = 2f;
    [SerializeField] private float _minDistanceFromBase = 1f;
    [SerializeField] private float _maxDistanceFromBase = 20f;
    [SerializeField] private float _spawnAreaPadding = 0.2f;
    [SerializeField] private int maxSpawnAttempts = 50;
    [SerializeField] private int _initialPoolSize = 5;
    [SerializeField] private int _maxPoolSize = 10;

    private Queue<GameObject> objectPool = new Queue<GameObject>();
    private int totalObjectsCreated = 0;

    private void Start()
    {
        InitializePool();
        StartCoroutine(SpawnLoop());
    }
    public void ReturnToPool(GameObject objToReturn)
    {
        if (objToReturn == null) return;

        if (objToReturn.activeSelf) objToReturn.SetActive(false);

        objectPool.Enqueue(objToReturn);
    }

    private void InitializePool()
    {
        int sizeToCreate = _initialPoolSize;
        if (_maxPoolSize > 0 && _initialPoolSize > _maxPoolSize)
        {
            sizeToCreate = _maxPoolSize;
        }
        for (int i = 0; i < sizeToCreate; i++)
        {
            GameObject obj = Instantiate(_prefabToSpawn);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
            totalObjectsCreated++;
        }
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(_spawnInterval);
        while (true)
        {
            SpawnObject();
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnObject()
    {
        Vector3 spawnPosition = GetValidSpawnPosition();
        if (spawnPosition != Vector3.zero)
        {
            GameObject objToSpawn = GetFromPool();
            if (objToSpawn != null)
            {
                objToSpawn.transform.position = spawnPosition;
                objToSpawn.transform.rotation = Quaternion.identity;

                Loot lootComponent = objToSpawn.GetComponent<Loot>();
                
                if (lootComponent != null)
                    lootComponent.Initialize(this);

                objToSpawn.SetActive(true);
            }
        }
    }

    private GameObject GetFromPool()
    {
        if (objectPool.Count > 0)
        {
            return objectPool.Dequeue();
        }
        else
        {
            if (_maxPoolSize > 0 && totalObjectsCreated >= _maxPoolSize)
                return null;

            totalObjectsCreated++;
            return Instantiate(_prefabToSpawn);
        }
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 randomPoint = GetRandomPointOnGround();
            float distanceToBase = Vector3.Distance(randomPoint, _baseObject.position);
            if (distanceToBase >= _minDistanceFromBase && distanceToBase <= _maxDistanceFromBase)
            {
                return randomPoint;
            }
        }
        return Vector3.zero;
    }

    private Vector3 GetRandomPointOnGround()
    {
        Bounds bounds = _groundCube.bounds;
        float randomX = Random.Range(bounds.min.x - _spawnAreaPadding, bounds.max.x - _spawnAreaPadding);
        float randomZ = Random.Range(bounds.min.z - _spawnAreaPadding, bounds.max.z - _spawnAreaPadding);
        float groundY = bounds.max.y;
        return new Vector3(randomX, groundY, randomZ);
    }
}