using UnityEngine;
using System;
using System.Collections.Generic;

public class Base : MonoBehaviour
{
    [SerializeField] private Transform _lootCenter;
    [SerializeField] private Detector _detector;
    [SerializeField] private DronSpawner _dronSpawner;
    [SerializeField] private Base _newBasePrefab;
    [SerializeField] private float _deliveryAnimationDuration = 2f;
    [SerializeField] private ExpansionHandler _expansionHandler;

    private List<Dron> _availableDrons = new List<Dron>();
    private Queue<Loot> _lootQueue = new Queue<Loot>();
    private HashSet<Loot> _trackedLoot = new HashSet<Loot>();

    public Transform Center => _lootCenter != null ? _lootCenter : transform;
    public ExpansionHandler ExpansionHandler => _expansionHandler;
    public DronSpawner GetDronSpawner() => _dronSpawner;

    public event Action<int> ResourceCountChanged;

    private void Awake()
    {
        if (_expansionHandler == null)
            _expansionHandler = GetComponent<ExpansionHandler>();

        SceneContext context = FindFirstObjectByType<SceneContext>();

        if (context != null)
            context.RegisterBase(this);
    }

    private void Start()
    {
        if (_detector != null)
            _detector.LootFound += OnLootFound;

        if (_expansionHandler != null)
        {
            _expansionHandler.ExpansionReady += OnExpansionReady;
            _expansionHandler.ResourceCountChanged += OnResourceCountChanged;
        }
    }

    private void OnDestroy()
    {
        if (_detector != null)
            _detector.LootFound -= OnLootFound;

        if (_expansionHandler != null)
        {
            _expansionHandler.ExpansionReady -= OnExpansionReady;
            _expansionHandler.ResourceCountChanged -= OnResourceCountChanged;
        }

        SceneContext context = FindFirstObjectByType<SceneContext>();

        if (context != null)
            context.UnregisterBase(this);
    }

    private void OnResourceCountChanged(int count)
    {
        ResourceCountChanged?.Invoke(count);
    }

    public void RegisterDron(Dron dron)
    {
        if (!_availableDrons.Contains(dron))
            _availableDrons.Add(dron);

        dron.DeliveryReady += OnDronDeliveryReady;
        dron.BuildCompleted += OnDronBuildCompleted;
        dron.DroneReturned += OnDronReturned;

        _expansionHandler?.NotifyDroneRegistered();
        _expansionHandler?.NotifyDroneAvailable();
        TryAssignTasksToFreeDrones();
    }

    private void UnregisterDron(Dron dron)
    {
        dron.DeliveryReady -= OnDronDeliveryReady;
        dron.BuildCompleted -= OnDronBuildCompleted;
        dron.DroneReturned -= OnDronReturned;
        _availableDrons.Remove(dron);
        _expansionHandler?.NotifyDroneRemoved();
    }

    private void OnDronDeliveryReady(Dron dron, Loot loot)
    {
        if (loot == null)
            return;

        Loot receivedLoot = dron.HandOverLoot();

        if (receivedLoot != null)
        {
            _trackedLoot.Remove(receivedLoot);

            _expansionHandler?.NotifyResourceCollected();

            if (receivedLoot.gameObject.activeInHierarchy)
                receivedLoot.StartDeliveryAnimation(Center, _deliveryAnimationDuration);

            TrySpawnDroneIfNeeded();
        }
    }

    private void OnDronBuildCompleted(Dron dron)
    {
        if (_expansionHandler == null || !_expansionHandler.HasFlag)
            return;

        BaseFlag flag = _expansionHandler.GetFlag();
        Transform oldSpawnPoint = dron.GetSpawnPoint();
        GameObject flagObjectToDestroy = flag.gameObject;
        Vector3 flagPosition = flag.transform.position;

        Base newBase = Instantiate(_newBasePrefab, flagPosition, Quaternion.identity);
        DronSpawner newSpawner = newBase.GetDronSpawner();

        if (_dronSpawner != null && oldSpawnPoint != null)
            _dronSpawner.ReleaseParking(oldSpawnPoint);

        if (newSpawner != null)
        {
            newSpawner.SkipInitialSpawn();
            newSpawner.AssignParkingToExistingDrone(dron);
        }

        UnregisterDron(dron);
        newBase.RegisterDron(dron);

        _expansionHandler.NotifyBuildCompleted();
        Destroy(flagObjectToDestroy);
    }

    private void OnExpansionReady(BaseFlag flag)
    {
        if (!_expansionHandler.ConsumeExpansionResources())
        {
            _expansionHandler.NotifyBuildCompleted();
            return;
        }

        if (_availableDrons.Count > 0)
        {
            Dron builder = _availableDrons[0];
            _availableDrons.RemoveAt(0);
            _expansionHandler.NotifyDroneBusy();
            builder.GoToBuildTarget(flag.transform);
        }
    }

    private void OnDronReturned(Dron dron)
    {
        if (!_availableDrons.Contains(dron))
        {
            _availableDrons.Add(dron);
            _expansionHandler?.NotifyDroneAvailable();
            TryAssignTasksToFreeDrones();
        }
    }

    private void TrySpawnDroneIfNeeded()
    {
        if (_expansionHandler == null)
            return;

        if (_expansionHandler.ShouldPauseDroneProduction())
            return;

        if (!_expansionHandler.CanAffordDrone())
            return;

        if (_dronSpawner != null && _dronSpawner.CanSpawnDrone())
        {
            if (_expansionHandler.ConsumeDroneProductionResources())
                _dronSpawner.SpawnNewDrone();
        }
    }

    private void OnLootFound(Loot loot)
    {
        if (_trackedLoot.Contains(loot))
            return;

        _trackedLoot.Add(loot);
        _lootQueue.Enqueue(loot);
        TryAssignTasksToFreeDrones();
    }

    private void TryAssignTasksToFreeDrones()
    {
        while (_lootQueue.Count > 0 && _availableDrons.Count > 0)
        {
            Loot nextLoot = _lootQueue.Dequeue();
            _trackedLoot.Remove(nextLoot);

            if (nextLoot == null)
                continue;

            Dron freeDron = _availableDrons[0];
            _availableDrons.RemoveAt(0);
            _expansionHandler?.NotifyDroneBusy();
            freeDron.GoToLoot(nextLoot);
        }
    }
}