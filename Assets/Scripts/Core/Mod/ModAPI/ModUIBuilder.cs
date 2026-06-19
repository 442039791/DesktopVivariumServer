using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using SUIFW;

/// <summary>
/// 模组 UI 构建器
/// 提供简洁的链式 API 来构建 UI 界面，无需手动处理 RectTransform 等复杂设置
///
/// 使用示例：
/// var panel = ModUIBuilder.CreatePanel("MyPanel", 400, 300)
///     .WithBackground(new Color(0.1f, 0.1f, 0.1f, 0.95f))
///     .WithTitle("我的面板")
///     .AddButton("按钮1", () => Debug.Log("点击了按钮1"))
///     .AddButton("关闭", () => context.UIServices.CloseUI("MyPanel"))
///     .AddText("这是一段说明文字")
///     .Build(parent);
/// </summary>
public class ModUIBuilder
{
    private string _name;
    private Vector2 _size;
    private Vector2 _position;
    private Color _backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    private string _title;
    private float _titleFontSize = 22f;
    private bool _showCloseButton = false;
    private Action _onCloseClick;
    private bool _isDraggable = false;

    private List<UIElementDefinition> _elements = new List<UIElementDefinition>();

    // UI 元素定义
    private abstract class UIElementDefinition
    {
        public abstract void Build(Transform parent, ModUIBuilder builder);
    }

    #region 静态工厂方法

    /// <summary>
    /// 创建一个面板
    /// </summary>
    /// <param name="name">面板名称</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    public static ModUIBuilder CreatePanel(string name, float width = 400, float height = 300)
    {
        return new ModUIBuilder
        {
            _name = name,
            _size = new Vector2(width, height),
            _position = Vector2.zero
        };
    }

    /// <summary>
    /// 创建一个弹窗
    /// </summary>
    public static ModUIBuilder CreatePopup(string name, float width = 350, float height = 200)
    {
        return new ModUIBuilder
        {
            _name = name,
            _size = new Vector2(width, height),
            _position = Vector2.zero,
            _backgroundColor = new Color(0.15f, 0.15f, 0.2f, 0.98f),
            _showCloseButton = true
        };
    }

    /// <summary>
    /// 创建一个 HUD 元素（固定位置）
    /// </summary>
    public static ModUIBuilder CreateHUD(string name, float width = 200, float height = 100)
    {
        return new ModUIBuilder
        {
            _name = name,
            _size = new Vector2(width, height),
            _position = Vector2.zero,
            _backgroundColor = new Color(0, 0, 0, 0.7f)
        };
    }

    #endregion

    #region 链式配置方法

    /// <summary>
    /// 设置背景颜色
    /// </summary>
    public ModUIBuilder WithBackground(Color color)
    {
        _backgroundColor = color;
        return this;
    }

    /// <summary>
    /// 设置位置（相对于屏幕中心的偏移）
    /// </summary>
    public ModUIBuilder AtPosition(float x, float y)
    {
        _position = new Vector2(x, y);
        return this;
    }

    /// <summary>
    /// 设置位置（使用预设锚点）
    /// </summary>
    public ModUIBuilder AtPosition(UIAnchor anchor, float offsetX = 0, float offsetY = 0)
    {
        // 位置将在 Build 时根据锚点计算
        _elements.Add(new AnchorDefinition { Anchor = anchor, OffsetX = offsetX, OffsetY = offsetY });
        return this;
    }

    /// <summary>
    /// 添加标题
    /// </summary>
    public ModUIBuilder WithTitle(string title, float fontSize = 22f)
    {
        _title = title;
        _titleFontSize = fontSize;
        return this;
    }

    /// <summary>
    /// 添加关闭按钮
    /// </summary>
    public ModUIBuilder WithCloseButton(Action onClick = null)
    {
        _showCloseButton = true;
        _onCloseClick = onClick;
        return this;
    }

    /// <summary>
    /// 启用拖拽移动
    /// </summary>
    public ModUIBuilder Draggable()
    {
        _isDraggable = true;
        return this;
    }

    #endregion

    #region 添加 UI 元素

    /// <summary>
    /// 添加按钮
    /// </summary>
    public ModUIBuilder AddButton(string text, Action onClick, ButtonStyle style = null)
    {
        _elements.Add(new ButtonDefinition
        {
            Text = text,
            OnClick = onClick,
            Style = style ?? ButtonStyle.Default
        });
        return this;
    }

