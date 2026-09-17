using UnityEngine;
using TMPro;
using System;

public class ResourceCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _counterText;
    [SerializeField] private string _startText = "0";
    [SerializeField] private Base _base;

    private void Start()
    {
        if (_counterText != null)
            _counterText.text = _startText;
    }

    private void OnEnable()
    {
        if (_base != null)
            _base.ResourceCountChanged += UpdateCounterText;
    }

    private void OnDisable()
    {
        if (_base != null)
            _base.ResourceCountChanged -= UpdateCounterText;
    }

    private void UpdateCounterText(int currentCount)
    {
        if (_counterText != null)
            _counterText.text = currentCount.ToString();
    }
}