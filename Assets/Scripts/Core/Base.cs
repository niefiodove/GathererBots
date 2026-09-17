using UnityEngine;
using System;
using System.Collections.Generic;

public class Base : MonoBehaviour
{
    [SerializeField] private Transform _parkingPoint;
    [SerializeField] private Transform _lootCenter;
    [SerializeField] private Detector _detector;

    private List<Dron> _availableDrons = new List<Dron>();
    private Queue<Loot> _lootQueue = new Queue<Loot>();
    private HashSet<Loot> _trackedLoot = new HashSet<Loot>();
    private int _collectedResourcesCount = 0;

    public Transform Center => _lootCenter != null ? _lootCenter : transform;

    public event Action<int> ResourceCountChanged;

    private void Start()
    {
        if (_detector != null)
            _detector.LootFound += OnLootFound;
    }

    private void OnDestroy()
    {
        if (_detector != null)
            _detector.LootFound -= OnLootFound;
    }

    public void RegisterDron(Dron dron)
    {
        dron.Initialize(this);
        if (!_availableDrons.Contains(dron)) _availableDrons.Add(dron);
        
        dron.DroneReturned += OnDronReturned;
    }

    public void ResourceDelivered(Loot deliveredLoot)
    {
        if (deliveredLoot != null)
            _trackedLoot.Remove(deliveredLoot);
            
        _collectedResourcesCount++;
        ResourceCountChanged?.Invoke(_collectedResourcesCount);
    }

    private void OnLootFound(Loot loot)
    {
        if (_trackedLoot.Contains(loot)) return;
        _trackedLoot.Add(loot);
        _lootQueue.Enqueue(loot);
        TryAssignLootToFreeDrones();
    }

    private void OnDronReturned(Dron dron)
    {
        if (!_availableDrons.Contains(dron))
        {
            _availableDrons.Add(dron);
            TryAssignLootToFreeDrones();
        }
    }

    private void TryAssignLootToFreeDrones()
    {
        while (_lootQueue.Count > 0 && _availableDrons.Count > 0)
        {
            Loot nextLoot = _lootQueue.Dequeue();
            _trackedLoot.Remove(nextLoot);
            if (nextLoot == null) continue;
            
            Dron freeDron = _availableDrons[0];
            _availableDrons.RemoveAt(0);
            freeDron.GoToLoot(nextLoot);
        }
    }
}