    /// <summary>
    /// 添加文本
    /// </summary>
    public ModUIBuilder AddText(string text, float fontSize = 14f, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        _elements.Add(new TextDefinition
        {
            Text = text,
            FontSize = fontSize,
            Alignment = alignment
        });
        return this;
    }

    /// <summary>
    /// 添加动态文本（可通过返回的 setter 更新）
    /// </summary>
    public ModUIBuilder AddDynamicText(string initialText, out Action<string> textSetter, float fontSize = 14f)
    {
        var def = new DynamicTextDefinition
        {
            Text = initialText,
            FontSize = fontSize
        };
        _elements.Add(def);
        textSetter = def.SetText;
        return this;
    }

    /// <summary>
    /// 添加输入框
    /// </summary>
    public ModUIBuilder AddInputField(string placeholder, Action<string> onValueChanged, out Func<string> getValue)
    {
        var def = new InputFieldDefinition
        {
            Placeholder = placeholder,
            OnValueChanged = onValueChanged
        };
        _elements.Add(def);
        getValue = def.GetValue;
        return this;
    }

    /// <summary>
    /// 添加滑动条
    /// </summary>
    public ModUIBuilder AddSlider(string label, float min, float max, float defaultValue, Action<float> onValueChanged)
    {
        _elements.Add(new SliderDefinition
        {
            Label = label,
            Min = min,
            Max = max,
            DefaultValue = defaultValue,
            OnValueChanged = onValueChanged
        });
        return this;
    }

    /// <summary>
    /// 添加开关
    /// </summary>
    public ModUIBuilder AddToggle(string label, bool defaultValue, Action<bool> onValueChanged)
    {
        _elements.Add(new ToggleDefinition
        {
            Label = label,
            DefaultValue = defaultValue,
            OnValueChanged = onValueChanged
        });
        return this;
    }

    /// <summary>
    /// 添加下拉框
    /// </summary>
    public ModUIBuilder AddDropdown(string label, List<string> options, int defaultIndex, Action<int> onValueChanged)
    {
        _elements.Add(new DropdownDefinition
        {
            Label = label,
            Options = options,
            DefaultIndex = defaultIndex,
            OnValueChanged = onValueChanged
        });
        return this;
    }

    /// <summary>
    /// 添加分隔线
    /// </summary>
    public ModUIBuilder AddSeparator(float height = 2f)
    {
        _elements.Add(new SeparatorDefinition { Height = height });
        return this;
    }

