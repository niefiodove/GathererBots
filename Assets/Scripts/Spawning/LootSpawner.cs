using UnityEngine;
using System.Collections;

public class LootSpawner : Spawner<Loot>
{
    [SerializeField] private Transform _baseObject;
    [SerializeField] private BoxCollider _groundCube;
    [SerializeField] private float _spawnInterval = 2f;
    [SerializeField] private float _minDistanceFromBase = 1f;
    [SerializeField] private float _maxDistanceFromBase = 20f;
    [SerializeField] private float _spawnAreaPadding = 0.2f;
    [SerializeField] private int _maxSpawnAttempts = 50;

    private WaitForSeconds _spawnTime;

    private void Start()
    {
        _spawnTime = new WaitForSeconds(_spawnInterval);
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return _spawnTime;

        while (true)
        {
            SpawnObject();
            yield return _spawnTime;
        }
    }

    private void SpawnObject()
    {
        Vector3 spawnPosition = GetValidSpawnPosition();

        if (spawnPosition != Vector3.zero)
        {
            Loot loot = GetFromPool();

            if (loot != null)
            {
                loot.transform.position = spawnPosition;
                loot.transform.rotation = Quaternion.identity;
                loot.OnDeliveryFinished += HandleDeliveryFinished;
                loot.gameObject.SetActive(true);
            }
        }
    }

    private void HandleDeliveryFinished(Loot loot)
    {
        loot.OnDeliveryFinished -= HandleDeliveryFinished;
        loot.Deactivate();
        ReturnToPool(loot);
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int i = 0; i < _maxSpawnAttempts; i++)
        {
            Vector3 randomPoint = GetRandomPointOnGround();
            float distanceToBase = Vector3.Distance(randomPoint, _baseObject.position);
            
            if (distanceToBase >= _minDistanceFromBase && distanceToBase <= _maxDistanceFromBase)
                return randomPoint;
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