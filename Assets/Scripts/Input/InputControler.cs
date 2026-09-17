using System;
using UnityEngine;

public class InputControler : MonoBehaviour
{
    public event Action<Vector2> OnMouseClicked;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnMouseClicked?.Invoke(Input.mousePosition);
    }
}