    /// <summary>
    /// 添加间距
    /// </summary>
    public ModUIBuilder AddSpace(float height = 10f)
    {
        _elements.Add(new SpaceDefinition { Height = height });
        return this;
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    public ModUIBuilder AddImage(Sprite sprite, float width = 100, float height = 100)
    {
        _elements.Add(new ImageDefinition
        {
            Sprite = sprite,
            Width = width,
            Height = height
        });
        return this;
    }

    /// <summary>
    /// 开始水平布局组
    /// </summary>
    public ModUIBuilder BeginHorizontal(float spacing = 10f)
    {
        _elements.Add(new HorizontalGroupDefinition { Spacing = spacing, IsStart = true });
        return this;
    }

    /// <summary>
    /// 结束水平布局组
    /// </summary>
    public ModUIBuilder EndHorizontal()
    {
        _elements.Add(new HorizontalGroupDefinition { IsStart = false });
        return this;
    }

    /// <summary>
    /// 添加自定义元素
    /// </summary>
    public ModUIBuilder AddCustom(Action<Transform> builder)
    {
        _elements.Add(new CustomDefinition { Builder = builder });
        return this;
    }

    #endregion

    #region 构建

    /// <summary>
    /// 构建 UI 并返回根对象
    /// </summary>
    public GameObject Build(Transform parent)
    {
        // 创建根面板
        var panelGO = CreateRootPanel();

        // 创建内容容器
        var contentContainer = CreateContentContainer(panelGO.transform);

        // 添加标题
        if (!string.IsNullOrEmpty(_title))
        {
            CreateTitle(panelGO.transform);
        }

        // 添加关闭按钮
        if (_showCloseButton)
        {
            CreateCloseButton(panelGO.transform);
        }

        // 添加拖拽功能
        if (_isDraggable)
        {
            AddDragHandler(panelGO);
        }

        // 构建所有元素
        Transform currentParent = contentContainer;
        GameObject currentHorizontalGroup = null;

        foreach (var element in _elements)
        {
            if (element is HorizontalGroupDefinition hGroup)
            {
                if (hGroup.IsStart)
                {
                    currentHorizontalGroup = CreateHorizontalGroup(currentParent, hGroup.Spacing);
                    currentParent = currentHorizontalGroup.transform;
                }
                else
                {
                    currentParent = contentContainer;
                    currentHorizontalGroup = null;
                }
            }
            else if (element is AnchorDefinition)
            {
                // 锚点设置在最后处理
            }
            else
            {
                element.Build(currentParent, this);
            }
        }

        // 处理锚点定位
        foreach (var element in _elements)
        {
            if (element is AnchorDefinition anchor)
            {
                ApplyAnchor(panelGO.GetComponent<RectTransform>(), anchor);
            }
        }

        // 设置父节点
        if (parent != null)
        {
            panelGO.transform.SetParent(parent, false);
        }

        // 添加 BaseUIForms 组件
        var uiForm = panelGO.GetComponent<BaseUIForms>();
        if (uiForm == null)
        {
            uiForm = panelGO.AddComponent<ModBuiltUIForm>();
        }

        return panelGO;
    }

    private GameObject CreateRootPanel()
    {
        var panelGO = new GameObject(_name);
        var rectTransform = panelGO.AddComponent<RectTransform>();

        rectTransform.sizeDelta = _size;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = _position;

        var image = panelGO.AddComponent<Image>();
        image.color = _backgroundColor;

        // 添加圆角效果（如果需要可以使用自定义 shader）

        return panelGO;
    }

    private Transform CreateContentContainer(Transform parent)
    {
        var containerGO = new GameObject("Content");
        var rectTransform = containerGO.AddComponent<RectTransform>();
        rectTransform.SetParent(parent, false);

        // 留出标题和边距的空间
        float topMargin = string.IsNullOrEmpty(_title) ? 10f : 45f;
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = new Vector2(15, 15);
        rectTransform.offsetMax = new Vector2(-15, -topMargin);

        // 添加垂直布局
        var layout = containerGO.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(5, 5, 5, 5);

        // 添加 ContentSizeFitter
        var fitter = containerGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return containerGO.transform;
    }

    private void CreateTitle(Transform parent)
    {
        var titleGO = new GameObject("Title");
        var rectTransform = titleGO.AddComponent<RectTransform>();
        rectTransform.SetParent(parent, false);

        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.anchoredPosition = new Vector2(0, -8);
        rectTransform.sizeDelta = new Vector2(0, 30);

        var text = titleGO.AddComponent<TextMeshProUGUI>();
        text.text = _title;
        text.fontSize = _titleFontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
    }

    private void CreateCloseButton(Transform parent)
    {
        var buttonGO = new GameObject("CloseButton");
        var rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.SetParent(parent, false);

        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-8, -8);
        rectTransform.sizeDelta = new Vector2(24, 24);

        var image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.8f, 0.3f, 0.3f, 1f);

        var button = buttonGO.AddComponent<Button>();
        button.targetGraphic = image;

        var colors = button.colors;
        colors.highlightedColor = new Color(0.9f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.6f, 0.2f, 0.2f, 1f);
        button.colors = colors;

        if (_onCloseClick != null)
        {
            button.onClick.AddListener(() => _onCloseClick());
        }

        // X 文字
        var textGO = new GameObject("X");
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.SetParent(buttonGO.transform, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var xText = textGO.AddComponent<TextMeshProUGUI>();
        xText.text = "×";
        xText.fontSize = 18;
        xText.alignment = TextAlignmentOptions.Center;
        xText.color = Color.white;
    }

    private void AddDragHandler(GameObject panelGO)
    {
        var dragHandler = panelGO.AddComponent<ModUIDragHandler>();
    }

    private GameObject CreateHorizontalGroup(Transform parent, float spacing)
    {
        var groupGO = new GameObject("HorizontalGroup");
        var rectTransform = groupGO.AddComponent<RectTransform>();
        rectTransform.SetParent(parent, false);

        var layout = groupGO.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var layoutElement = groupGO.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 35;

        return groupGO;
    }

