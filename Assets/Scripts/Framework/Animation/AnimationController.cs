
using UnityEngine;
using System.Linq;
using UnityEngine.U2D;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

public class AnimationController : MonoBehaviour {

    public SpriteAtlas[] atlas;
    public string imagename;
    public Sprite[] images
    {
        get
        {
            if (allImages.ContainsKey(imagename))
            {
                return allImages[imagename];
            }
            else
            {
                Debug.LogError(Equals(imagename));
                return null;
            }
        }
    }

    public Dictionary<string, Sprite[]> allImages = new Dictionary<string, Sprite[]>();
    public float frameTime = 0.03f;
    public int order;
    /// <summary>
    /// 播放次数
    /// </summary>
    public int playTimes = 0;
#pragma warning disable CS0414
    private float Logic_accumulatedTime;
#pragma warning restore CS0414
    public bool loop = true;
    public bool looped = false;
    /**
     * 是否倒播
     */
    public bool reverse = false;

    public bool autoPlayOnLoad = false;

    /// <summary>
    /// 播放完自动销毁
    /// </summary>
    public bool autoDestroy = false;

    public int frameNum = 0;

    public int frameIndex = 0;

    public int nextFrameIndex = 0;

    private float accumulatedTime = 0f;

    private bool running = false;

    public Image m_render = null;

    private float time = 0;

   

    private int currentTimes = 0;

    private RectTransform uiTransform = null;

    private SpriteSplitter spriteSplitter;
    public void SetSprite(string name , int num=0)
    {
        stopPlay();
        m_render.sprite = allImages[name][num];
        SetAinmationNormalRect();
    }

	// Use this for initialization
	 void Awake () {

        //Init();
        
        

	}
    public Sprite spriteToSplit;

    public void Init()
    {
        this.spriteSplitter = new SpriteSplitter();
        this.uiTransform = this.GetComponent<RectTransform>();
        this.m_render = this.GetComponent<Image>();
        if (this.atlas.Length > 0)
        {
            foreach (var atla in this.atlas)
            {
                var ima = new Sprite[atla.spriteCount];
                atla.GetSprites(ima);
                ima = ima.OrderBy(sprite => {
                    // 从精灵名称中提取数字
                    var match = Regex.Match(sprite.name, @"\d+");
                    return match.Success ? int.Parse(match.Value) : 0;
                }).ToArray();
                allImages.Add(atla.name, ima);
            }
        }
        if (spriteToSplit!=null)
        {
            var sprites = spriteSplitter.SplitSprite(spriteToSplit).ToArray();
            allImages.TryAdd(spriteToSplit.name, sprites);
        }
        imagename = allImages.Keys.ElementAt(0);
        if (this.images.Length != 0)
        {
            this.frameNum = this.images.Length;
        }
        
        //this.time = this.frameTime;
        this.running = this.autoPlayOnLoad;

        if (this.reverse)
        {
            this.frameIndex = this.frameNum - 1;
            this.nextFrameIndex = this.frameNum - 1;
        }
    }
	// Update is called once per frame
    public void Update ()
    {


        float dt = Time.deltaTime;
        ImageUpdate(dt);
    }

