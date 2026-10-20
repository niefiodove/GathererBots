using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class Dron : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _lootStoppingDistance = 3f;
    [SerializeField] private float _approachOffset = 2f;
    [SerializeField] private Vector3 _capturedLootPosition = new Vector3(0, 0, -1.3f);
    [SerializeField] private MagneticAttractor _magnet;
    [SerializeField] private float _buildDuration = 3f;

    private const float RotationSnapThresholdDegrees = 2f;
    private const float ApproachStoppingDistance = 0.5f;
    private const float VelocityStopThreshold = 0.05f;

    private NavMeshAgent _navMeshAgent;
    private Loot _carryingLoot;
    private Transform _spawnPoint;
    private Vector3 _approachPoint;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Coroutine _currentRoutine;

    public event Action<Dron, Loot> DeliveryReady;
    public event Action<Dron> BuildCompleted;
    public event Action<Dron> DroneReturned;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();

        if (_navMeshAgent != null) 
            _navMeshAgent.speed = _speed;

        if (_magnet == null) 
            _magnet = GetComponentInChildren<MagneticAttractor>();
    }

    public void SetSpawnPoint(Transform spawnPoint)
    {
        _spawnPoint = spawnPoint;
        _approachPoint = spawnPoint.TransformPoint(new Vector3(0, 0, _approachOffset));
        _initialPosition = spawnPoint.position;
        _initialRotation = Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
    }

    public Transform GetSpawnPoint() => _spawnPoint;

    public void GoToLoot(Loot loot)
    {
        if (loot == null)
            return;

        StopCurrentRoutine();
        _currentRoutine = StartCoroutine(FetchLootRoutine(loot));
    }

    public void GoToBuildTarget(Transform targetPosition)
    {
        if (targetPosition == null)
            return;

        StopCurrentRoutine();
        _currentRoutine = StartCoroutine(BuildTargetRoutine(targetPosition));
    }

    public Loot HandOverLoot()
    {
        Loot loot = _carryingLoot;

        if (loot != null)
        {
            _carryingLoot = null;
            loot.Detach();
            _magnet.Deactivate();
        }

        return loot;
    }

    private void StopCurrentRoutine()
    {
        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
            _currentRoutine = null; 
        }

        ResetNavMeshAgent();
    }

    private void ResetNavMeshAgent()
    {
        if (_navMeshAgent == null)
            return;

        _navMeshAgent.enabled = true;
        _navMeshAgent.isStopped = false;
        _navMeshAgent.updateRotation = true;
        _navMeshAgent.ResetPath();
    }

    private void NavigateTo(Vector3 destination, float stoppingDistance)
    {
        if (_navMeshAgent == null)
            return;

        _navMeshAgent.stoppingDistance = stoppingDistance;
        _navMeshAgent.SetDestination(destination);
        _navMeshAgent.enabled = true;
        _navMeshAgent.isStopped = false;
        _navMeshAgent.updateRotation = true;
    }

    private void HaltMovement()
    {
        if (_navMeshAgent == null)
            return;

        _navMeshAgent.isStopped = true;
        _navMeshAgent.updateRotation = false;
    }

    private void DisableAgent()
    {
        if (_navMeshAgent == null)
            return;

        _navMeshAgent.enabled = false;
        _navMeshAgent.updateRotation = false;
    }

    private IEnumerator FetchLootRoutine(Loot loot)
    {
        _carryingLoot = loot;
        ResetNavMeshAgent();

        NavigateTo(loot.transform.position, _lootStoppingDistance);
        yield return new WaitUntil(() => HasReachedDestination());

        HaltMovement();
        Vector3 directionAwayFromLoot = (transform.position - loot.transform.position).normalized;
       
        if (directionAwayFromLoot != Vector3.zero)
            yield return RotateToDirection(directionAwayFromLoot);

        PickupLoot();

        NavigateTo(_approachPoint, ApproachStoppingDistance);
        yield return new WaitUntil(() => HasReachedDestination());

        DisableAgent();
        yield return RotateToRotation(_initialRotation);
        transform.position = _initialPosition;

        DeliveryReady?.Invoke(this, _carryingLoot);
        CompleteTask();
    }

    private IEnumerator BuildTargetRoutine(Transform target)
    {
        ResetNavMeshAgent();
        NavigateTo(target.position, ApproachStoppingDistance);
        yield return new WaitUntil(() => HasReachedDestination());
        HaltMovement();
        yield return new WaitForSeconds(_buildDuration);
        BuildCompleted?.Invoke(this);
        CompleteTask();
    }

    private bool HasReachedDestination()
    {
        if (!_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
            return true;

        if (_navMeshAgent.pathPending)
            return false;

        return _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance &&
               _navMeshAgent.velocity.sqrMagnitude < VelocityStopThreshold;
    }

    private IEnumerator RotateToDirection(Vector3 direction) => RotateToRotation(Quaternion.LookRotation(direction));

    private IEnumerator RotateToRotation(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > RotationSnapThresholdDegrees)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    private void PickupLoot()
    {
        if (_carryingLoot != null)
        {
            _carryingLoot.AttachTo(transform, _capturedLootPosition);
            _carryingLoot.PrepareForPickup();
            _magnet.Activate();
            _magnet.SetAttractMode(true);
        }
    }

    private void CompleteTask()
    {
        ResetNavMeshAgent();
        DroneReturned?.Invoke(this);
    }
}