    private void ApplyAnchor(RectTransform rectTransform, AnchorDefinition anchor)
    {
        Vector2 anchorPos = Vector2.zero;
        Vector2 pivot = new Vector2(0.5f, 0.5f);

        switch (anchor.Anchor)
        {
            case UIAnchor.TopLeft:
                anchorPos = new Vector2(0, 1);
                pivot = new Vector2(0, 1);
                break;
            case UIAnchor.TopCenter:
                anchorPos = new Vector2(0.5f, 1);
                pivot = new Vector2(0.5f, 1);
                break;
            case UIAnchor.TopRight:
                anchorPos = new Vector2(1, 1);
                pivot = new Vector2(1, 1);
                break;
            case UIAnchor.MiddleLeft:
                anchorPos = new Vector2(0, 0.5f);
                pivot = new Vector2(0, 0.5f);
                break;
            case UIAnchor.Center:
                anchorPos = new Vector2(0.5f, 0.5f);
                pivot = new Vector2(0.5f, 0.5f);
                break;
            case UIAnchor.MiddleRight:
                anchorPos = new Vector2(1, 0.5f);
                pivot = new Vector2(1, 0.5f);
                break;
            case UIAnchor.BottomLeft:
                anchorPos = new Vector2(0, 0);
                pivot = new Vector2(0, 0);
                break;
            case UIAnchor.BottomCenter:
                anchorPos = new Vector2(0.5f, 0);
                pivot = new Vector2(0.5f, 0);
                break;
            case UIAnchor.BottomRight:
                anchorPos = new Vector2(1, 0);
                pivot = new Vector2(1, 0);
                break;
        }

        rectTransform.anchorMin = anchorPos;
        rectTransform.anchorMax = anchorPos;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = new Vector2(anchor.OffsetX, anchor.OffsetY);
    }

    #endregion

    #region 元素定义类

    private class ButtonDefinition : UIElementDefinition
    {
        public string Text;
        public Action OnClick;
        public ButtonStyle Style;

        public override void Build(Transform parent, ModUIBuilder builder)
        {
            var buttonGO = new GameObject("Button_" + Text);
            var rectTransform = buttonGO.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            var layoutElement = buttonGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = Style.Height;
            layoutElement.preferredWidth = Style.Width;
            layoutElement.minWidth = Style.Width;

            var image = buttonGO.AddComponent<Image>();
            image.color = Style.BackgroundColor;

            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = image;

            var colors = button.colors;
            colors.highlightedColor = Style.HoverColor;
            colors.pressedColor = Style.PressedColor;
            button.colors = colors;

            if (OnClick != null)
            {
                button.onClick.AddListener(() => OnClick());
            }

            // 按钮文字
            var textGO = new GameObject("Text");
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.SetParent(buttonGO.transform, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);

            var tmpText = textGO.AddComponent<TextMeshProUGUI>();
            tmpText.text = Text;
            tmpText.fontSize = Style.FontSize;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Style.TextColor;
        }
    }

    private class TextDefinition : UIElementDefinition
    {
        public string Text;
        public float FontSize;
        public TextAlignmentOptions Alignment;

        public override void Build(Transform parent, ModUIBuilder builder)
        {
            var textGO = new GameObject("Text");
            var rectTransform = textGO.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            var layoutElement = textGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = FontSize + 10;

            var text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = Text;
            text.fontSize = FontSize;
            text.alignment = Alignment;
            text.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        }
    }

    private class DynamicTextDefinition : UIElementDefinition
    {
        public string Text;
        public float FontSize;
        private TextMeshProUGUI _textComponent;

        public void SetText(string newText)
        {
            if (_textComponent != null)
            {
                _textComponent.text = newText;
            }
        }

        public override void Build(Transform parent, ModUIBuilder builder)
        {
            var textGO = new GameObject("DynamicText");
            var rectTransform = textGO.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            var layoutElement = textGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = FontSize + 10;

            _textComponent = textGO.AddComponent<TextMeshProUGUI>();
            _textComponent.text = Text;
            _textComponent.fontSize = FontSize;
            _textComponent.alignment = TextAlignmentOptions.Left;
            _textComponent.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        }
    }

