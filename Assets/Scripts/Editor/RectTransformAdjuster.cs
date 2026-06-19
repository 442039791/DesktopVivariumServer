using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class RectTransformAdjuster : EditorWindow
{
    private float posX;
    private float posY;
    private float width;
    private float height;

    [MenuItem("Tools/Adjust RectTransform Position")]
    public static void ShowWindow()
    {
        GetWindow<RectTransformAdjuster>("RectTransform Adjuster");
    }

    void OnGUI()
    {
        GUILayout.Label("RectTransform Settings", EditorStyles.boldLabel);

        posX = EditorGUILayout.FloatField("X Position", posX);
        posY = EditorGUILayout.FloatField("Y Position", posY);
        width = EditorGUILayout.FloatField("Width", width);
        height = EditorGUILayout.FloatField("Height", height);

        if (GUILayout.Button("Apply to Selected"))
        {
            ApplySettings();
        }
    }

    void ApplySettings()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform == null) continue;

            Undo.RecordObject(rectTransform, "Adjust RectTransform");

            // 计算中心点偏移后的位置
            Vector3 newPosition = new Vector3(
                posX + width * 0.5f-306-33,
                -posY - height * 0.5f+347,
                rectTransform.position.z
            );

            // 设置世界坐标位置
            rectTransform.position = newPosition;

            // 设置尺寸
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
