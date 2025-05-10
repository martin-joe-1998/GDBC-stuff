using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RippleBW : MonoBehaviour
{
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        _renderer.GetPropertyBlock(_propertyBlock);

        // ��������������ţ�lossyScale������ Shader
        _propertyBlock.SetVector("_ObjectScale", transform.lossyScale);

        _renderer.SetPropertyBlock(_propertyBlock);
    }
}
