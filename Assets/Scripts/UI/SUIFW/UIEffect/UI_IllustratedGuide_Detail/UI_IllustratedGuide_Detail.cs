/*****************************************************************
 * --@ FileName: UI_IllustratedGuide_Detail
 * --@ Description: 图鉴详情界面，显示鱼类详细信息
 * --@ Author: paofan25, alivecn2@gmail.com
 * --@ Copyright: Copyright (c) 2025, paofan25
 ******************************************************************/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using AssetBundles;

public class UI_IllustratedGuide_Detail : UIBase
{
    [Header("通用UI组件")]
    public TextMeshProUGUI nameText;

    [Header("鱼类专用UI组件")]
    public GameObject fishDetailPanel;
    public Button adultButton;
    public Button juvenileButton;
    public Image adultIcon;
    public Image juvenileIcon;
    public TextMeshProUGUI adultPriceText;
    public TextMeshProUGUI juvenilePriceText;
    public TextMeshProUGUI adultNameText;
    public TextMeshProUGUI juvenileNameText;

    [Header("稀有度图标")]
    public Image whiteQualityIcon;
    public Image blueQualityIcon;
    public Image purpleQualityIcon;
    public Image goldQualityIcon;

    private IlluData currentData;
    private bool isAdultSelected = true; // 默认显示成年状态
#pragma warning disable CS0414
    private bool isInitialized = false;
#pragma warning restore CS0414

    public override bool Init()
    {

        if (initcomplete)
        {
            return true;
        }

        // 初始化UI组件
        InitializeUIComponents();

        // 设置按钮事件
        SetupButtonEvents();

        // 监听详情显示事件
        ListenToDetailEvents();

        // 调用基类初始化
        if (!base.Init())
        {
            Debug.LogError("[图鉴详情] 基类初始化失败");
            return false;
        }

        isInitialized = true;
        initcomplete = true;
        return true;
    }

    /// <summary>
    /// 初始化UI组件
    /// </summary>
    private void InitializeUIComponents()
    {

        // 确保所有UI组件都已获取
        
        if (nameText == null) nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();

        // 鱼类专用组件
        if (fishDetailPanel == null) fishDetailPanel = transform.Find("FishDetailPanel")?.gameObject;
        if (adultButton == null) adultButton = transform.Find("FishDetailPanel/AdultButton")?.GetComponent<Button>();
        if (juvenileButton == null) juvenileButton = transform.Find("FishDetailPanel/JuvenileButton")?.GetComponent<Button>();
        if (adultIcon == null) adultIcon = transform.Find("FishDetailPanel/AdultIcon")?.GetComponent<Image>();
        if (juvenileIcon == null) juvenileIcon = transform.Find("FishDetailPanel/JuvenileIcon")?.GetComponent<Image>();
        if (adultPriceText == null) adultPriceText = transform.Find("FishDetailPanel/AdultPriceText")?.GetComponent<TextMeshProUGUI>();
        if (juvenilePriceText == null) juvenilePriceText = transform.Find("FishDetailPanel/JuvenilePriceText")?.GetComponent<TextMeshProUGUI>();
        if (adultNameText == null) adultNameText = transform.Find("FishDetailPanel/AdultNameText")?.GetComponent<TextMeshProUGUI>();
        if (juvenileNameText == null) juvenileNameText = transform.Find("FishDetailPanel/JuvenileNameText")?.GetComponent<TextMeshProUGUI>();

        // 稀有度图标
        if (whiteQualityIcon == null) whiteQualityIcon = transform.Find("FishDetailPanel/QualityIcons/WhiteQuality")?.GetComponent<Image>();
        if (blueQualityIcon == null) blueQualityIcon = transform.Find("FishDetailPanel/QualityIcons/BlueQuality")?.GetComponent<Image>();
        if (purpleQualityIcon == null) purpleQualityIcon = transform.Find("FishDetailPanel/QualityIcons/PurpleQuality")?.GetComponent<Image>();
        if (goldQualityIcon == null) goldQualityIcon = transform.Find("FishDetailPanel/QualityIcons/GoldQuality")?.GetComponent<Image>();

    }

    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private void SetupButtonEvents()
    {

        // if (closeButton != null)
        // {
        //     closeButton.onClick.RemoveAllListeners();
        //     closeButton.onClick.AddListener(OnCloseButtonClick);
        // }

        if (adultButton != null)
        {
            adultButton.onClick.RemoveAllListeners();
            adultButton.onClick.AddListener(OnAdultButtonClick);
        }

        if (juvenileButton != null)
        {
            juvenileButton.onClick.RemoveAllListeners();
            juvenileButton.onClick.AddListener(OnJuvenileButtonClick);
        }

    }

    /// <summary>
    /// 监听详情显示事件
    /// </summary>
    private void ListenToDetailEvents()
    {

        event_manager.instance.add_UIevent_listener("IllustratedGuide_ShowDetail", OnShowDetailEvent);

    }

    /// <summary>
    /// 显示详情事件处理
    /// </summary>
    private void OnShowDetailEvent(string name, object udata)
    {

        if (udata is IlluData data)
        {
            ShowDetail(data);
        }
        else
        {
            Debug.LogWarning("[图鉴详情] 事件数据不是IlluData类型");
        }
    }

    /// <summary>
    /// 显示详情
    /// </summary>
    public void ShowDetail(IlluData data)
    {

        if (data == null)
        {
            Debug.LogWarning("[图鉴详情] 数据为空，无法显示详情");
            return;
        }

        currentData = data;
        isAdultSelected = true; // 重置为成年状态

        // 显示鱼类详情
        ShowFishDetail(data);

        // 显示界面
        Show();
    }

