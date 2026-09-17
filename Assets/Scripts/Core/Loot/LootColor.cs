using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class LootColor : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;

    private const float FullAlpha = 1f;

    private MaterialPropertyBlock _materialPropertyBlock;
    private int _colorId;
    private Color _originalColor;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
        _colorId = Shader.PropertyToID("_BaseColor");
        _originalColor = _meshRenderer.sharedMaterial.GetColor(_colorId);
    }

    public void ResetAlpha()
    {
        SetAlpha(FullAlpha);
    }

    public void SetAlpha(float alpha)
    {
        _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
        Color newColor = _originalColor;
        newColor.a = alpha;
        _materialPropertyBlock.SetColor(_colorId, newColor);
        _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}