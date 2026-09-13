using UnityEngine;

public class DronSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _prefabDron;
    [SerializeField] private int _initialQuantityDrons = 3;
    [SerializeField] private Base _base;

    private Transform[] _droneParkings;

    private void Awake()
    {
        InitializedDrondroneParkings();
        InitializedDrons();
    }

    private Dron CreateDron(Transform droneParking)
    {
        GameObject dron = Instantiate(_prefabDron, droneParking.position, droneParking.rotation);
        Dron dronComponent = dron.GetComponent<Dron>();

        dronComponent.SetSpawnPoint(droneParking);
        _base.RegisterDron(dronComponent);

        return dronComponent;
    }

    private void InitializedDrondroneParkings()
    {
        DronParking[] parkings = GetComponentsInChildren<DronParking>();
        _droneParkings = new Transform[parkings.Length];
        for (int i = 0; i < parkings.Length; i++) _droneParkings[i] = parkings[i].transform;
    }

    private void InitializedDrons()
    {
        int count = Mathf.Min(_initialQuantityDrons, _droneParkings.Length);
        for (int i = 0; i < count; i++) CreateDron(_droneParkings[i]);
    }
}