    /// <summary>
    /// 显示鱼类详情
    /// </summary>
    private void ShowFishDetail(IlluData data)
    {

        // 显示鱼类面板
        if (fishDetailPanel != null) fishDetailPanel.SetActive(true);

        

        // 获取鱼类数据
        var fishData = GetFishData(data.id);
        if (fishData == null)
        {
            Debug.LogError($"[图鉴详情] 无法获取鱼类数据: {data.id}");
            return;
        }

        // 设置名称
        if (nameText != null) nameText.text = fishData.name;
        if (adultNameText != null) adultNameText.text = $"{fishData.name} (成年)";
        if (juvenileNameText != null) juvenileNameText.text = $"{fishData.name} (幼年)";

        // 设置价格
        UpdateFishPrices(fishData);

        // 加载图标
        LoadFishIcons(fishData);

        // 更新显示状态
        UpdateFishDisplayState();
    }

    /// <summary>
    /// 获取鱼类数据
    /// </summary>
    private AnimalData GetFishData(string fishId)
    {
        return GameConfigDataBase.GetConfigData<AnimalData>(int.Parse(fishId), SysDefine.AnimalConfigData);
    }

    /// <summary>
    /// 更新鱼类价格显示
    /// </summary>
    private void UpdateFishPrices(AnimalData fishData)
    {
        if (adultPriceText != null)
        {
            string adultPriceInfo = "";
            for (int i = 0; i < 4; i++)
            {
                var quality = (ObjectQuality)i;
                float price = fishData.GetPrice(quality, true);
                adultPriceInfo += $"{quality}: {price}\n";
            }
            adultPriceText.text = $"成年价格:\n{adultPriceInfo}";
        }

        if (juvenilePriceText != null)
        {
            string juvenilePriceInfo = "";
            for (int i = 0; i < 4; i++)
            {
                var quality = (ObjectQuality)i;
                float price = fishData.GetPrice(quality, false);
                juvenilePriceInfo += $"{quality}: {price}\n";
            }
            juvenilePriceText.text = $"幼年价格:\n{juvenilePriceInfo}";
        }
    }

    /// <summary>
    /// 加载鱼类图标
    /// </summary>
    private void LoadFishIcons(AnimalData fishData)
    {
        // 加载成年图标
        if (adultIcon != null)
        {
            StartCoroutine(LoadFishIconCoroutine(fishData, true, adultIcon));
        }

        // 加载幼年图标
        if (juvenileIcon != null)
        {
            StartCoroutine(LoadFishIconCoroutine(fishData, false, juvenileIcon));
        }
    }

    /// <summary>
    /// 加载鱼类图标协程
    /// </summary>
    private IEnumerator LoadFishIconCoroutine(AnimalData fishData, bool isAdult, Image targetIcon)
    {
        string iconPath = "";
        if (isAdult)
        {
            iconPath = SysDefine.ImageAssetbundle + SysDefine.AnimalSpritePath + fishData.adultIcon_white;
        }
        else
        {
            iconPath = SysDefine.ImageAssetbundle + SysDefine.AnimalSpritePath + fishData.juvenileIcon_white;
        }

        var loader = AssetBundleManager.Instance.LoadAssetAsync(iconPath, typeof(Sprite));

        while (!loader.isDone)
        {
            yield return null;
        }

        if (loader.asset is Sprite sprite)
        {
            targetIcon.sprite = sprite;
            targetIcon.color = Color.white;
        }
    }

    /// <summary>
    /// 更新鱼类显示状态
    /// </summary>
    private void UpdateFishDisplayState()
    {
        if (adultButton != null)
        {
            // 更新按钮状态
            var adultButtonColors = adultButton.colors;
            adultButtonColors.normalColor = isAdultSelected ? Color.yellow : Color.white;
            adultButton.colors = adultButtonColors;
        }

        if (juvenileButton != null)
        {
            var juvenileButtonColors = juvenileButton.colors;
            juvenileButtonColors.normalColor = !isAdultSelected ? Color.yellow : Color.white;
            juvenileButton.colors = juvenileButtonColors;
        }

        // // 更新主图标显示
        // if (mainIcon != null)
        // {
        //     if (isAdultSelected && adultIcon != null)
        //     {
        //         mainIcon.sprite = adultIcon.sprite;
        //     }
        //     else if (!isAdultSelected && juvenileIcon != null)
        //     {
        //         mainIcon.sprite = juvenileIcon.sprite;
        //     }
        // }

        // // 更新价格显示
        // if (priceText != null && currentData != null)
        // {
        //     var fishData = GetFishData(currentData.id);
        //     if (fishData != null)
        //     {
        //         string priceInfo = "";
        //         for (int i = 0; i < 4; i++)
        //         {
        //             var quality = (ObjectQuality)i;
        //             float price = fishData.GetPrice(quality, isAdultSelected);
        //             priceInfo += $"{quality}: {price}\n";
        //         }
        //         priceText.text = $"价格:\n{priceInfo}";
        //     }
        // }
    }

    /// <summary>
    /// 成年按钮点击事件
    /// </summary>
    private void OnAdultButtonClick()
    {
        isAdultSelected = true;
        UpdateFishDisplayState();
    }

    /// <summary>
    /// 幼年按钮点击事件
    /// </summary>
    private void OnJuvenileButtonClick()
    {
        isAdultSelected = false;
        UpdateFishDisplayState();
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void OnCloseButtonClick()
    {
        Hide();
    }

    public new void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
