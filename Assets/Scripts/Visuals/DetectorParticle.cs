using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DetectorParticle : MonoBehaviour
{
    [SerializeField] private Base _ownerBase;

    private RaycastHandler _raycastHandler;
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _particleSystem.Stop();
        _particleSystem.Clear();

        SceneContext context = FindFirstObjectByType<SceneContext>();

        if (context != null)
            _raycastHandler = context.RaycastHandler;
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

        StartParticle();
    }

    private void StartParticle()
    {
        if (_particleSystem.isPlaying) return;
        _particleSystem.Play();
    }
}