using UnityEngine;
using System;
using System.Collections;

public class Loot : MonoBehaviour
{
    [SerializeField] private LootVisual _visual;
    [SerializeField] private LootPhysics _physics;
    [SerializeField] private LootColor _color;
    [SerializeField] private LootMover _mover;

    public event Action<Loot> DeliveryFinished;
    public bool IsRevealed { get; private set; } = false;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _visual = GetComponent<LootVisual>();
        _physics = GetComponent<LootPhysics>();
        _color = GetComponent<LootColor>();
        _mover = GetComponent<LootMover>();
        _mover.MovementFinished += HandleMovementFinished;
    }

    private void OnDestroy()
    {
        _mover.MovementFinished -= HandleMovementFinished;
    }

    public void Reveal()
    {
        IsRevealed = true;
        _visual.Show();
        _physics.SetKinematic(false);
        _physics.EnableComponents(true);
        _color.ResetAlpha();
    }

    public void PrepareForPickup()
    {
        _physics.SetKinematic(true);
        _physics.EnableComponents(false);
    }

    public void StartDeliveryAnimation(Transform targetCenter, float duration)
    {
        _mover.MoveTo(targetCenter, duration);
        _fadeCoroutine = StartCoroutine(FadeOutRoutine(duration));
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - (timer / duration);
            _color.SetAlpha(alpha);
            yield return null;
        }
    }

    private void HandleMovementFinished()
    {
        DeliveryFinished?.Invoke(this);
    }

    public void Deactivate()
    {
        IsRevealed = false;
        _mover.StopMoving();

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        transform.SetParent(null);
        _visual.Hide();
        _physics.SetKinematic(false);
        _physics.EnableComponents(true);
        _color.ResetAlpha();
        gameObject.SetActive(false);
    }
}