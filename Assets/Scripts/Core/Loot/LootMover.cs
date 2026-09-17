using UnityEngine;
using System;
using System.Collections;

public class LootMover : MonoBehaviour
{
    public event Action MovementFinished;

    private Coroutine _moveCoroutine;

    public void MoveTo(Transform target, float duration)
    {
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);

        _moveCoroutine = StartCoroutine(MoveRoutine(target, duration));
    }

    public void StopMoving()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
    }

    private IEnumerator MoveRoutine(Transform target, float duration)
    {
        Vector3 startPosition = transform.position;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float interpolationFactor = timer / duration;
            transform.position = Vector3.Lerp(startPosition, target.position, interpolationFactor);
            yield return null;
        }

        MovementFinished?.Invoke();
    }
}