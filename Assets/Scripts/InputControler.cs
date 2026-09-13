using System;
using UnityEngine;

public class InputControler : MonoBehaviour
{
    public static event Action OnBaseClicked;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                InputControler targetController = hit.collider.GetComponentInParent<InputControler>();

                if (targetController != null)
                    OnBaseClicked.Invoke();
            }
        }
    }
}