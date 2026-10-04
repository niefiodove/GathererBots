using UnityEngine;

public class BaseFlag : MonoBehaviour
{
    public void PlaceAt(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
    }
}