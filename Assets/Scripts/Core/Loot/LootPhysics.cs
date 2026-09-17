using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class LootPhysics : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Collider[] _colliders;
    [SerializeField] private NavMeshObstacle _obstacle;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();
        _obstacle = GetComponent<NavMeshObstacle>();
    }

    public void SetKinematic(bool isKinematic)
    {
        _rigidbody.isKinematic = isKinematic;
    }

    public void EnableComponents(bool enable)
    {
        foreach (var col in _colliders)
            col.enabled = enable;

        if (_obstacle != null)
            _obstacle.enabled = enable;
    }
}