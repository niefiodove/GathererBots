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

    private enum State { Idle, MovingToLoot, TurningAtLoot, ReturningToBase, Parking, TurningInPlace, Delivering }

    private State _currentState = State.Idle;
    private NavMeshAgent _agent;
    private Base _homeBase;
    private Loot _carryingLoot;
    private Transform _spawnPoint;
    private Vector3 _approachPoint;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    public static event Action<Dron> DroneReturned;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent != null) _agent.speed = _speed;
        if (_magnet == null) _magnet = GetComponentInChildren<MagneticAttractor>();

        _initialPosition = transform.position;
        _initialRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }

    public void Initialize(Base baseRef) => _homeBase = baseRef;

    public void SetSpawnPoint(Transform spawnPoint)
    {
        _spawnPoint = spawnPoint;
        _approachPoint = spawnPoint.TransformPoint(new Vector3(0, 0, _approachOffset));
    }

    public void GoToLoot(Loot loot)
    {
        if (_agent == null || loot == null) return;

        Debug.Log($"{gameObject.name}: Получил задачу, двигаюсь к луту.");

        _carryingLoot = loot;
        _agent.stoppingDistance = _lootStoppingDistance;
        _agent.SetDestination(loot.transform.position);
        _agent.enabled = true;
        _agent.isStopped = false;
        _currentState = State.MovingToLoot;
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.MovingToLoot: UpdateMovingToLoot(); break;
            case State.TurningAtLoot: UpdateTurningAtLoot(); break;
            case State.ReturningToBase: UpdateReturningToBase(); break;
            case State.Parking: UpdateParking(); break;
            case State.TurningInPlace: UpdateTurningInPlace(); break;
            case State.Delivering: break;
        }
    }

    private void UpdateMovingToLoot()
    {
        if (_agent.remainingDistance <= _agent.stoppingDistance && _agent.velocity.sqrMagnitude < 0.01f)
            _currentState = State.TurningAtLoot;
    }

    private void UpdateTurningAtLoot()
    {
        Vector3 directionAwayFromLoot = (transform.position - _carryingLoot.transform.position);
        if (directionAwayFromLoot.sqrMagnitude < 0.01f)
        {
            PickupLoot();
            ReturnToSpawnPoint();
            return;
        }

        Quaternion targetRot = Quaternion.LookRotation(directionAwayFromLoot.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * _rotationSpeed);

        if (Quaternion.Angle(transform.rotation, targetRot) < 2f)
        {
            transform.rotation = targetRot;
            PickupLoot();
            ReturnToSpawnPoint();
        }
    }

    private void UpdateReturningToBase()
    {
        if (_agent.remainingDistance <= _agent.stoppingDistance && _agent.velocity.sqrMagnitude < 0.01f)
        {
            _agent.stoppingDistance = _parkingEpsilon;
            _agent.SetDestination(_initialPosition);
            _currentState = State.Parking;
        }
    }

    private void UpdateParking()
    {
        if (_agent.remainingDistance <= _agent.stoppingDistance && _agent.velocity.sqrMagnitude < 0.01f)
        {
            _agent.enabled = false;
            _currentState = State.TurningInPlace;
        }
    }

    private void UpdateTurningInPlace()
    {
        if (Quaternion.Angle(transform.rotation, _initialRotation) < 2f)
        {
            transform.rotation = _initialRotation;
            transform.position = _initialPosition;
            _currentState = State.Delivering;
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
            _magnet.Activate();
            _magnet.SetAttractMode(true);
        }
    }

    private void ReturnToSpawnPoint()
    {
        if (_spawnPoint == null) return;

        _agent.stoppingDistance = 0.5f;
        _agent.SetDestination(_approachPoint);
        _agent.isStopped = false;
        _agent.enabled = true;
        _currentState = State.ReturningToBase;
    }

    private void StartDelivery()
    {
        if (_carryingLoot != null)
        {
            _magnet.SetAttractMode(false);
            _homeBase.ResourceDelivered(_carryingLoot);
            _carryingLoot.transform.SetParent(_homeBase.transform);
            _carryingLoot.StartDeliveryAnimation(_homeBase.Center, _deliveryDuration, OnDeliveryFinished);
        }
    }

    private void OnDeliveryFinished()
    {
        _carryingLoot = null;
        _magnet.Deactivate();
        _agent.enabled = true;
        _agent.isStopped = false;
        _currentState = State.Idle;
        DroneReturned?.Invoke(this);
    }
}