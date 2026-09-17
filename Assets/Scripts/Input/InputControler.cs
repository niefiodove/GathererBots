using System;
using UnityEngine;

public class InputControler : MonoBehaviour
{
    public event Action<Vector2> MouseClicked;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            MouseClicked?.Invoke(Input.mousePosition);
    }
}