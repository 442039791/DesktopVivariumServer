#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// Tutorial Canvas 预制体创建器
/// 用于在Unity编辑器中自动创建完整的引导UI结构
/// </summary>
public class TutorialCanvasCreator : EditorWindow
{
    [MenuItem("GameObject/Tutorial/Create Tutorial Canvas", false, 10)]
    private static void CreateTutorialCanvasInScene()
    {
        GameObject canvasGO = CreateTutorialCanvasHierarchy();

        // 选中创建的对象
        Selection.activeGameObject = canvasGO;

        Debug.Log("[Tutorial] Tutorial Canvas 已创建在场景中，请手动保存为预制体");
    }

    [MenuItem("Tools/Tutorial/Create Tutorial Canvas Prefab")]
    private static void CreateTutorialCanvasPrefab()
    {
        GameObject canvasGO = CreateTutorialCanvasHierarchy();

        // 确保预制体目录存在
        string prefabPath = "Assets/Prefabs/Tutorial";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Tutorial");
        }

        // 保存为预制体
        string fullPath = prefabPath + "/TutorialCanvas.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasGO, fullPath);

        Debug.Log($"[Tutorial] Tutorial Canvas 预制体已创建: {fullPath}");

        // 选中预制体
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
    }

    private static GameObject CreateTutorialCanvasHierarchy()
    {
        // 1. 创建Canvas根节点
        GameObject canvasGO = new GameObject("TutorialCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // 确保在最上层

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // 添加TutorialUIController组件
        TutorialUIController uiController = canvasGO.AddComponent<TutorialUIController>();

        // 2. 创建遮罩层 (MaskImage)
        GameObject maskGO = CreateImageObject("MaskImage", canvasGO.transform);
        Image maskImage = maskGO.GetComponent<Image>();
        maskImage.color = new Color(0, 0, 0, 0.8f);
        maskImage.raycastTarget = true;

        // 设置为全屏
        RectTransform maskRect = maskGO.GetComponent<RectTransform>();
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = Vector2.zero;
        maskRect.offsetMax = Vector2.zero;

        // 添加TutorialMask组件
        TutorialMask tutorialMask = maskGO.AddComponent<TutorialMask>();

        // 尝试加载材质
        Material maskMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Tutorial/TutorialMaskMaterial.mat");
        if (maskMaterial != null)
        {
            maskImage.material = maskMaterial;
        }
        else
        {
            Debug.LogWarning("[Tutorial] 未找到TutorialMaskMaterial材质，请手动关联");
        }

        // 3. 创建指针层 (PointerImage)
        GameObject pointerGO = CreateImageObject("PointerImage", canvasGO.transform);
        Image pointerImage = pointerGO.GetComponent<Image>();
        pointerImage.raycastTarget = false;
        pointerImage.color = Color.white;

        RectTransform pointerRect = pointerGO.GetComponent<RectTransform>();
        pointerRect.sizeDelta = new Vector2(100, 100);

        // 添加TutorialPointer组件
        TutorialPointer tutorialPointer = pointerGO.AddComponent<TutorialPointer>();

        // 初始隐藏
        pointerGO.SetActive(false);

        // 4. 创建对话框面板 (DialogPanel)
        GameObject dialogPanelGO = new GameObject("DialogPanel");
        dialogPanelGO.transform.SetParent(canvasGO.transform, false);

        RectTransform dialogPanelRect = dialogPanelGO.AddComponent<RectTransform>();
        dialogPanelRect.anchorMin = new Vector2(0.5f, 0.3f);
        dialogPanelRect.anchorMax = new Vector2(0.5f, 0.3f);
        dialogPanelRect.pivot = new Vector2(0.5f, 0.5f);
        dialogPanelRect.sizeDelta = new Vector2(800, 300);

        // 4.1 对话框背景
        GameObject dialogBgGO = CreateImageObject("Background", dialogPanelGO.transform);
        Image dialogBg = dialogBgGO.GetComponent<Image>();
        dialogBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        RectTransform dialogBgRect = dialogBgGO.GetComponent<RectTransform>();
        dialogBgRect.anchorMin = Vector2.zero;
        dialogBgRect.anchorMax = Vector2.one;
        dialogBgRect.offsetMin = Vector2.zero;
        dialogBgRect.offsetMax = Vector2.zero;

        // 4.2 对话文本
        GameObject dialogTextGO = new GameObject("DialogText");
        dialogTextGO.transform.SetParent(dialogPanelGO.transform, false);

        TextMeshProUGUI dialogText = dialogTextGO.AddComponent<TextMeshProUGUI>();
        dialogText.text = "欢迎来到游戏！";
        dialogText.fontSize = 32;
        dialogText.alignment = TextAlignmentOptions.Center;
        dialogText.color = Color.white;

        RectTransform dialogTextRect = dialogTextGO.GetComponent<RectTransform>();
        dialogTextRect.anchorMin = new Vector2(0.1f, 0.4f);
        dialogTextRect.anchorMax = new Vector2(0.9f, 0.9f);
        dialogTextRect.offsetMin = Vector2.zero;
        dialogTextRect.offsetMax = Vector2.zero;

        // 4.3 确认按钮
        GameObject confirmButtonGO = CreateButtonObject("ConfirmButton", dialogPanelGO.transform);
        Button confirmButton = confirmButtonGO.GetComponent<Button>();

        RectTransform confirmBtnRect = confirmButtonGO.GetComponent<RectTransform>();
        confirmBtnRect.anchorMin = new Vector2(0.5f, 0.1f);
        confirmBtnRect.anchorMax = new Vector2(0.5f, 0.1f);
        confirmBtnRect.pivot = new Vector2(0.5f, 0.5f);
        confirmBtnRect.sizeDelta = new Vector2(200, 60);

        // 按钮文本
        TextMeshProUGUI confirmBtnText = confirmButtonGO.GetComponentInChildren<TextMeshProUGUI>();
        confirmBtnText.text = "确定";
        confirmBtnText.fontSize = 28;

        // 初始隐藏对话框
        dialogPanelGO.SetActive(false);

        // 5. 创建跳过按钮
        GameObject skipButtonGO = CreateButtonObject("SkipButton", canvasGO.transform);
        Button skipButton = skipButtonGO.GetComponent<Button>();

        RectTransform skipBtnRect = skipButtonGO.GetComponent<RectTransform>();
        skipBtnRect.anchorMin = new Vector2(1f, 1f);
        skipBtnRect.anchorMax = new Vector2(1f, 1f);
        skipBtnRect.pivot = new Vector2(1f, 1f);
        skipBtnRect.anchoredPosition = new Vector2(-20, -20);
        skipBtnRect.sizeDelta = new Vector2(150, 50);

        TextMeshProUGUI skipBtnText = skipButtonGO.GetComponentInChildren<TextMeshProUGUI>();
        skipBtnText.text = "跳过";
        skipBtnText.fontSize = 24;

        // 6. 创建进度文本
        GameObject progressTextGO = new GameObject("ProgressText");
        progressTextGO.transform.SetParent(canvasGO.transform, false);

        TextMeshProUGUI progressText = progressTextGO.AddComponent<TextMeshProUGUI>();
        progressText.text = "1/5";
        progressText.fontSize = 28;
        progressText.alignment = TextAlignmentOptions.Center;
        progressText.color = Color.white;

        RectTransform progressTextRect = progressTextGO.GetComponent<RectTransform>();
        progressTextRect.anchorMin = new Vector2(0f, 1f);
        progressTextRect.anchorMax = new Vector2(0f, 1f);
        progressTextRect.pivot = new Vector2(0f, 1f);
        progressTextRect.anchoredPosition = new Vector2(20, -20);
        progressTextRect.sizeDelta = new Vector2(100, 40);

        // 7. 使用反射关联UI引用到TutorialUIController
        SerializedObject serializedController = new SerializedObject(uiController);

        serializedController.FindProperty("tutorialCanvas").objectReferenceValue = canvas;
        serializedController.FindProperty("tutorialMask").objectReferenceValue = tutorialMask;
        serializedController.FindProperty("tutorialPointer").objectReferenceValue = tutorialPointer;
        serializedController.FindProperty("dialogPanel").objectReferenceValue = dialogPanelGO;
        serializedController.FindProperty("dialogText").objectReferenceValue = dialogText;
        serializedController.FindProperty("dialogConfirmButton").objectReferenceValue = confirmButton;
        serializedController.FindProperty("skipButton").objectReferenceValue = skipButton;
        serializedController.FindProperty("progressText").objectReferenceValue = progressText;

        serializedController.ApplyModifiedProperties();

        Debug.Log("[Tutorial] Tutorial Canvas 层级结构创建完成");

        return canvasGO;
    }

    /// <summary>
    /// 创建Image对象
    /// </summary>
    private static GameObject CreateImageObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Image image = go.AddComponent<Image>();
        image.color = Color.white;

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);

        return go;
    }

    /// <summary>
    /// 创建Button对象
    /// </summary>
    private static GameObject CreateButtonObject(string name, Transform parent)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent, false);

        // 添加Image背景
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);

        // 添加Button组件
        Button button = buttonGO.AddComponent<Button>();

        // 创建文本子对象
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = "Button";
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return buttonGO;
    }
}
#endif
