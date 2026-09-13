using System;
using System.Collections;
using UnityEngine;

public class Detector : MonoBehaviour
{
    [SerializeField] private float _scanSpeed = 5f;
    [SerializeField] private float _scaleZ = 0.1f;
    [SerializeField] private float _scanTime = 3f;

    private Vector3 _defaultScale;
    private bool _isScanning = false;

    public static event Action<Loot> LootFound;

    private void Start()
    {
        _defaultScale = transform.localScale;
    }

    private void OnEnable()
    {
        InputControler.OnBaseClicked += StartCoroutineScale;
    }

    private void OnDisable()
    {
        InputControler.OnBaseClicked -= StartCoroutineScale;
    }

    public void ResetScale()
    {
        transform.localScale = _defaultScale;
    }

    private void StartCoroutineScale()
    {
        if (_isScanning) return;
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

    private void OnTriggerEnter(Collider other)
    {
        Loot loot = other.GetComponent<Loot>();
        if (loot != null)
        {
            loot.Reveal();
            LootFound?.Invoke(loot);
        }
    }
}