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
    private bool[] _isParkingOccupied;
    private WaitForSeconds _waitHologramFadeDuration;
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

    public void SkipInitialSpawn() => _skipInitialSpawn = true;

    public void AssignParkingToExistingDrone(Dron dron)
    {
        Transform parking = GetNextParking();

        if (parking == null)
            return;

        dron.SetSpawnPoint(parking);
        dron.transform.position = parking.position;
        dron.transform.rotation = parking.rotation;
        _spawnedDronesCount++;
    }

    public void ReleaseParking(Transform parkingToRelease)
    {
        for (int i = 0; i < _droneParkings.Length; i++)
        {
            if (_droneParkings[i] == parkingToRelease)
            {
                _isParkingOccupied[i] = false;
                _spawnedDronesCount = Mathf.Max(0, _spawnedDronesCount - 1);
                return;
            }
        }
    }

    private void InitializeParkings()
    {
        DronParking[] parkings = GetComponentsInChildren<DronParking>();
        _droneParkings = new Transform[parkings.Length];
        _isParkingOccupied = new bool[parkings.Length];

        for (int i = 0; i < parkings.Length; i++)
        {
            _droneParkings[i] = parkings[i].transform;
            _isParkingOccupied[i] = false;
        }
    }

    private void SpawnDrons()
    {
        int count = Mathf.Min(_initialQuantityDrons, _droneParkings.Length);

        for (int i = 0; i < count; i++)
        {
            Transform parking = GetNextParking();

            if (parking != null)
            {
                CreateDron(parking);
                _spawnedDronesCount++;
            }
        }
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
        for (int i = 0; i < _isParkingOccupied.Length; i++)
        {
            if (!_isParkingOccupied[i])
            {
                _isParkingOccupied[i] = true;
                return _droneParkings[i];
            }
        }
        return null;
    }

    private void CreateDron(Transform parking)
    {
        Dron dron = Instantiate(Prefab, parking.position, parking.rotation);
        dron.SetSpawnPoint(parking);
        _base.RegisterDron(dron);
    }
}