    private class InputFieldDefinition : UIElementDefinition
    {
        public string Placeholder;
        public Action<string> OnValueChanged;
        private TMP_InputField _inputField;

        public string GetValue() => _inputField?.text ?? "";

        public override void Build(Transform parent, ModUIBuilder builder)
        {
            var inputGO = new GameObject("InputField");
            var rectTransform = inputGO.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            var layoutElement = inputGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 35;

            var image = inputGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // 文本区域
            var textAreaGO = new GameObject("Text Area");
            var textAreaRect = textAreaGO.AddComponent<RectTransform>();
            textAreaRect.SetParent(inputGO.transform, false);
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 5);
            textAreaRect.offsetMax = new Vector2(-10, -5);

            // 占位符
            var placeholderGO = new GameObject("Placeholder");
            var placeholderRect = placeholderGO.AddComponent<RectTransform>();
            placeholderRect.SetParent(textAreaRect, false);
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;

            var placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
            placeholderText.text = Placeholder;
            placeholderText.fontSize = 14;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.fontStyle = FontStyles.Italic;

            // 输入文本
            var textGO = new GameObject("Text");
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.SetParent(textAreaRect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var inputText = textGO.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 14;
            inputText.color = Color.white;

            // 输入框组件
            _inputField = inputGO.AddComponent<TMP_InputField>();
            _inputField.textViewport = textAreaRect;
            _inputField.textComponent = inputText;
            _inputField.placeholder = placeholderText;

            if (OnValueChanged != null)
            {
                _inputField.onValueChanged.AddListener((value) => OnValueChanged(value));
            }
        }
    }

    private class SliderDefinition : UIElementDefinition
    {
        public string Label;
        public float Min;
        public float Max;
        public float DefaultValue;
        public Action<float> OnValueChanged;

        public override void Build(Transform parent, ModUIBuilder builder)
        {
            var containerGO = new GameObject("Slider_" + Label);
            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.SetParent(parent, false);

            var layoutElement = containerGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 50;

            // 标签
            var labelGO = new GameObject("Label");
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.SetParent(containerGO.transform, false);
            labelRect.anchorMin = new Vector2(0, 0.5f);
            labelRect.anchorMax = new Vector2(0.3f, 1);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = Label;
            labelText.fontSize = 14;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.color = Color.white;

            // 滑动条背景
            var sliderGO = new GameObject("Slider");
            var sliderRect = sliderGO.AddComponent<RectTransform>();
            sliderRect.SetParent(containerGO.transform, false);
            sliderRect.anchorMin = new Vector2(0.35f, 0.3f);
            sliderRect.anchorMax = new Vector2(0.85f, 0.7f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            var bgImage = sliderGO.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            // 填充区域
            var fillAreaGO = new GameObject("Fill Area");
            var fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
            fillAreaRect.SetParent(sliderGO.transform, false);
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            var fillGO = new GameObject("Fill");
            var fillRect = fillGO.AddComponent<RectTransform>();
            fillRect.SetParent(fillAreaRect, false);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var fillImage = fillGO.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.6f, 0.9f, 1f);

            // 滑块
            var handleAreaGO = new GameObject("Handle Slide Area");
            var handleAreaRect = handleAreaGO.AddComponent<RectTransform>();
            handleAreaRect.SetParent(sliderGO.transform, false);
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = Vector2.zero;
            handleAreaRect.offsetMax = Vector2.zero;

            var handleGO = new GameObject("Handle");
            var handleRect = handleGO.AddComponent<RectTransform>();
            handleRect.SetParent(handleAreaRect, false);
            handleRect.sizeDelta = new Vector2(20, 0);

            var handleImage = handleGO.AddComponent<Image>();
            handleImage.color = Color.white;

            // Slider 组件
            var slider = sliderGO.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.minValue = Min;
            slider.maxValue = Max;
            slider.value = DefaultValue;

            // 数值显示
            var valueGO = new GameObject("Value");
            var valueRect = valueGO.AddComponent<RectTransform>();
            valueRect.SetParent(containerGO.transform, false);
            valueRect.anchorMin = new Vector2(0.88f, 0.5f);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;

            var valueText = valueGO.AddComponent<TextMeshProUGUI>();
            valueText.text = DefaultValue.ToString("F1");
            valueText.fontSize = 12;
            valueText.alignment = TextAlignmentOptions.Center;
            valueText.color = Color.white;

            slider.onValueChanged.AddListener((value) =>
            {
                valueText.text = value.ToString("F1");
                OnValueChanged?.Invoke(value);
            });
        }
    }