    public void ImageUpdate(float dt)
    {

        if (!this.running)
        {
            return;
        }



        if (this.images.Length == 0)
            return;
        accumulatedTime += dt;
        this.time -= dt;

        if (this.playTimes != 0 && this.currentTimes == this.playTimes)
        {
            this.running = false;
            return;
        }


        if (this.time <= 0)
        {
            this.time = this.frameTime;
            var Index = (int)Math.Ceiling(accumulatedTime / this.frameTime);
            var nowIndex = this.frameIndex;
            if (!this.reverse)
            {


                this.frameIndex = Index % this.frameNum;
                if (nowIndex > this.frameIndex)
                {
                    this.looped = true;

                    if (!this.loop)
                    {
                        this.frameIndex = frameNum - 1;
                    }
                }
                if (this.frameIndex < 0)
                {
                    this.frameIndex = 0;
                }
                //this.nextFrameIndex = this.frameIndex + 1;

                this.m_render.sprite = this.images[this.frameIndex];

                if (this.m_render.sprite)
                {
                    SetAinmationNormalRect();
                }


                if (this.frameIndex == this.frameNum - 1)
                {
                    this.currentTimes++;


                    //frameTime = 0.03f;
                    if (this.playTimes != 0 && this.currentTimes == this.playTimes)
                    {
                        if (this.autoDestroy)
                        {
                            GameObject.Destroy(this.gameObject);
                        }
                    }
                }
            }

            else
            {
                this.frameIndex = this.frameNum - 1 - (Index % this.frameNum);
                if (nowIndex < this.frameIndex)
                {
                    this.looped = true;
                    if (!this.loop)
                    {
                        this.frameIndex = 0;
                    }
                    this.frameIndex = (this.nextFrameIndex + this.frameNum) % this.frameNum;
                    this.nextFrameIndex = this.frameIndex - 1;

                    this.m_render.sprite = this.images[this.frameIndex];

                    if (this.m_render.sprite)
                    {
                        SetAinmationNormalRect();
                    }


                    if (this.frameIndex == 0)
                    {
                        this.currentTimes++;

                        //frameTime = 0.03f;
                        if (this.playTimes != 0 && this.currentTimes == this.playTimes)
                        {



                            if (this.autoDestroy)
                            {
                                GameObject.Destroy(this.gameObject);
                            }
                        }
                    }

                }
            }
        }
    }
    public void SetAinmationNormalRect()
    {

        Rect rect = this.m_render.sprite.rect;
        Vector2 pivotInPixels = m_render.sprite.pivot;

        // 将pivot转换为归一化坐标（相对于Sprite的尺寸）
        Vector2 normalizedPivot = new Vector2(pivotInPixels.x / rect.width, pivotInPixels.y / rect.height);

        // 设置RectTransform的pivot来匹配Sprite的pivot
        //uiTransform.pivot = normalizedPivot;

        this.uiTransform.sizeDelta = new Vector2(rect.width, rect.height);
    }
    public void PlayAnimation(string name)
    {
        if (this.allImages.ContainsKey(name))
        {
            imagename = name;
            PlayAnimation();

        }

    }
    /// <summary>
    /// 播放
    /// </summary>
    /// 
    public void PlayAnimation(string name,float totaltime)
    {
        if (this.allImages.ContainsKey(name))
        {
            imagename = name;
            frameTime = totaltime / this.allImages[name].Length;
            PlayAnimation();
            
        }
    }
    public void PlayAnimation()
    {
        this.running = true;
        this.frameIndex = 0;
        this.currentTimes = 0;
        
        this.time = this.frameTime;
        this.Logic_accumulatedTime = 0;
        this.accumulatedTime = 0;
        this.looped= false;
        if (this.images.Length != 0)
        {
            this.frameNum = this.images.Length;

            if(this.reverse)
            {
                this.frameIndex = this.frameNum - 1;
                this.nextFrameIndex = this.frameNum - 1;
            }

        }

        if(!this.m_render)
        {
            this.m_render = this.GetComponent<Image>();
        }

        if (this.m_render)
            this.m_render.sprite = this.images[0];


        if (this.m_render.sprite)
        {
            SetAinmationNormalRect();
        }

    }

