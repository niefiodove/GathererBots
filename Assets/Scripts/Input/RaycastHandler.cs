using System;
using UnityEngine;

public class RaycastHandler : MonoBehaviour
{
    [SerializeField] private InputControler _inputControler;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _groundObject;

    public event Action<Base> BaseClicked;
    public event Action<Base> BaseRightClicked;
    public event Action<Vector3> GroundRightClicked;

    private void Awake()
    {
        if (_camera == null) 
            _camera = Camera.main;
    }

    private void OnEnable()
    {
        if (_inputControler != null)
        {
            _inputControler.MouseClicked += HandleMouseClicked;
            _inputControler.MouseRightClicked += HandleMouseRightClicked;
        }
    }

    private void OnDisable()
    {
        if (_inputControler != null)
        {
            _inputControler.MouseClicked -= HandleMouseClicked;
            _inputControler.MouseRightClicked -= HandleMouseRightClicked;
        }
    }

    private void HandleMouseClicked(Vector2 screenPosition)
    {
        if (_camera == null)
            return;

        Ray ray = _camera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Base targetBase = hit.collider.GetComponentInParent<Base>();

            if (targetBase != null)
                BaseClicked?.Invoke(targetBase);
        }
    }

    private void HandleMouseRightClicked(Vector2 screenPosition)
    {
        if (_camera == null)
            return;

        Ray ray = _camera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Base targetBase = hit.collider.GetComponentInParent<Base>();

            if (targetBase != null)
            {
                BaseRightClicked?.Invoke(targetBase);
                return;
            }

            if (_groundObject != null && hit.collider.gameObject == _groundObject)
                GroundRightClicked?.Invoke(hit.point);
        }
    }
}