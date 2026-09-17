using UnityEngine;

public class DronSpawner : Spawner<Dron>
{
    [SerializeField] private int _initialQuantityDrons = 3;
    [SerializeField] private Base _base;

    private Transform[] _droneParkings;

    protected override void Awake()
    {
        InitializeParkings();
        SpawnDrons();
    }

    private void InitializeParkings()
    {
        DronParking[] parkings = GetComponentsInChildren<DronParking>();
        _droneParkings = new Transform[parkings.Length];
        
        for (int i = 0; i < parkings.Length; i++)
        {
            _droneParkings[i] = parkings[i].transform;
        }
    }

    private void SpawnDrons()
    {
        int count = Mathf.Min(_initialQuantityDrons, _droneParkings.Length);
        
        for (int i = 0; i < count; i++)
        {
            CreateDron(_droneParkings[i]);
        }
    }

    private void CreateDron(Transform parking)
    {
        Dron dron = Instantiate(_prefab, parking.position, parking.rotation);

        dron.SetSpawnPoint(parking);
        _base.RegisterDron(dron);
    }
}