    public void gotoAndPlay(int frameIndex)
    {
        if(!this.m_render)
        {
            this.m_render = this.GetComponent<Image>();
        }

        this.running = true;
        this.frameIndex = frameIndex;
        this.nextFrameIndex = frameIndex;
        this.currentTimes = 0;
        //this.time = 0;
    }
    public void SetForward()
    {
        transform.localScale = new Vector3(1, 1, 1);
    }
    public void SetBackward()
    {
        transform.localScale = new Vector3(-1, 1, 1);
    }
    public Vector2 GetSize()
    {
        if (this.m_render.sprite)
        {
            return new Vector2(this.m_render.sprite.rect.width, this.m_render.sprite.rect.height);
        }
        else
        {
            return Vector2.zero;
        }
    }
    public void startPlay()
    {
        this.running = false;
    }
    /// <summary>
    /// 停止
    /// </summary>
    public void stopPlay()
    {
        this.running = false;
    }

    public void gotoAndStop(int frameIndex)
    {
        this.frameIndex = frameIndex;

        if (this.frameIndex < 0)
            this.frameIndex = 0;

        if (this.frameIndex > this.images.Length - 1)
            this.frameIndex = this.images.Length - 1;

        if(!this.m_render)
        {
            this.m_render = this.GetComponent<Image>();
        }

        this.m_render.sprite = this.images[this.frameIndex];

        if(this.m_render.sprite)
        {
            Rect rect = this.m_render.sprite.rect;
            this.uiTransform.sizeDelta = new Vector2(rect.width, rect.height);
        }

        this.running = false;
    }

    public bool isPlayEnd()
    {
        return this.frameIndex == this.frameNum;
    }

}

public class SpriteSplitter 
{
    [Header("Sprite Settings")]
    public Sprite spriteToSplit; // 原始精灵
    public bool processOnStart = true; // 启动时自动处理
    public bool createObjects = true; // 是否创建GameObject

    [Header("Result Settings")]
    public string segmentPrefix = "Segment"; // 分割后的对象名称前缀
    public Material segmentMaterial; // 分割后对象的材质（可选）

    private List<Sprite> segments = new List<Sprite>();
    private List<GameObject> segmentObjects = new List<GameObject>();

    //void Start()
    //{
    //    if (processOnStart && spriteToSplit != null)
    //    {
    //        SplitSprite(spriteToSplit);
    //    }
    //}

    /// <summary>
    /// 将Sprite分割成8个水平等分部分
    /// </summary>
    private Texture2D GetReadableTexture(Sprite sprite)
    {
        // 如果纹理已经可读，直接返回
        if (sprite.texture.isReadable)
        {
            return sprite.texture;
        }

        // 创建临时RenderTexture进行复制
        RenderTexture renderTexture = RenderTexture.GetTemporary(
            sprite.texture.width,
            sprite.texture.height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.sRGB
        );

        // 复制纹理到RenderTexture
        Graphics.Blit(sprite.texture, renderTexture);

        // 保存当前活动RenderTexture
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        // 创建新的可读Texture2D
        Texture2D readableTexture = new Texture2D(
            (int)sprite.rect.width,
            (int)sprite.rect.height,
            TextureFormat.ARGB32,
            false
        );

        // 读取所需区域的像素数据
        readableTexture.ReadPixels(
            new Rect(
                sprite.textureRect.x,
                sprite.textureRect.y,
                sprite.textureRect.width,
                sprite.textureRect.height
            ),
            0,
            0
        );
        readableTexture.Apply();

        // 恢复RenderTexture状态
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);

