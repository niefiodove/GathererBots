using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DetectorParticle : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();

        _particleSystem.Stop();
        _particleSystem.Clear();
    }
    private void OnEnable()
    {
        InputControler.OnBaseClicked += StartParticle;
    }

    private void OnDisable()
    {
        InputControler.OnBaseClicked -= StartParticle;
    }
    private void StartParticle()
    {
        if (_particleSystem.isPlaying)
            return;

        _particleSystem.Play();
    }
}
