using UnityEngine;
using UnityEngine.UI;

public class SeamlessScroller : MonoBehaviour
{
    public Image[] images; // 在Inspector中拖入两个相同的UI Image
    public float scrollSpeed = 100f; // 移动速度（像素/秒）
    public Direction scrollDirection = Direction.Left; // 移动方向
    public RectTransform alignmentTarget; // 用于左侧对齐的参考RectTransform

    public enum Direction { Left, Right, Up, Down }

    private RectTransform[] rectTransforms;
    private Vector2[] originalSizes;
    private float imageWidth;
    private float imageHeight;
    public bool isScrolling = false; // 控制滚动状态
    private Vector2[] initialPositions; // 存储初始位置

    void Start()
    {
        InitializeScroller();
    }

    // 初始化滚动器
    public void InitializeScroller()
    {
        // 确保至少有两个图片
        if (images == null || images.Length < 2)
        {
            Debug.LogError("需要至少两个Image对象");
            return;
        }

        rectTransforms = new RectTransform[images.Length];
        originalSizes = new Vector2[images.Length];
        initialPositions = new Vector2[images.Length];

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null) continue;

            rectTransforms[i] = images[i].GetComponent<RectTransform>();

            if (images[i].sprite != null)
            {
                // 记录纹理原始尺寸但不应用
                Vector2 textureSize = new Vector2(
                    images[i].sprite.texture.width,
                    images[i].sprite.texture.height
                );

                // 保留当前实际尺寸
                originalSizes[i] = rectTransforms[i].sizeDelta;

                // 仅记录纹理尺寸供参考（可选）
                // textureSizes[i] = textureSize; 
            }
            else
            {
                Debug.LogWarning($"Image {i} 未分配Sprite");
                originalSizes[i] = rectTransforms[i].sizeDelta;
            }
        }

        // 使用第一个图片的当前实际尺寸作为基准
        imageWidth = originalSizes[0].x; // 实际显示宽度
        imageHeight = originalSizes[0].y; // 实际显示高度

        // 对齐到目标RectTransform的左侧
        AlignToTarget();

        // 保存初始位置
        for (int i = 0; i < rectTransforms.Length; i++)
        {
            if (rectTransforms[i] != null)
            {
                initialPositions[i] = rectTransforms[i].anchoredPosition;
            }
        }

        // 初始化位置（第二个图片紧接在第一个后面）
        PositionSecondImage();
    }

    // 对齐到目标RectTransform的左侧
    private void AlignToTarget()
    {
        if (alignmentTarget == null)
        {
            Debug.LogWarning("未指定对齐目标，使用默认位置");
            return;
        }

        // 获取目标RectTransform的左边界位置
        Vector3 targetLeftEdge = GetLeftEdgeWorldPosition(alignmentTarget);

        // 将第一个图片对齐到目标左侧
        RectTransform firstImageRT = rectTransforms[0];
        Vector3 firstImageLeftEdge = GetLeftEdgeWorldPosition(firstImageRT);

        // 计算偏移量并应用
        Vector3 offset = targetLeftEdge - firstImageLeftEdge;

        // 获取当前Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            // 世界空间Canvas - 直接应用世界坐标偏移
            Vector3 newPosition = firstImageRT.position + new Vector3(offset.x, 0, 0);// + offset;
            firstImageRT.position = newPosition;
        }
        else
        {
            // 屏幕空间Canvas - 使用anchoredPosition
            // 将世界坐标偏移转换为局部坐标偏移
            Vector2 localOffset = firstImageRT.InverseTransformVector(offset);
            firstImageRT.anchoredPosition += new Vector2(localOffset.x, 0); // + localOffset;
        }
    }

    // 获取RectTransform的左边界世界坐标
    private Vector3 GetLeftEdgeWorldPosition(RectTransform rt)
    {
        // 根据轴心点计算左边界位置
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        // 左下角是corners[0]，左上角是corners[1]
        // 取左右两个点的中间值作为左边界位置
        Vector3 leftEdge = (corners[0] + corners[1]) / 2;
        return leftEdge;
    }

    // 定位第二个图片
    private void PositionSecondImage()
    {
        if (rectTransforms == null || rectTransforms.Length < 2) return;

        switch (scrollDirection)
        {
            case Direction.Left:
            case Direction.Right:
                rectTransforms[1].anchoredPosition = new Vector2(
                    rectTransforms[0].anchoredPosition.x + imageWidth,
                    rectTransforms[0].anchoredPosition.y
                );
                break;
            case Direction.Up:
            case Direction.Down:
                rectTransforms[1].anchoredPosition = new Vector2(
                    rectTransforms[0].anchoredPosition.x,
                    rectTransforms[0].anchoredPosition.y - imageHeight
                );
                break;
        }
    }
    public void StartScroll()
    {
        isScrolling = true;
    }

    public void StopScroll()
    {
        isScrolling = false;
    }
    //public void SetScrollSpeed(float speed)
    //{
    //    scrollSpeed = Mathf.Max(0, speed); // 确保速度不为负
    //}
    public void SetScrollDirection(Direction direction)
    {
        scrollDirection = direction;
    }
    void Update()
    {
        if (!isScrolling) return; // 如果停止状态则不移动

        Vector2 moveVector = Vector2.zero;

        // 根据方向设置移动向量
        switch (scrollDirection)
        {
            case Direction.Left: moveVector = Vector2.left; break;
            case Direction.Right: moveVector = Vector2.right; break;
            case Direction.Up: moveVector = Vector2.up; break;
            case Direction.Down: moveVector = Vector2.down; break;
        }

        // 移动所有图片
        foreach (RectTransform rt in rectTransforms)
        {
            if (rt != null)
            {
                rt.anchoredPosition += moveVector * scrollSpeed * Time.deltaTime;
            }
        }

        // 检查是否需要重置位置
        CheckResetPosition();
    }
    public float Test; // 控制重置阈值的偏移量

    void CheckResetPosition()
    {
        for (int i = 0; i < rectTransforms.Length; i++)
        {
            if (rectTransforms[i] == null) continue;

            Vector2 pos = rectTransforms[i].anchoredPosition;

            switch (scrollDirection)
            {
                case Direction.Left:
                    // 添加 Test 作为偏移量
                    if (pos.x < -imageWidth - Test)
                        pos.x += imageWidth * 2;
                    break;

                case Direction.Right:
                    // 添加 Test 作为偏移量
                    if (pos.x > imageWidth + Test)
                        pos.x -= imageWidth * 2;
                    break;

                case Direction.Up:
                    // 添加 Test 作为偏移量
                    if (pos.y > imageHeight + Test)
                        pos.y -= imageHeight * 2;
                    break;

                case Direction.Down:
                    // 添加 Test 作为偏移量
                    if (pos.y < -imageHeight - Test)
                        pos.y += imageHeight * 2;
                    break;
            }

            rectTransforms[i].anchoredPosition = pos;
        }
    }


    // ================= 新增控制方法 =================

    /// <summary>
    /// 重置滚动器到初始状态
    /// </summary>
    public void Reset()
    {
        // 重置位置
        if (initialPositions != null && rectTransforms != null)
        {
            for (int i = 0; i < rectTransforms.Length; i++)
            {
                if (rectTransforms[i] != null)
                {
                    rectTransforms[i].anchoredPosition = initialPositions[i];
                }
            }
        }

        // 重新对齐
        AlignToTarget();

        // 重新定位第二个图片
        PositionSecondImage();

        // 重新开始滚动
        //StartScrolling();
    }

    /// <summary>
    /// 停止滚动
    /// </summary>
    public void Stop()
    {
        isScrolling = false;
    }

    /// <summary>
    /// 开始滚动
    /// </summary>
    public void StartScrolling()
    {
        isScrolling = true;
    }

    /// <summary>
    /// 设置滚动速度
    /// </summary>
    /// <param name="speed">新的滚动速度（像素/秒）</param>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = Mathf.Max(0, speed); // 确保速度不为负
    }

    // ================= 编辑器辅助方法 =================

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (alignmentTarget != null && images != null && images.Length > 0 && images[0] != null)
        {
            RectTransform firstImageRT = images[0].GetComponent<RectTransform>();
            if (firstImageRT == null) return;

            // 绘制目标左侧位置线
            Vector3 targetLeft = GetLeftEdgeWorldPosition(alignmentTarget);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(targetLeft + Vector3.up * 100, targetLeft + Vector3.down * 100);

            // 绘制图片当前左侧位置线
            Vector3 imageLeft = GetLeftEdgeWorldPosition(firstImageRT);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(imageLeft + Vector3.up * 100, imageLeft + Vector3.down * 100);
        }
    }
#endif
}