    private class ToggleDefinition : UIElementDefinition
    {
        public string Label;
        public bool DefaultValue;
        public Action<bool> OnValueChanged;

        public override void Build(Transform parent, ModUIBuilder builder)
        {
            var toggleGO = new GameObject("Toggle_" + Label);
            var toggleRect = toggleGO.AddComponent<RectTransform>();
            toggleRect.SetParent(parent, false);

            var layoutElement = toggleGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 30;

            var horizontalLayout = toggleGO.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 10;
            horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
            horizontalLayout.childControlWidth = false;
            horizontalLayout.childControlHeight = true;

            // 复选框背景
            var checkboxGO = new GameObject("Checkbox");
            var checkboxRect = checkboxGO.AddComponent<RectTransform>();
            checkboxRect.SetParent(toggleGO.transform, false);
            checkboxRect.sizeDelta = new Vector2(24, 24);

            var checkboxImage = checkboxGO.AddComponent<Image>();
            checkboxImage.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            // 勾选标记
            var checkmarkGO = new GameObject("Checkmark");
            var checkmarkRect = checkmarkGO.AddComponent<RectTransform>();
            checkmarkRect.SetParent(checkboxGO.transform, false);
            checkmarkRect.anchorMin = new Vector2(0.15f, 0.15f);
            checkmarkRect.anchorMax = new Vector2(0.85f, 0.85f);
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;

            var checkmarkImage = checkmarkGO.AddComponent<Image>();
            checkmarkImage.color = new Color(0.3f, 0.7f, 0.3f, 1f);

            // 标签
            var labelGO = new GameObject("Label");
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.SetParent(toggleGO.transform, false);

            var labelLayout = labelGO.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;

            var labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = Label;
            labelText.fontSize = 14;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.color = Color.white;

            // Toggle 组件
            var toggle = toggleGO.AddComponent<Toggle>();
            toggle.targetGraphic = checkboxImage;
            toggle.graphic = checkmarkImage;
            toggle.isOn = DefaultValue;

            if (OnValueChanged != null)
            {
                toggle.onValueChanged.AddListener((value) => OnValueChanged(value));
            }
        }
    }

    private class DropdownDefinition : UIElementDefinition
    {
        public string Label;
        public List<string> Options;
        public int DefaultIndex;
        public Action<int> OnValueChanged;

