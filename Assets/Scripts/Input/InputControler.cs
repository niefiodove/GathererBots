using System;
using UnityEngine;

public class InputControler : MonoBehaviour
{
    public event Action<Vector2> MouseClicked;
    public event Action<Vector2> MouseRightClicked;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            MouseClicked?.Invoke(Input.mousePosition);

        if (Input.GetMouseButtonDown(1))
            MouseRightClicked?.Invoke(Input.mousePosition);
    }
}