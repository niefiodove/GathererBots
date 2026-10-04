using System;
using System.Collections;
using UnityEngine;

public class Detector : MonoBehaviour
{
    [SerializeField] private float _scanSpeed = 5f;
    [SerializeField] private float _scanTime = 3f;
    [SerializeField] private Base _ownerBase;

    private RaycastHandler _raycastHandler;
    private Vector3 _defaultScale;
    private bool _isScanning = false;

    public event Action<Loot> LootFound;

    private void Awake()
    {
        SceneContext context = FindFirstObjectByType<SceneContext>();

        if (context != null)
            _raycastHandler = context.RaycastHandler;
    }

    private void Start()
    {
        _defaultScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (_raycastHandler != null)
            _raycastHandler.BaseClicked += OnBaseClicked;
    }

    private void OnDisable()
    {
        if (_raycastHandler != null)
            _raycastHandler.BaseClicked -= OnBaseClicked;
    }

    private void OnBaseClicked(Base clickedBase)
    {
        if (clickedBase != _ownerBase)
            return;

        StartCoroutineScale();
    }

    private void OnTriggerEnter(Collider other)
    {
        Loot loot = other.GetComponent<Loot>();

        if (loot != null)
        {
            loot.Reveal();
            LootFound?.Invoke(loot);
        }
    }

    public void ResetScale()
    {
        transform.localScale = _defaultScale;
    }

    private void StartCoroutineScale()
    {
        if (_isScanning) 
            return;

        StartCoroutine(StartScale());
    }

    private IEnumerator StartScale()
    {
        _isScanning = true;
        float timer = 0f;

        while (timer < _scanTime)
        {
            timer += Time.deltaTime;
            float growthMultiplier = 1f + (_scanSpeed * timer);
            transform.localScale = _defaultScale * growthMultiplier;
            yield return null;
        }

        _isScanning = false;
        ResetScale();
    }
}