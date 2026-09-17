using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DetectorParticle : MonoBehaviour
{
    [SerializeField] private RaycastHandler _raycastHandler;
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _particleSystem.Stop();
        _particleSystem.Clear();
    }

    private void OnEnable()
    {
        if (_raycastHandler != null)
            _raycastHandler.BaseClicked += StartParticle;
    }

    private void OnDisable()
    {
        if (_raycastHandler != null)
            _raycastHandler.BaseClicked -= StartParticle;
    }

    private void StartParticle()
    {
        if (_particleSystem.isPlaying)
            return;

        _particleSystem.Play();
    }
}