        public override void Build(Transform parent, ModUIBuilder builder)
        {
            var containerGO = new GameObject("Dropdown_" + Label);
            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.SetParent(parent, false);

            var layoutElement = containerGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 35;

            var horizontalLayout = containerGO.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 10;
            horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
            horizontalLayout.childControlWidth = false;
            horizontalLayout.childControlHeight = true;

            // 标签
            var labelGO = new GameObject("Label");
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.SetParent(containerGO.transform, false);

            var labelLayout = labelGO.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 100;

            var labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = Label;
            labelText.fontSize = 14;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.color = Color.white;

            // 下拉框
            var dropdownGO = new GameObject("Dropdown");
            var dropdownRect = dropdownGO.AddComponent<RectTransform>();
            dropdownRect.SetParent(containerGO.transform, false);

            var dropdownLayout = dropdownGO.AddComponent<LayoutElement>();
            dropdownLayout.preferredWidth = 150;
            dropdownLayout.flexibleWidth = 1;

            var dropdownImage = dropdownGO.AddComponent<Image>();
            dropdownImage.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            // 选中文本
            var captionGO = new GameObject("Caption");
            var captionRect = captionGO.AddComponent<RectTransform>();
            captionRect.SetParent(dropdownGO.transform, false);
            captionRect.anchorMin = new Vector2(0, 0);
            captionRect.anchorMax = new Vector2(1, 1);
            captionRect.offsetMin = new Vector2(10, 0);
            captionRect.offsetMax = new Vector2(-25, 0);

            var captionText = captionGO.AddComponent<TextMeshProUGUI>();
            captionText.fontSize = 14;
            captionText.alignment = TextAlignmentOptions.Left;
            captionText.color = Color.white;

            // 箭头
            var arrowGO = new GameObject("Arrow");
            var arrowRect = arrowGO.AddComponent<RectTransform>();
            arrowRect.SetParent(dropdownGO.transform, false);
            arrowRect.anchorMin = new Vector2(1, 0);
            arrowRect.anchorMax = new Vector2(1, 1);
            arrowRect.pivot = new Vector2(1, 0.5f);
            arrowRect.sizeDelta = new Vector2(20, 0);
            arrowRect.anchoredPosition = new Vector2(-5, 0);

            var arrowText = arrowGO.AddComponent<TextMeshProUGUI>();
            arrowText.text = "▼";
            arrowText.fontSize = 12;
            arrowText.alignment = TextAlignmentOptions.Center;
            arrowText.color = Color.white;

            // 下拉列表模板（TMP_Dropdown 需要）
            var templateGO = CreateDropdownTemplate(dropdownGO.transform);

            // TMP_Dropdown 组件
            var dropdown = dropdownGO.AddComponent<TMP_Dropdown>();
            dropdown.targetGraphic = dropdownImage;
            dropdown.captionText = captionText;
            dropdown.template = templateGO.GetComponent<RectTransform>();
            dropdown.itemText = templateGO.GetComponentInChildren<TextMeshProUGUI>();

            dropdown.ClearOptions();
            dropdown.AddOptions(Options);
            dropdown.value = DefaultIndex;

            if (OnValueChanged != null)
            {
                dropdown.onValueChanged.AddListener((index) => OnValueChanged(index));
            }
        }

        private GameObject CreateDropdownTemplate(Transform parent)
        {
            var templateGO = new GameObject("Template");
            var templateRect = templateGO.AddComponent<RectTransform>();
            templateRect.SetParent(parent, false);
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1);
            templateRect.sizeDelta = new Vector2(0, 150);

            var templateImage = templateGO.AddComponent<Image>();
            templateImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            var scrollRect = templateGO.AddComponent<ScrollRect>();

            // Viewport
            var viewportGO = new GameObject("Viewport");
            var viewportRect = viewportGO.AddComponent<RectTransform>();
            viewportRect.SetParent(templateGO.transform, false);
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            viewportGO.AddComponent<Image>().color = Color.clear;
            viewportGO.AddComponent<Mask>().showMaskGraphic = false;

            // Content
            var contentGO = new GameObject("Content");
            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.SetParent(viewportGO.transform, false);
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            // Item
            var itemGO = new GameObject("Item");
            var itemRect = itemGO.AddComponent<RectTransform>();
            itemRect.SetParent(contentGO.transform, false);
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.sizeDelta = new Vector2(0, 30);

            var itemToggle = itemGO.AddComponent<Toggle>();

            var itemBg = new GameObject("Item Background");
            var itemBgRect = itemBg.AddComponent<RectTransform>();
            itemBgRect.SetParent(itemGO.transform, false);
            itemBgRect.anchorMin = Vector2.zero;
            itemBgRect.anchorMax = Vector2.one;
            itemBgRect.offsetMin = Vector2.zero;
            itemBgRect.offsetMax = Vector2.zero;

            var itemBgImage = itemBg.AddComponent<Image>();
            itemBgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            itemToggle.targetGraphic = itemBgImage;

            var itemLabelGO = new GameObject("Item Label");
            var itemLabelRect = itemLabelGO.AddComponent<RectTransform>();
            itemLabelRect.SetParent(itemGO.transform, false);
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(10, 0);
            itemLabelRect.offsetMax = new Vector2(-10, 0);

            var itemLabel = itemLabelGO.AddComponent<TextMeshProUGUI>();
            itemLabel.fontSize = 14;
            itemLabel.alignment = TextAlignmentOptions.Left;
            itemLabel.color = Color.white;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;

            templateGO.SetActive(false);

