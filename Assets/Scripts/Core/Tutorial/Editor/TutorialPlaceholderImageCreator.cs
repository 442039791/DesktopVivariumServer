#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Tutorial 占位符图片资源生成器
/// 用于生成新手引导系统所需的基础图片资源
/// </summary>
public class TutorialPlaceholderImageCreator : EditorWindow
{
    [MenuItem("Tools/Tutorial/Create Placeholder Images")]
    private static void CreatePlaceholderImages()
    {
        // 确保目录存在
        string texturePath = "Assets/Textures/Tutorial";
        if (!AssetDatabase.IsValidFolder("Assets/Textures"))
        {
            AssetDatabase.CreateFolder("Assets", "Textures");
        }
        if (!AssetDatabase.IsValidFolder(texturePath))
        {
            AssetDatabase.CreateFolder("Assets/Textures", "Tutorial");
        }

        // 创建各种占位符图片
        CreatePointerImage(texturePath);
        CreateDialogBackgroundImage(texturePath);
        CreateButtonBackgroundImage(texturePath);

        AssetDatabase.Refresh();

        Debug.Log("[Tutorial] 占位符图片资源创建完成!");
        EditorUtility.DisplayDialog("完成", "Tutorial 占位符图片资源已创建在:\n" + texturePath, "确定");
    }

    /// <summary>
    /// 创建指针/手指图片 (箭头形状)
    /// </summary>
    private static void CreatePointerImage(string path)
    {
        int width = 128;
        int height = 128;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];

        // 填充透明背景
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        // 绘制向下的箭头
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 箭头主体 (竖线)
                if (x >= width / 2 - 8 && x <= width / 2 + 8 && y >= 20 && y <= 80)
                {
                    pixels[y * width + x] = Color.white;
                }
                // 箭头头部 (三角形)
                else if (y >= 80 && y <= 110)
                {
                    int triangleWidth = (y - 80) * 2;
                    if (x >= width / 2 - triangleWidth && x <= width / 2 + triangleWidth)
                    {
                        pixels[y * width + x] = Color.white;
                    }
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        SaveTexture(texture, path + "/PointerArrow.png");
    }

    /// <summary>
    /// 创建对话框背景图片
    /// </summary>
    private static void CreateDialogBackgroundImage(string path)
    {
        int width = 256;
        int height = 128;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];

        // 填充深灰色背景
        Color bgColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = bgColor;
        }

        // 添加边框
        Color borderColor = new Color(0.3f, 0.6f, 1f, 1f);
        int borderWidth = 4;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 上下边框
                if (y < borderWidth || y >= height - borderWidth)
                {
                    pixels[y * width + x] = borderColor;
                }
                // 左右边框
                else if (x < borderWidth || x >= width - borderWidth)
                {
                    pixels[y * width + x] = borderColor;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        SaveTexture(texture, path + "/DialogBackground.png");
    }

    /// <summary>
    /// 创建按钮背景图片
    /// </summary>
    private static void CreateButtonBackgroundImage(string path)
    {
        int width = 200;
        int height = 60;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];

        // 填充蓝色背景
        Color bgColor = new Color(0.2f, 0.6f, 1f, 1f);
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = bgColor;
        }

        // 添加高光效果（渐变）
        for (int y = 0; y < height / 2; y++)
        {
            float gradientFactor = (float)y / (height / 2) * 0.2f;
            Color highlightColor = new Color(
                bgColor.r + gradientFactor,
                bgColor.g + gradientFactor,
                bgColor.b + gradientFactor,
                bgColor.a
            );

            for (int x = 0; x < width; x++)
            {
                pixels[y * width + x] = highlightColor;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        SaveTexture(texture, path + "/ButtonBackground.png");
    }

    /// <summary>
    /// 保存纹理为PNG文件
    /// </summary>
    private static void SaveTexture(Texture2D texture, string filePath)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);

        // 导入资源并设置为Sprite
        AssetDatabase.ImportAsset(filePath);

        TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;

            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
        }

        Debug.Log($"[Tutorial] 创建图片: {filePath}");
    }
}
#endif
