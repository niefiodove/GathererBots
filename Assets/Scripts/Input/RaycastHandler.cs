using System;
using UnityEngine;

public class RaycastHandler : MonoBehaviour
{
    [SerializeField] private InputControler _inputControler;
    [SerializeField] private Camera _camera;

    public event Action OnBaseClicked;

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main;
    }

    private void OnEnable()
    {
        if (_inputControler != null)
            _inputControler.OnMouseClicked += HandleMouseClicked;
    }

    private void OnDisable()
    {
        if (_inputControler != null)
            _inputControler.OnMouseClicked -= HandleMouseClicked;
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
                OnBaseClicked?.Invoke();
        }
    }
}