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


    public void AttachTo(Transform parentTransform, Vector3 localPosition)
    {
        transform.SetParent(parentTransform);
        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.identity;
    }

    public void PrepareForSpawn()
    {
        _physics.EnableComponents(true);
        _physics.SetKinematic(true);
        _visual.Hide();
        _color.ResetAlpha();
        IsRevealed = false;
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

        if (_fadeCoroutine != null) 
        { 
            StopCoroutine(_fadeCoroutine); _fadeCoroutine = null;
        }

        _mover.StopMoving();
    }

    public void StartDeliveryAnimation(Transform targetCenter, float duration)
    {
        if (!gameObject.activeInHierarchy)
            return;

        _mover.MoveTo(targetCenter, duration);
        _fadeCoroutine = StartCoroutine(FadeOutRoutine(duration));
    }

    public void Detach()
    {
        transform.SetParent(null);
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

        _physics.SetKinematic(true);
        _physics.EnableComponents(false);

        _color.ResetAlpha();
        gameObject.SetActive(false);
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            _color.SetAlpha(1f - (timer / duration));
            yield return null;
        }
    }
    private void OnDestroy() => _mover.MovementFinished -= HandleMovementFinished;

    private void HandleMovementFinished() => DeliveryFinished?.Invoke(this);
}