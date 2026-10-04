using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DronSpawner : Spawner<Dron>
{
    [SerializeField] private int _initialQuantityDrons = 3;
    [SerializeField] private Base _base;

    [SerializeField] private DronHologram _hologramPrefab;
    [SerializeField] private float _hologramFadeDuration = 3f;

    private Transform[] _droneParkings;
    private WaitForSeconds _waitHologramFadeDuration;
    private int _nextParkingIndex = 0;
    private int _spawnedDronesCount = 0;
    private bool _skipInitialSpawn = false;

    protected override void Awake()
    {
        InitializeParkings();
        _waitHologramFadeDuration = new WaitForSeconds(_hologramFadeDuration);
    }

    private void Start()
    {
        if (!_skipInitialSpawn)
            SpawnDrons();
    }

    private void InitializeParkings()
    {
        DronParking[] parkings = GetComponentsInChildren<DronParking>();
        _droneParkings = new Transform[parkings.Length];

        for (int i = 0; i < parkings.Length; i++)
            _droneParkings[i] = parkings[i].transform;
    }

    private void SpawnDrons()
    {
        int count = Mathf.Min(_initialQuantityDrons, _droneParkings.Length);

        for (int i = 0; i < count; i++)
        {
            CreateDron(_droneParkings[i]);
            _spawnedDronesCount++;
            _nextParkingIndex++;
        }
    }

    public bool CanSpawnDrone()
    {
        return _spawnedDronesCount < _droneParkings.Length;
    }

    public void SpawnNewDrone()
    {
        if (!CanSpawnDrone())
            return;

        Transform parking = GetNextParking();

        if (parking == null)
            return;

        StartCoroutine(SpawnDroneWithHologramRoutine(parking));
    }

    public void SkipInitialSpawn()
    {
        _skipInitialSpawn = true;
    }

    public void AssignParkingToExistingDrone(Dron dron)
    {
        if (_nextParkingIndex >= _droneParkings.Length)
            return;

        Transform parking = _droneParkings[_nextParkingIndex++];
        dron.SetSpawnPoint(parking);
        dron.transform.position = parking.position;
        dron.transform.rotation = parking.rotation;
        _spawnedDronesCount++;
    }

    private IEnumerator SpawnDroneWithHologramRoutine(Transform parking)
    {
        DronHologram hologram = Instantiate(_hologramPrefab, parking.position, parking.rotation);
        yield return _waitHologramFadeDuration;
        Destroy(hologram.gameObject);
        CreateDron(parking);
        _spawnedDronesCount++;
    }

    private Transform GetNextParking()
    {
        if (_nextParkingIndex < _droneParkings.Length)
            return _droneParkings[_nextParkingIndex++];

        return null;
    }

    private void CreateDron(Transform parking)
    {
        Dron dron = Instantiate(Prefab, parking.position, parking.rotation);
        dron.SetSpawnPoint(parking);
        _base.RegisterDron(dron);
    }
}