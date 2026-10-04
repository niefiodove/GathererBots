using UnityEngine;
using System.Collections.Generic;

public class SceneContext : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _groundObject;
    [SerializeField] private BoxCollider _groundCube;
    [SerializeField] private RaycastHandler _raycastHandler;
    [SerializeField] private InputControler _inputControler;

    private List<Base> _registeredBases = new List<Base>();

    public Camera MainCamera => _mainCamera;
    public GameObject GroundObject => _groundObject;
    public BoxCollider GroundCube => _groundCube;
    public RaycastHandler RaycastHandler => _raycastHandler;
    public InputControler InputControler => _inputControler;

    public IReadOnlyList<Base> RegisteredBases => _registeredBases;

    private void Awake()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;
    }

    public void RegisterBase(Base baseObj)
    {
        if (baseObj != null && !_registeredBases.Contains(baseObj))
            _registeredBases.Add(baseObj);
    }
    public void UnregisterBase(Base baseObj)
    {
        if (baseObj != null)
            _registeredBases.Remove(baseObj);
    }
}