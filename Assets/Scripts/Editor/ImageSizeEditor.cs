#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;

public class ImageSizeEditor : EditorWindow
{
    private float scaleFactor = 1.0f;
    private bool preserveAspect = true;

    [MenuItem("Tools/Image/Image Size Settings")]
    public static void ShowWindow()
    {
        GetWindow<ImageSizeEditor>("Image Size Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("批量设置图片尺寸", EditorStyles.boldLabel);

        scaleFactor = EditorGUILayout.Slider("缩放比例", scaleFactor, 0.1f, 3f);
        preserveAspect = EditorGUILayout.Toggle("保持宽高比", preserveAspect);

        GUILayout.Space(20);

        if (GUILayout.Button("设置选中图片为原始尺寸"))
        {
            SetImagesToNativeSize(scaleFactor, preserveAspect);
        }
    }

    private void SetImagesToNativeSize(float scale, bool keepAspect)
    {
        List<Image> imagesToProcess = new List<Image>();
        List<Object> undoObjects = new List<Object>();

        // 收集所有选中的Image组件
        foreach (GameObject obj in Selection.gameObjects)
        {
            Image image = obj.GetComponent<Image>();
            if (image != null && image.sprite != null)
            {
                imagesToProcess.Add(image);
                undoObjects.Add(image.GetComponent<RectTransform>());
            }
        }

        if (imagesToProcess.Count == 0)
        {
            Debug.LogWarning("未找到可处理的Image组件");
            return;
        }

        Undo.RecordObjects(undoObjects.ToArray(), "Set Images to Native Size");

        foreach (Image image in imagesToProcess)
        {
            RectTransform rectTransform = image.GetComponent<RectTransform>();

            // 获取原始尺寸
            float width = image.sprite.texture.width * scale;
            float height = image.sprite.texture.height * scale;

            // 保持宽高比
            if (keepAspect && image.preserveAspect)
            {
                float aspectRatio = (float)image.sprite.texture.width / image.sprite.texture.height;
                if (width > height * aspectRatio)
                {
                    width = height * aspectRatio;
                }
                else
                {
                    height = width / aspectRatio;
                }
            }

            rectTransform.sizeDelta = new Vector2(width, height);
            EditorUtility.SetDirty(rectTransform);
        }

    }
}
#endif
