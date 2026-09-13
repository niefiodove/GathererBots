using UnityEngine;

public class MagneticAttractor : MonoBehaviour
{
    [SerializeField] private ParticleSystem _magnetParticle;
    [SerializeField] private float _attractSpeed = -2f;
    [SerializeField] private float _repelSpeed = 2f;

    private void Awake()
    {
        if (_magnetParticle != null)
        {
            _magnetParticle.Stop();
        }
    }

    public void Activate()
    {
        if (_magnetParticle != null && !_magnetParticle.isPlaying)
        {
            _magnetParticle.Play();
        }
    }

    public void Deactivate()
    {
        if (_magnetParticle != null && _magnetParticle.isPlaying)
        {
            _magnetParticle.Stop();
        }
    }

    public void SetAttractMode(bool isAttracting)
    {
        if (_magnetParticle == null) return;
        var vel = _magnetParticle.velocityOverLifetime;
       
        if (vel.enabled)
            vel.z = isAttracting ? _attractSpeed : _repelSpeed;
    }
}