            return templateGO;
        }
    }

    private class SeparatorDefinition : UIElementDefinition
    {
        public float Height;

        public override void Build(Transform parent, ModUIBuilder builder)
        {
            var separatorGO = new GameObject("Separator");
            var rectTransform = separatorGO.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            var layoutElement = separatorGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = Height;

            var image = separatorGO.AddComponent<Image>();
            image.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        }
    }

    private class SpaceDefinition : UIElementDefinition
    {
        public float Height;

        public override void Build(Transform parent, ModUIBuilder builder)
        {
            var spaceGO = new GameObject("Space");
            var rectTransform = spaceGO.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            var layoutElement = spaceGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = Height;
        }
    }

    private class ImageDefinition : UIElementDefinition
    {
        public Sprite Sprite;
        public float Width;
        public float Height;

        public override void Build(Transform parent, ModUIBuilder builder)
        {
            var imageGO = new GameObject("Image");
            var rectTransform = imageGO.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            var layoutElement = imageGO.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = Width;
            layoutElement.preferredHeight = Height;

            var image = imageGO.AddComponent<Image>();
            image.sprite = Sprite;
            image.preserveAspect = true;
        }
    }

    private class HorizontalGroupDefinition : UIElementDefinition
    {
        public float Spacing;
        public bool IsStart;

        public override void Build(Transform parent, ModUIBuilder builder) { }
    }

    private class AnchorDefinition : UIElementDefinition
    {
        public UIAnchor Anchor;
        public float OffsetX;
        public float OffsetY;

        public override void Build(Transform parent, ModUIBuilder builder) { }
    }

    private class CustomDefinition : UIElementDefinition
    {
        public Action<Transform> Builder;

        public override void Build(Transform parent, ModUIBuilder builder)
        {
            Builder?.Invoke(parent);
        }
    }

    #endregion
}

#region 辅助类

/// <summary>
/// UI 锚点位置
/// </summary>
public enum UIAnchor
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    Center,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

/// <summary>
/// 按钮样式
/// </summary>
public class ButtonStyle
{
    public float Width = 150;
    public float Height = 35;
    public float FontSize = 14;
    public Color BackgroundColor = new Color(0.3f, 0.5f, 0.7f, 1f);
    public Color HoverColor = new Color(0.4f, 0.6f, 0.8f, 1f);
    public Color PressedColor = new Color(0.2f, 0.4f, 0.6f, 1f);
    public Color TextColor = Color.white;

    public static ButtonStyle Default => new ButtonStyle();

    public static ButtonStyle Primary => new ButtonStyle
    {
        BackgroundColor = new Color(0.2f, 0.5f, 0.8f, 1f),
        HoverColor = new Color(0.3f, 0.6f, 0.9f, 1f),
        PressedColor = new Color(0.15f, 0.4f, 0.7f, 1f)
    };

    public static ButtonStyle Success => new ButtonStyle
    {
        BackgroundColor = new Color(0.2f, 0.6f, 0.3f, 1f),
        HoverColor = new Color(0.3f, 0.7f, 0.4f, 1f),
        PressedColor = new Color(0.15f, 0.5f, 0.25f, 1f)
    };

    public static ButtonStyle Danger => new ButtonStyle
    {
        BackgroundColor = new Color(0.7f, 0.2f, 0.2f, 1f),
        HoverColor = new Color(0.8f, 0.3f, 0.3f, 1f),
        PressedColor = new Color(0.6f, 0.15f, 0.15f, 1f)
    };

    public static ButtonStyle Small => new ButtonStyle
    {
        Width = 80,
        Height = 28,
        FontSize = 12
    };
}

/// <summary>
/// 使用 ModUIBuilder 构建的 UI 窗体基类
/// </summary>
public class ModBuiltUIForm : BaseUIForms
{
    public override bool Init()
    {
        CurrentUIType = new UIType
        {
            UIForms_Type = UIFormsType.Normal,
            UIForms_ShowMode = UIFormsShowMode.Normal,
            UIForms_LucencyType = UIFormsLucencyType.Lucency
        };
        return base.Init();
    }
}

/// <summary>
/// UI 拖拽处理器
/// </summary>
public class ModUIDragHandler : MonoBehaviour, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IBeginDragHandler
{
    private RectTransform _rectTransform;
    private Canvas _canvas;
    private Vector2 _dragOffset;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        _dragOffset = _rectTransform.anchoredPosition - localPoint;
    }

    public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        _rectTransform.anchoredPosition = localPoint + _dragOffset;
    }
}

#endregion
