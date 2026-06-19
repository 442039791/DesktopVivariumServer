

using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using Common;

public class TextureLoader
{
    private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private static Dictionary<string, int> textureReferenceCount = new Dictionary<string, int>();

    public async Task<Texture2D> LoadTextureAsync(string filePath)
    {
        if (textureCache.ContainsKey(filePath))
        {
            textureReferenceCount[filePath]++;
            return textureCache[filePath];
        }

        if (File.Exists(filePath))
        {
            try
            {
                byte[] fileData = await File.ReadAllBytesAsync(filePath);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(fileData))
                {
                    textureCache[filePath] = texture;
                    textureReferenceCount[filePath] = 1;
                    return texture;
                }
                else
                {
                    Debug.LogError("Failed to load texture: " + filePath);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Exception while loading texture: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Texture file not found: " + filePath);
        }
        return null;
    }

    public static void UnloadTexture(string filePath)
    {
        if (textureCache.ContainsKey(filePath))
        {
            textureReferenceCount[filePath]--;
            if (textureReferenceCount[filePath] <= 0)
            {
                Texture2D texture = textureCache[filePath];
                textureCache.Remove(filePath);
                textureReferenceCount.Remove(filePath);
                Object.Destroy(texture);
            }
        }
    }

    public void Reset()
    {
        // 清理缓存或重置状态的逻辑可以在这里实现
    }
}

public class TextureLoaderPool:MonoSingleton<TextureLoaderPool>
{
    private Queue<TextureLoader> pool;

    public TextureLoaderPool(int initialSize = 5)
    {
        pool = new Queue<TextureLoader>();
        for (int i = 0; i < initialSize; i++)
        {
            pool.Enqueue(new TextureLoader());
        }
    }

    public TextureLoader GetLoader()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        else
        {
            return new TextureLoader();
        }
    }

    public void ReturnLoader(TextureLoader loader)
    {
        loader.Reset();
        pool.Enqueue(loader);
    }
}

/*
string texturePath = PathName.Combine(modsPath, "Textures", "UserTexture.png");
TextureLoader textureLoader = new TextureLoader();
Texture2D userTexture = await textureLoader.LoadTextureAsync(texturePath);
if (userTexture != null)
{
    Renderer renderer = yourGameObject.GetComponent<Renderer>();
    renderer.material.mainTexture = userTexture;
}





 */