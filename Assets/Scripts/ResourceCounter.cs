using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class ResourceCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _counterText;
    [SerializeField] private string _startText = "0";

    private void Start()
    {
        if (_counterText != null)
            _counterText.text = _startText;
    }

    private void OnEnable()
    {
        Base.ResourceCountChanged += UpdateCounterText;
    }

    private void OnDisable()
    {
        Base.ResourceCountChanged -= UpdateCounterText;
    }

    private void UpdateCounterText(int currentCount)
    {
        if (_counterText != null)
            _counterText.text = currentCount.ToString();
    }
}