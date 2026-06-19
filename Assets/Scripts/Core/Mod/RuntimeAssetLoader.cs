using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;

/// <summary>
/// 运行时资源加载器 - 支持从文件系统直接加载美术资源
/// 用于 Mod 系统，让玩家无需打包 AssetBundle
/// </summary>
public static class RuntimeAssetLoader
{
    // 缓存已加载的资源
    private static Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();
    private static Dictionary<string, Mesh> _meshCache = new Dictionary<string, Mesh>();
    private static Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();

    #region 贴图加载

    /// <summary>
    /// 从文件加载贴图 (支持 PNG, JPG)
    /// </summary>
    public static Texture2D LoadTexture(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogWarning($"[RuntimeAssetLoader] 贴图文件不存在: {filePath}");
            return null;
        }

        // 检查缓存
        if (_textureCache.TryGetValue(filePath, out var cached))
        {
            return cached;
        }

        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, true);

            if (texture.LoadImage(fileData))
            {
                texture.name = Path.GetFileNameWithoutExtension(filePath);
                texture.Apply(true, false); // 生成 mipmap，不设为只读以便后续可能的操作
                _textureCache[filePath] = texture;
                Debug.Log($"[RuntimeAssetLoader] 贴图加载成功: {texture.name} ({texture.width}x{texture.height})");
                return texture;
            }
            else
            {
                Debug.LogError($"[RuntimeAssetLoader] 贴图解析失败: {filePath}");
                UnityEngine.Object.Destroy(texture);
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RuntimeAssetLoader] 贴图加载异常: {filePath}\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 从贴图创建 Sprite
    /// </summary>
    public static Sprite CreateSprite(Texture2D texture, float pixelsPerUnit = 100f)
    {
        if (texture == null) return null;

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit
        );
    }

    /// <summary>
    /// 从文件加载 Sprite
    /// </summary>
    public static Sprite LoadSprite(string filePath, float pixelsPerUnit = 100f)
    {
        var texture = LoadTexture(filePath);
        return CreateSprite(texture, pixelsPerUnit);
    }

    #endregion

    #region OBJ 模型加载

    /// <summary>
    /// 从 OBJ 文件加载 Mesh
    /// </summary>
    public static Mesh LoadOBJ(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogWarning($"[RuntimeAssetLoader] OBJ 文件不存在: {filePath}");
            return null;
        }

        // 检查缓存
        if (_meshCache.TryGetValue(filePath, out var cached))
        {
            return cached;
        }

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            Mesh mesh = ParseOBJ(lines, Path.GetFileNameWithoutExtension(filePath));

            if (mesh != null)
            {
                _meshCache[filePath] = mesh;
                Debug.Log($"[RuntimeAssetLoader] OBJ 加载成功: {mesh.name} (顶点: {mesh.vertexCount}, 三角形: {mesh.triangles.Length / 3})");
            }

            return mesh;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RuntimeAssetLoader] OBJ 加载异常: {filePath}\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 解析 OBJ 文件内容
    /// </summary>
    private static Mesh ParseOBJ(string[] lines, string meshName)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<int> uvIndices = new List<int>();
        List<int> normalIndices = new List<int>();

        // 用于存储面数据
        List<Vector3Int> faceVertices = new List<Vector3Int>(); // x=顶点索引, y=uv索引, z=法线索引

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            string[] parts = trimmedLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            switch (parts[0].ToLower())
            {
                case "v": // 顶点
                    if (parts.Length >= 4)
                    {
                        float x = ParseFloat(parts[1]);
                        float y = ParseFloat(parts[2]);
                        float z = ParseFloat(parts[3]);
                        vertices.Add(new Vector3(x, y, z));
                    }
                    break;

                case "vt": // UV
                    if (parts.Length >= 3)
                    {
                        float u = ParseFloat(parts[1]);
                        float v = ParseFloat(parts[2]);
                        uvs.Add(new Vector2(u, v));
                    }
                    break;

                case "vn": // 法线
                    if (parts.Length >= 4)
                    {
                        float x = ParseFloat(parts[1]);
                        float y = ParseFloat(parts[2]);
                        float z = ParseFloat(parts[3]);
                        normals.Add(new Vector3(x, y, z));
                    }
                    break;

                case "f": // 面
                    ParseFace(parts, faceVertices);
                    break;
            }
        }

        if (vertices.Count == 0 || faceVertices.Count == 0)
        {
            Debug.LogWarning($"[RuntimeAssetLoader] OBJ 文件没有有效数据");
            return null;
        }

        // 构建最终的 Mesh 数据
        return BuildMesh(meshName, vertices, uvs, normals, faceVertices);
    }

    /// <summary>
    /// 解析面数据
    /// </summary>
    private static void ParseFace(string[] parts, List<Vector3Int> faceVertices)
    {
        List<Vector3Int> faceIndices = new List<Vector3Int>();

        for (int i = 1; i < parts.Length; i++)
        {
            string[] indices = parts[i].Split('/');
            int vertexIndex = 0, uvIndex = -1, normalIndex = -1;

            if (indices.Length >= 1 && !string.IsNullOrEmpty(indices[0]))
                vertexIndex = int.Parse(indices[0]) - 1; // OBJ 索引从 1 开始

            if (indices.Length >= 2 && !string.IsNullOrEmpty(indices[1]))
                uvIndex = int.Parse(indices[1]) - 1;

            if (indices.Length >= 3 && !string.IsNullOrEmpty(indices[2]))
                normalIndex = int.Parse(indices[2]) - 1;

            faceIndices.Add(new Vector3Int(vertexIndex, uvIndex, normalIndex));
        }

        // 三角化多边形（简单的扇形三角化）
        for (int i = 1; i < faceIndices.Count - 1; i++)
        {
            faceVertices.Add(faceIndices[0]);
            faceVertices.Add(faceIndices[i]);
            faceVertices.Add(faceIndices[i + 1]);
        }
    }

    /// <summary>
    /// 构建 Mesh
    /// </summary>
    private static Mesh BuildMesh(string name, List<Vector3> srcVertices, List<Vector2> srcUVs,
        List<Vector3> srcNormals, List<Vector3Int> faceVertices)
    {
        // 由于 OBJ 的顶点/UV/法线可以有不同的索引，需要重新组织数据
        List<Vector3> finalVertices = new List<Vector3>();
        List<Vector2> finalUVs = new List<Vector2>();
        List<Vector3> finalNormals = new List<Vector3>();
        List<int> finalTriangles = new List<int>();

        Dictionary<string, int> vertexMap = new Dictionary<string, int>();

        for (int i = 0; i < faceVertices.Count; i++)
        {
            Vector3Int indices = faceVertices[i];
            string key = $"{indices.x}/{indices.y}/{indices.z}";

            if (!vertexMap.TryGetValue(key, out int finalIndex))
            {
                finalIndex = finalVertices.Count;
                vertexMap[key] = finalIndex;

                finalVertices.Add(srcVertices[indices.x]);

                if (indices.y >= 0 && indices.y < srcUVs.Count)
                    finalUVs.Add(srcUVs[indices.y]);
                else
                    finalUVs.Add(Vector2.zero);

                if (indices.z >= 0 && indices.z < srcNormals.Count)
                    finalNormals.Add(srcNormals[indices.z]);
                else
                    finalNormals.Add(Vector3.up);
            }

            finalTriangles.Add(finalIndex);
        }

        Mesh mesh = new Mesh();
        mesh.name = name;

        // 如果顶点数超过 65535，使用 32 位索引
        if (finalVertices.Count > 65535)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        mesh.vertices = finalVertices.ToArray();
        mesh.uv = finalUVs.ToArray();
        mesh.triangles = finalTriangles.ToArray();

        if (finalNormals.Count == finalVertices.Count)
        {
            mesh.normals = finalNormals.ToArray();
        }
        else
        {
            mesh.RecalculateNormals();
        }

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }

    /// <summary>
    /// 解析浮点数（支持不同区域设置）
    /// </summary>
    private static float ParseFloat(string s)
    {
        return float.Parse(s, CultureInfo.InvariantCulture);
    }

    #endregion

    #region 材质创建

    /// <summary>
    /// 创建基础材质（使用 HDRP Lit Shader）
    /// </summary>
    public static Material CreateMaterial(string name, Texture2D mainTexture = null)
    {
        // 尝试使用 HDRP Lit Shader
        Shader shader = Shader.Find("HDRP/Lit");
        if (shader == null)
        {
            // 回退到标准 Shader
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            Debug.LogError("[RuntimeAssetLoader] 找不到合适的 Shader");
            return null;
        }

        Material material = new Material(shader);
        material.name = name;

        if (mainTexture != null)
        {
            // HDRP Lit 使用 _BaseColorMap
            if (material.HasProperty("_BaseColorMap"))
            {
                material.SetTexture("_BaseColorMap", mainTexture);
            }
            // Standard 使用 _MainTex
            else if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", mainTexture);
            }
        }

        return material;
    }

    /// <summary>
    /// 从文件夹加载完整的模型资源（OBJ + 贴图）
    /// </summary>
    public static (Mesh mesh, Material material) LoadModelWithMaterial(string objPath, string texturePath = null)
    {
        Mesh mesh = LoadOBJ(objPath);
        if (mesh == null) return (null, null);

        // 如果没有指定贴图路径，尝试查找同名贴图
        if (string.IsNullOrEmpty(texturePath))
        {
            string directory = Path.GetDirectoryName(objPath);
            string baseName = Path.GetFileNameWithoutExtension(objPath);

            // 尝试常见的贴图扩展名
            string[] extensions = { ".png", ".jpg", ".jpeg", ".tga" };
            foreach (var ext in extensions)
            {
                string possiblePath = Path.Combine(directory, baseName + ext);
                if (File.Exists(possiblePath))
                {
                    texturePath = possiblePath;
                    break;
                }
            }
        }

        Texture2D texture = null;
        if (!string.IsNullOrEmpty(texturePath) && File.Exists(texturePath))
        {
            texture = LoadTexture(texturePath);
        }

        Material material = CreateMaterial(mesh.name, texture);

        return (mesh, material);
    }

    #endregion

    #region AssetBundle 加载

    // AssetBundle 缓存
    private static Dictionary<string, AssetBundle> _bundleCache = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// 加载 AssetBundle
    /// </summary>
    public static AssetBundle LoadAssetBundle(string bundlePath)
    {
        if (string.IsNullOrEmpty(bundlePath) || !File.Exists(bundlePath))
        {
            Debug.LogWarning($"[RuntimeAssetLoader] AssetBundle 文件不存在: {bundlePath}");
            return null;
        }

        // 检查缓存
        if (_bundleCache.TryGetValue(bundlePath, out var cached))
        {
            if (cached != null)
                return cached;
            else
                _bundleCache.Remove(bundlePath);
        }

        try
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle != null)
            {
                _bundleCache[bundlePath] = bundle;
                Debug.Log($"[RuntimeAssetLoader] AssetBundle 加载成功: {Path.GetFileName(bundlePath)}");
                return bundle;
            }
            else
            {
                Debug.LogError($"[RuntimeAssetLoader] AssetBundle 加载失败: {bundlePath}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RuntimeAssetLoader] AssetBundle 加载异常: {bundlePath}\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 从 AssetBundle 加载资源
    /// </summary>
    public static T LoadAssetFromBundle<T>(string bundlePath, string assetName) where T : UnityEngine.Object
    {
        AssetBundle bundle = LoadAssetBundle(bundlePath);
        if (bundle == null) return null;

        try
        {
            T asset = bundle.LoadAsset<T>(assetName);
            if (asset != null)
            {
                Debug.Log($"[RuntimeAssetLoader] 从 Bundle 加载资源: {assetName}");
                return asset;
            }
            else
            {
                Debug.LogWarning($"[RuntimeAssetLoader] Bundle 中未找到资源: {assetName}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RuntimeAssetLoader] 从 Bundle 加载资源异常: {assetName}\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 从 AssetBundle 加载所有指定类型的资源
    /// </summary>
    public static T[] LoadAllAssetsFromBundle<T>(string bundlePath) where T : UnityEngine.Object
    {
        AssetBundle bundle = LoadAssetBundle(bundlePath);
        if (bundle == null) return null;

        try
        {
            return bundle.LoadAllAssets<T>();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RuntimeAssetLoader] 从 Bundle 加载所有资源异常: {bundlePath}\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 卸载 AssetBundle
    /// </summary>
    public static void UnloadAssetBundle(string bundlePath, bool unloadAllLoadedObjects = false)
    {
        if (_bundleCache.TryGetValue(bundlePath, out var bundle))
        {
            if (bundle != null)
            {
                bundle.Unload(unloadAllLoadedObjects);
            }
            _bundleCache.Remove(bundlePath);
        }
    }

    /// <summary>
    /// 卸载所有 AssetBundle
    /// </summary>
    public static void UnloadAllAssetBundles(bool unloadAllLoadedObjects = false)
    {
        foreach (var bundle in _bundleCache.Values)
        {
            if (bundle != null)
            {
                bundle.Unload(unloadAllLoadedObjects);
            }
        }
        _bundleCache.Clear();
        Debug.Log("[RuntimeAssetLoader] 所有 AssetBundle 已卸载");
    }

    #endregion

    #region 缓存管理

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    public static void ClearCache()
    {
        foreach (var tex in _textureCache.Values)
        {
            if (tex != null) UnityEngine.Object.Destroy(tex);
        }
        _textureCache.Clear();

        foreach (var mesh in _meshCache.Values)
        {
            if (mesh != null) UnityEngine.Object.Destroy(mesh);
        }
        _meshCache.Clear();

        foreach (var mat in _materialCache.Values)
        {
            if (mat != null) UnityEngine.Object.Destroy(mat);
        }
        _materialCache.Clear();

        Debug.Log("[RuntimeAssetLoader] 缓存已清除");
    }

    /// <summary>
    /// 从缓存中移除指定资源
    /// </summary>
    public static void RemoveFromCache(string filePath)
    {
        if (_textureCache.TryGetValue(filePath, out var tex))
        {
            if (tex != null) UnityEngine.Object.Destroy(tex);
            _textureCache.Remove(filePath);
        }

        if (_meshCache.TryGetValue(filePath, out var mesh))
        {
            if (mesh != null) UnityEngine.Object.Destroy(mesh);
            _meshCache.Remove(filePath);
        }
    }

    #endregion
}
