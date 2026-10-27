using UnityEngine;
using System;

public class ExpansionHandler : MonoBehaviour
{
    [SerializeField] private int _expansionCost = 5;
    [SerializeField] private int _droneCost = 3;
    [SerializeField] private int _minDronesToKeep = 1;

    private BaseFlag _flag;
    private int _collectedResources = 0;
    private int _totalDronesCount = 0;
    private int _availableDronesCount = 0;
    private bool _isExpansionPending = false;

    public event Action<BaseFlag> ExpansionReady;
    public event Action<int> ResourceCountChanged;

    public int CollectedResources => _collectedResources;
    public bool HasFlag => _flag != null;
    public bool IsExpansionPending => _isExpansionPending;
    public BaseFlag GetFlag() => _flag;

    public bool ShouldPauseDroneProduction()
    {
        return HasFlag && _totalDronesCount > _minDronesToKeep;
    }

    public bool CanAffordDrone() => _collectedResources >= _droneCost;
    public int GetDroneCost() => _droneCost;

    public void SetFlag(BaseFlag flag)
    {
        _flag = flag;
        TryStartExpansion();
    }

    public void NotifyBuildCompleted()
    {
        _flag = null;
        _isExpansionPending = false;
    }

    public void NotifyResourceCollected()
    {
        _collectedResources++;
        ResourceCountChanged?.Invoke(_collectedResources);
        TryStartExpansion();
    }

    public bool ConsumeExpansionResources()
    {
        if (_collectedResources < _expansionCost)
            return false;

        _collectedResources -= _expansionCost;
        ResourceCountChanged?.Invoke(_collectedResources);
        return true;
    }

    public bool ConsumeDroneProductionResources()
    {
        if (_collectedResources < _droneCost)
            return false;

        _collectedResources -= _droneCost;
        ResourceCountChanged?.Invoke(_collectedResources);
        return true;
    }

    public void NotifyDroneRegistered() => _totalDronesCount++;
    public void NotifyDroneRemoved() => _totalDronesCount = Mathf.Max(0, _totalDronesCount - 1);

    public void NotifyDroneAvailable()
    {
        _availableDronesCount++;
        TryStartExpansion();
    }

    public void NotifyDroneBusy() => _availableDronesCount = Mathf.Max(0, _availableDronesCount - 1);

    private void TryStartExpansion()
    {
        if (_flag == null)
            return;

        if (_isExpansionPending)
            return;

        if (_collectedResources < _expansionCost)
            return;

        if (_totalDronesCount <= _minDronesToKeep)
            return;

        if (_availableDronesCount <= 0) 
            return;

        _isExpansionPending = true;
        ExpansionReady?.Invoke(_flag);
    }
}