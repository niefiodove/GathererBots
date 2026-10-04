using UnityEngine;

public class GhostBase : MonoBehaviour
{
    [SerializeField] private Color _validColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color _invalidColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private float _yOffset = 0.1f;
    [SerializeField] private float _smoothSpeed = 15f;

    private Renderer[] _renderers;
    private BoxCollider[] _childColliders;
    private MaterialPropertyBlock _block;
    private int _colorId;

    private Bounds _groundBounds;
    private float _groundY;
    private Base _currentBase;
    private bool _isValid;
    private Vector3 _targetPosition;

    public bool IsValid => _isValid;

    public void Initialize(Bounds groundBounds, Base currentBase)
    {
        _groundBounds = groundBounds;
        _groundY = groundBounds.max.y;
        _currentBase = currentBase;

        _renderers = GetComponentsInChildren<Renderer>();
        _childColliders = GetComponentsInChildren<BoxCollider>();

        _block = new MaterialPropertyBlock();
        _colorId = Shader.PropertyToID("_BaseColor");

        _targetPosition = transform.position;
    }

    public void SetTargetPosition(Vector3 groundHitPoint)
    {
        _targetPosition = new Vector3(groundHitPoint.x, _groundY + _yOffset, groundHitPoint.z);
    }

    private void LateUpdate()
    {
        if (Vector3.Distance(transform.position, _targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, _smoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = _targetPosition;
        }

        _isValid = CheckValidity();
        UpdateVisual(_isValid);
    }

    private bool CheckValidity()
    {
        foreach (var col in _childColliders)
        {
            Bounds b = col.bounds;
            if (b.min.x < _groundBounds.min.x || b.max.x > _groundBounds.max.x ||
                b.min.z < _groundBounds.min.z || b.max.z > _groundBounds.max.z)
                return false;
        }

        foreach (var col in _childColliders)
        {
            Vector3 worldCenter = col.transform.TransformPoint(col.center);
            Vector3 worldExtents = Vector3.Scale(col.size * 0.5f, col.transform.lossyScale);

            Collider[] hits = Physics.OverlapBox(
                worldCenter,
                worldExtents,
                col.transform.rotation,
                -1,
                QueryTriggerInteraction.Collide
            );

            foreach (var hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    continue;

                if (hit.GetComponentInParent<Base>() != null)
                    return false;

                Loot foundLoot = hit.GetComponentInParent<Loot>();
                if (foundLoot != null && foundLoot.IsRevealed)
                    return false;

                if (hit.GetComponentInParent<Dron>() != null)
                    return false;
            }
        }

        return true;
    }

    private void UpdateVisual(bool isValid)
    {
        Color targetColor = isValid ? _validColor : _invalidColor;

        foreach (var rend in _renderers)
        {
            rend.GetPropertyBlock(_block);
            _block.SetColor(_colorId, targetColor);
            rend.SetPropertyBlock(_block);
        }
    }
}