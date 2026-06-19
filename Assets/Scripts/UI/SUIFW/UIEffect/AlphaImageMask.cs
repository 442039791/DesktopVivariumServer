using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteInEditMode]
public class AlphaImageMask : MonoBehaviour
{
    [Header("遮罩设置")]
    [SerializeField] private Texture2D maskTexture;
    [SerializeField][Range(0f, 1f)] private float alphaCutoff = 0.1f;
    [SerializeField] private Vector2 maskScale = Vector2.one;
    [SerializeField] private Vector2 maskOffset = Vector2.zero;

    [Header("遮罩预览")]
    [SerializeField] private bool showDebugMask = false;
    [SerializeField] private Color debugColor = new Color(1f, 0.4f, 0.78f, 0.7f);

    private Material maskMaterial;
    private Image maskImage;

    // Shader属性ID
    private static readonly int MaskTex = Shader.PropertyToID("_MaskTex");
    private static readonly int AlphaCutoff = Shader.PropertyToID("_AlphaCutoff");
    private static readonly int MaskScale = Shader.PropertyToID("_MaskScale");
    private static readonly int MaskOffset = Shader.PropertyToID("_MaskOffset");

    void OnEnable()
    {
        Initialize();
        ApplyToChildren();
    }

    void OnValidate()
    {
        if (!isActiveAndEnabled) return;

        Initialize();
        ApplyToChildren();
        UpdateDebugVisual();
    }

    void OnTransformChildrenChanged()
    {
        ApplyToChildren();
    }

    private void Initialize()
    {
        // 确保有Image组件
        if (maskImage == null)
            maskImage = GetComponent<Image>();

        // 创建材质
        if (maskMaterial == null)
        {
            Shader shader = Shader.Find("UI/AlphaImageMask");
            if (shader == null)
            {
                Debug.LogError("UI/AlphaImageMask shader not found!");
                return;
            }
            maskMaterial = new Material(shader);
        }

        // 应用遮罩属性
        if (maskTexture != null)
        {
            maskMaterial.SetTexture(MaskTex, maskTexture);
            maskMaterial.SetFloat(AlphaCutoff, alphaCutoff);
            maskMaterial.SetVector(MaskScale, maskScale);
            maskMaterial.SetVector(MaskOffset, maskOffset);
        }

        // 设置自身为遮罩源
        if (maskImage != null)
        {
            maskImage.material = maskMaterial;
        }
    }

    private void ApplyToChildren()
    {
        if (maskMaterial == null) return;

        foreach (Transform child in transform)
        {
            if (child == transform) continue;

            // 应用遮罩到所有Graphic组件
            var graphic = child.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.material = maskMaterial;
                graphic.SetMaterialDirty();
            }

            // 递归应用到嵌套UI
            if (child.childCount > 0)
            {
                ApplyToChildrenRecursive(child);
            }
        }
    }

    private void ApplyToChildrenRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            var graphic = child.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.material = maskMaterial;
                graphic.SetMaterialDirty();
            }

            if (child.childCount > 0)
            {
                ApplyToChildrenRecursive(child);
            }
        }
    }

    private void UpdateDebugVisual()
    {
        if (!showDebugMask || maskImage == null) return;

        // 创建临时调试材质
        Material debugMat = new Material(Shader.Find("UI/Default"));
        debugMat.color = debugColor;
        maskImage.material = debugMat;
    }

    // API方法 - 运行时修改遮罩
    public void SetMaskTexture(Texture2D newTexture)
    {
        maskTexture = newTexture;
        Initialize();
        ApplyToChildren();
    }

    public void SetAlphaCutoff(float cutoff)
    {
        alphaCutoff = Mathf.Clamp01(cutoff);
        maskMaterial.SetFloat(AlphaCutoff, alphaCutoff);
        ApplyToChildren();
    }

    public void SetMaskScale(Vector2 scale)
    {
        maskScale = scale;
        maskMaterial.SetVector(MaskScale, maskScale);
        ApplyToChildren();
    }

    public void SetMaskOffset(Vector2 offset)
    {
        maskOffset = offset;
        maskMaterial.SetVector(MaskOffset, maskOffset);
        ApplyToChildren();
    }
}
