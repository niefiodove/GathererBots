using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Loot : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    private Rigidbody _rb;
    private Collider[] _allColliders;
    private NavMeshObstacle _obstacle;
    private MaterialPropertyBlock _mpb;

    private int _colorId;
    private Color _originalColor;

    private Spawner _ownerSpawner;
    private Coroutine _deliveryCoroutine;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _rb = GetComponent<Rigidbody>();
        _allColliders = GetComponentsInChildren<Collider>();
        _obstacle = GetComponent<NavMeshObstacle>();

        _mpb = new MaterialPropertyBlock();
        _colorId = Shader.PropertyToID("_BaseColor");

        _originalColor = _meshRenderer.sharedMaterial.GetColor(_colorId);

        _meshRenderer.enabled = false;
    }

    public void Initialize(Spawner spawner) => _ownerSpawner = spawner;

    public void Reveal()
    {
        _meshRenderer.enabled = true;
        _rb.isKinematic = false;
        EnablePhysicsComponents(true);
        ResetAlpha();
    }

    public void PrepareForPickup()
    {
        _rb.isKinematic = true;
        EnablePhysicsComponents(false);
    }

    public void StartDeliveryAnimation(Transform targetCenter, float duration, Action onComplete)
    {
        if (_deliveryCoroutine != null) StopCoroutine(_deliveryCoroutine);
        _deliveryCoroutine = StartCoroutine(DeliveryAnimation(targetCenter, duration, onComplete));
    }

    private IEnumerator DeliveryAnimation(Transform targetCenter, float duration, Action onComplete)
    {
        Vector3 startPos = transform.position;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            transform.position = Vector3.Lerp(startPos, targetCenter.position, t);

            _meshRenderer.GetPropertyBlock(_mpb);

            Color c = _originalColor;
            c.a = Mathf.Lerp(1f, 0f, t);

            _mpb.SetColor(_colorId, c);
            _meshRenderer.SetPropertyBlock(_mpb);

            yield return null;
        }

        onComplete?.Invoke();
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        if (_deliveryCoroutine != null)
        {
            StopCoroutine(_deliveryCoroutine);
            _deliveryCoroutine = null;
        }

        transform.SetParent(null);
        _meshRenderer.enabled = false;
        _rb.isKinematic = false;
        EnablePhysicsComponents(true);
        ResetAlpha();

        gameObject.SetActive(false);

        if (_ownerSpawner != null) _ownerSpawner.ReturnToPool(gameObject);
    }

    private void ResetAlpha()
    {
        _meshRenderer.GetPropertyBlock(_mpb);

        Color c = _originalColor;
        c.a = 1f;

        _mpb.SetColor(_colorId, c);
        _meshRenderer.SetPropertyBlock(_mpb);
    }

    private void EnablePhysicsComponents(bool enable)
    {
        foreach (var col in _allColliders) col.enabled = enable;
        if (_obstacle != null) _obstacle.enabled = enable;
    }
}