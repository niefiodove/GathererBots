using UnityEngine;
using UnityEngine.AI;
using System;

public class Dron : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _lootStoppingDistance = 3f;
    [SerializeField] private float _approachOffset = 2f;
    [SerializeField] private float _parkingEpsilon = 0.1f;
    [SerializeField] private float _deliveryDuration = 2f;
    [SerializeField] private Vector3 _capturedLootPosition = new Vector3(0, 0, -1.3f);
    [SerializeField] private MagneticAttractor _magnet;
    [SerializeField] private float _buildDuration = 3f;

    private const float StoppedVelocitySqrThreshold = 0.01f;
    private const float ReachedLootDistanceSqrThreshold = 0.01f;
    private const float RotationSnapThresholdDefrees = 2f;
    private const float ApproachStoppingDistance = 0.5f;

    private DronState _currentState = DronState.Idle;
    private NavMeshAgent _navMeshAgent;
    private Base _homeBase;
    private Loot _carryingLoot;
    private Transform _spawnPoint;
    private Vector3 _approachPoint;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private BaseFlag _targetFlag;
    private GameObject _newBasePrefab;
    private float _buildTimer;

    public event Action<Dron> DroneReturned;

    public void Initialize(Base baseRef) => _homeBase = baseRef;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        if (_navMeshAgent != null)
            _navMeshAgent.speed = _speed;

        if (_magnet == null)
            _magnet = GetComponentInChildren<MagneticAttractor>();

        _initialPosition = transform.position;
        _initialRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }

    private void Update()
    {
        switch (_currentState)
        {
            case DronState.Idle: break;
            case DronState.MovingToLoot: UpdateMovingToLoot(); break;
            case DronState.TurningAtLoot: UpdateTurningAtLoot(); break;
            case DronState.ReturningToBase: UpdateReturningToBase(); break;
            case DronState.Parking: UpdateParking(); break;
            case DronState.TurningInPlace: UpdateTurningInPlace(); break;
            case DronState.Delivering: break;
            case DronState.MovingToFlag: UpdateMovingToFlag(); break;
            case DronState.BuildingBase: UpdateBuildingBase(); break;
        }
    }

    public void SetSpawnPoint(Transform spawnPoint)
    {
        _spawnPoint = spawnPoint;
        _approachPoint = spawnPoint.TransformPoint(new Vector3(0, 0, _approachOffset));
        _initialPosition = spawnPoint.position;
        _initialRotation = Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);
    }

    public void GoToLoot(Loot loot)
    {
        if (_navMeshAgent == null || loot == null)
            return;

        _carryingLoot = loot;
        _navMeshAgent.stoppingDistance = _lootStoppingDistance;
        _navMeshAgent.SetDestination(loot.transform.position);
        _navMeshAgent.enabled = true;
        _navMeshAgent.isStopped = false;
        _currentState = DronState.MovingToLoot;
    }

    public void GoToFlag(BaseFlag flag, GameObject newBasePrefab)
    {
        if (_navMeshAgent == null || flag == null)
            return;

        _targetFlag = flag;
        _newBasePrefab = newBasePrefab;

        _navMeshAgent.stoppingDistance = 0.5f;
        _navMeshAgent.SetDestination(flag.transform.position);
        _navMeshAgent.enabled = true;
        _navMeshAgent.isStopped = false;
        _currentState = DronState.MovingToFlag;
    }

    private void UpdateMovingToFlag()
    {
        if (_targetFlag == null)
        {
            _currentState = DronState.Idle;
            DroneReturned?.Invoke(this);
            return;
        }

        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance &&
            _navMeshAgent.velocity.sqrMagnitude < StoppedVelocitySqrThreshold)
        {
            _navMeshAgent.isStopped = true;
            _currentState = DronState.BuildingBase;
            _buildTimer = _buildDuration;
        }
    }

    private void UpdateBuildingBase()
    {
        _buildTimer -= Time.deltaTime;

        if (_buildTimer <= 0f)
            BuildBase();
    }

    private void BuildBase()
    {
        Base oldBase = _homeBase;

        GameObject newBaseObject = Instantiate(_newBasePrefab, _targetFlag.transform.position, Quaternion.identity);
        Base newBase = newBaseObject.GetComponent<Base>();
        DronSpawner newSpawner = newBase.GetDronSpawner();

        if (newSpawner != null)
        {
            newSpawner.SkipInitialSpawn();
            newSpawner.AssignParkingToExistingDrone(this);
        }

        newBase.RegisterDron(this);
        oldBase.RemoveDrone();
        oldBase.ClearFlag();
        Destroy(_targetFlag.gameObject);
        _navMeshAgent.isStopped = false;
        _currentState = DronState.Idle;
    }

    private void UpdateMovingToLoot()
    {
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && _navMeshAgent.velocity.sqrMagnitude < StoppedVelocitySqrThreshold)
            _currentState = DronState.TurningAtLoot;
    }

    private void UpdateTurningAtLoot()
    {
        Vector3 directionAwayFromLoot = (transform.position - _carryingLoot.transform.position);

        if (directionAwayFromLoot.sqrMagnitude < ReachedLootDistanceSqrThreshold)
        {
            PickupLoot();
            ReturnToSpawnPoint();
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionAwayFromLoot.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
        
        if (Quaternion.Angle(transform.rotation, targetRotation) < RotationSnapThresholdDefrees)
        {
            transform.rotation = targetRotation;
            PickupLoot();
            ReturnToSpawnPoint();
        }
    }

    private void UpdateReturningToBase()
    {
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && _navMeshAgent.velocity.sqrMagnitude < StoppedVelocitySqrThreshold)
        {
            _navMeshAgent.stoppingDistance = _parkingEpsilon;
            _navMeshAgent.SetDestination(_initialPosition);
            _currentState = DronState.Parking;
        }
    }

    private void UpdateParking()
    {
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && _navMeshAgent.velocity.sqrMagnitude < 0.01f)
        {
            _navMeshAgent.enabled = false;
            _currentState = DronState.TurningInPlace;
        }
    }

    private void UpdateTurningInPlace()
    {
        if (Quaternion.Angle(transform.rotation, _initialRotation) < RotationSnapThresholdDefrees)
        {
            transform.rotation = _initialRotation;
            transform.position = _initialPosition;
            _currentState = DronState.Delivering;
            StartDelivery();
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _initialRotation, Time.deltaTime * _rotationSpeed);
        }

        transform.position = new Vector3(transform.position.x, _initialPosition.y, transform.position.z);
    }

    private void PickupLoot()
    {
        if (_carryingLoot != null)
        {
            _carryingLoot.transform.SetParent(transform);
            _carryingLoot.transform.localPosition = _capturedLootPosition;
            _carryingLoot.transform.localRotation = Quaternion.identity;
            _carryingLoot.PrepareForPickup();
            _carryingLoot.DeliveryFinished += CompleteDelivery;
            _magnet.Activate();
            _magnet.SetAttractMode(true);
        }
    }

    private void ReturnToSpawnPoint()
    {
        if (_spawnPoint == null)
            return;

        _navMeshAgent.stoppingDistance = ApproachStoppingDistance;
        _navMeshAgent.SetDestination(_approachPoint);
        _navMeshAgent.isStopped = false;
        _navMeshAgent.enabled = true;
        _currentState = DronState.ReturningToBase;
    }

    private void StartDelivery()
    {
        if (_carryingLoot != null)
        {
            _magnet.SetAttractMode(false);
            _homeBase.ResourceDelivered(_carryingLoot);
            _carryingLoot.transform.SetParent(_homeBase.transform);
            _carryingLoot.StartDeliveryAnimation(_homeBase.Center, _deliveryDuration);
        }
        else
        {
            CompleteDelivery(null);
        }
    }

    private void CompleteDelivery(Loot loot)
    {
        if (_carryingLoot != null)
            _carryingLoot.DeliveryFinished -= CompleteDelivery;

        _carryingLoot = null;
        _magnet.Deactivate();
        _navMeshAgent.enabled = true;
        _navMeshAgent.isStopped = false;
        _currentState = DronState.Idle;
        DroneReturned?.Invoke(this);
    }
}