        return readableTexture;
    }

    public List<Sprite>  SplitSprite(Sprite sprite)
    {
        // 清除之前的结果
        ClearSegments();

        if (sprite == null)
        {
            Debug.LogWarning("No sprite to split!");
            return null;
        }

        // 获取原始纹理
        Texture2D originalTexture = GetReadableTexture(sprite);
        if (originalTexture == null)
        {
            Debug.LogError("Sprite texture is null!");
            return null;
        }

        // 计算每个片段的宽度
        float segmentWidth = sprite.textureRect.width / 8;
        float height = sprite.textureRect.height;

        if (segmentWidth <= 0)
        {
            Debug.LogError("Sprite width is too small for 8 segments!");
            return null;
        }
        // 获取原始Sprite的pivot点（基于原始Sprite的像素尺寸）
        Vector2 originalPivot = sprite.pivot;
        originalPivot.x -= sprite.textureRect.x;
        originalPivot.y -= sprite.textureRect.y;

        // 创建分割后的Sprite
        for (int i = 0; i < 8; i++)
        {
            // 计算当前片段的纹理位置
            Rect segmentRect = new Rect(
                sprite.textureRect.x + i * segmentWidth,
                sprite.textureRect.y,
                segmentWidth,
                height
            );

            // 创建纹理片段
            Texture2D segmentTexture = new Texture2D((int)segmentWidth, (int)height, TextureFormat.RGBA32, false);
            segmentTexture.filterMode = FilterMode.Point;

            // 复制像素数据
            Color[] pixels = originalTexture.GetPixels(
                (int)segmentRect.x,
                (int)segmentRect.y,
                (int)segmentRect.width,
                (int)segmentRect.height
            );

            segmentTexture.SetPixels(pixels);
            segmentTexture.Apply();

            // 计算新Sprite的pivot（基于片段自身尺寸）
            Vector2 segmentPivot = new Vector2(
                originalPivot.x - i * segmentWidth,
                originalPivot.y
            );

            // 将pivot转换为归一化坐标
            segmentPivot.x /= segmentWidth;
            segmentPivot.y /= height;

            // 创建Sprite
            Sprite segmentSprite = Sprite.Create(
                segmentTexture,
                new Rect(0, 0, segmentWidth, height),
                segmentPivot,
                sprite.pixelsPerUnit
            );

            segmentSprite.name = $"{segmentPrefix}_{i}";
            segments.Add(segmentSprite);

            // 如果需要，创建GameObject
            //if (createObjects)
            //{
            //    CreateSegmentObject(segmentSprite, i);
            //}
        }
        return segments;
        
    }

    /// <summary>
    /// 为分割后的Sprite创建GameObject
    /// </summary>
    //private void CreateSegmentObject(Sprite sprite, int index)
    //{
    //    GameObject segmentObj = new GameObject($"{segmentPrefix}_{index}");
    //    segmentObj.transform.position = transform.position + new Vector3(index * sprite.rect.width / sprite.pixelsPerUnit, 0, 0);
    //    segmentObj.transform.SetParent(transform);

    //    SpriteRenderer renderer = segmentObj.AddComponent<SpriteRenderer>();
    //    renderer.sprite = sprite;

    //    if (segmentMaterial != null)
    //    {
    //        renderer.material = segmentMaterial;
    //    }

    //    segmentObjects.Add(segmentObj);
    //}

    /// <summary>
    /// 清除所有分割结果
    /// </summary>
    public void ClearSegments()
    {
        // 销毁所有GameObject
        //foreach (GameObject obj in segmentObjects)
        //{
        //    if (obj != null) Destroy(obj);
        //}
        //segmentObjects.Clear();

        // 销毁所有Sprite的纹理
        foreach (Sprite segment in segments)
        {
            if (segment != null && segment.texture != null)
            {
               MonoBehaviour.Destroy(segment.texture);
            }
        }
        segments.Clear();
    }

    /// <summary>
    /// 从文件路径加载精灵
    /// </summary>
    public void LoadSpriteFromPath(string path)
    {
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            spriteToSplit = sprite;
            SplitSprite(sprite);
        }
        else
        {
            Debug.LogError($"Failed to load sprite from path: {path}");
        }
    }

    /// <summary>
    /// 获取所有分割后的Sprite
    /// </summary>
    public List<Sprite> GetSegments()
    {
        return segments;
    }

    /// <summary>
    /// 获取所有分割后的GameObject
    /// </summary>
    public List<GameObject> GetSegmentObjects()
    {
        return segmentObjects;
    }

    //void OnDestroy()
    //{
    //    ClearSegments();
    //}
}
