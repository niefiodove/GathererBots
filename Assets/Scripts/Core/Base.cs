using UnityEngine;
using System;
using System.Collections.Generic;

public class Base : MonoBehaviour
{
    [SerializeField] private Transform _parkingPoint;
    [SerializeField] private Transform _lootCenter;
    [SerializeField] private Detector _detector;
    [SerializeField] private int _droneCost = 3;
    [SerializeField] private int _expansionCost = 5;
    [SerializeField] private DronSpawner _dronSpawner;
    [SerializeField] private GameObject _newBasePrefab;

    private List<Dron> _availableDrons = new List<Dron>();
    private Queue<Loot> _lootQueue = new Queue<Loot>();
    private HashSet<Loot> _trackedLoot = new HashSet<Loot>();
    private int _collectedResourcesCount = 0;
    private int _totalDronesCount = 0;
    private BaseFlag _flag;
    private bool _isExpansionPending = false;
    public Transform Center => _lootCenter != null ? _lootCenter : transform;
    public event Action<int> ResourceCountChanged;
    public bool HasFlag => _flag != null;
    public BaseFlag GetFlag() => _flag;
    public void SetFlag(BaseFlag flag)
    {
        _flag = flag;
    }

    public void ClearFlag()
    {
        _flag = null;
        _isExpansionPending = false;
    }

    public void RemoveDrone()
    {
        _totalDronesCount = Mathf.Max(0, _totalDronesCount - 1);
    }
    public DronSpawner GetDronSpawner() => _dronSpawner;

    private void Awake()
    {
        SceneContext context = FindFirstObjectByType<SceneContext>();
        if (context != null)
            context.RegisterBase(this);
    }

    private void Start()
    {
        if (_detector != null)
            _detector.LootFound += OnLootFound;
    }

    private void OnDestroy()
    {
        if (_detector != null)
            _detector.LootFound -= OnLootFound;

        SceneContext context = FindFirstObjectByType<SceneContext>();

        if (context != null)
            context.UnregisterBase(this);
    }

    public void RegisterDron(Dron dron)
    {
        dron.Initialize(this);

        if (!_availableDrons.Contains(dron))
            _availableDrons.Add(dron);

        dron.DroneReturned += OnDronReturned;
        _totalDronesCount++;
        TryAssignTasksToFreeDrones();
    }

    public void ResourceDelivered(Loot deliveredLoot)
    {
        if (deliveredLoot != null)
            _trackedLoot.Remove(deliveredLoot);

        _collectedResourcesCount++;

        if (HasFlag && !_isExpansionPending && _collectedResourcesCount >= _expansionCost && _totalDronesCount > 1)
        {
            _collectedResourcesCount -= _expansionCost;
            _isExpansionPending = true;
            TryAssignTasksToFreeDrones();
        }
        else if ((!HasFlag || _totalDronesCount <= 1) && _collectedResourcesCount >= _droneCost)
        {
            if (_dronSpawner != null && _dronSpawner.CanSpawnDrone())
            {
                _collectedResourcesCount -= _droneCost;
                _dronSpawner.SpawnNewDrone();
            }
        }

        ResourceCountChanged?.Invoke(_collectedResourcesCount);
    }

    private void OnLootFound(Loot loot)
    {
        if (_trackedLoot.Contains(loot))
            return;

        _trackedLoot.Add(loot);
        _lootQueue.Enqueue(loot);
        TryAssignTasksToFreeDrones();
    }

    private void OnDronReturned(Dron dron)
    {
        if (!_availableDrons.Contains(dron))
        {
            _availableDrons.Add(dron);
            TryAssignTasksToFreeDrones();
        }
    }

    private void TryAssignTasksToFreeDrones()
    {
        if (_isExpansionPending && _availableDrons.Count > 0 && _totalDronesCount > 1)
        {
            Dron builder = _availableDrons[0];
            _availableDrons.RemoveAt(0);
            _isExpansionPending = false;

            builder.GoToFlag(_flag, _newBasePrefab);
            return;
        }

        while (_lootQueue.Count > 0 && _availableDrons.Count > 0)
        {
            Loot nextLoot = _lootQueue.Dequeue();
            _trackedLoot.Remove(nextLoot);

            if (nextLoot == null)
                continue;

            Dron freeDron = _availableDrons[0];
            _availableDrons.RemoveAt(0);
            freeDron.GoToLoot(nextLoot);
        }
    }
}