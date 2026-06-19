/*****************************************************************
 * --@ FileName: UI_BioTank_EditEditor
 * --@ Description: UI_BioTank_Edit的编辑器工具，用于调试Cube显示问题
 * --@ Author: Claude Code
 * --@ Copyright: Copyright (c) 2025
 ******************************************************************/

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UI_BioTank_Edit))]
public class UI_BioTank_EditEditor : Editor
{
    private UI_BioTank_Edit _target;

    private void OnEnable()
    {
        _target = (UI_BioTank_Edit)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== Cube调试工具 ===", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");

        // 检查图鉴状态
        if (GUILayout.Button("检查图鉴Cube解锁状态"))
        {
            CheckIllustratedGuideStatus();
        }

        // 检查仓库状态
        if (GUILayout.Button("检查仓库Cube数据"))
        {
            CheckWarehouseStatus();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== 快速操作 ===", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        // 解锁所有Cube（使用UnlockableItemManager）
        if (GUILayout.Button("解锁所有Cube", GUILayout.Height(30)))
        {
            UnlockAllCubes();
        }

        // 解锁所有Decoration
        if (GUILayout.Button("解锁所有Decoration", GUILayout.Height(30)))
        {
            UnlockAllDecorations();
        }

        // 解锁所有图鉴（旧方法，保留用于兼容）
        if (GUILayout.Button("解锁所有图鉴条目（图鉴系统）", GUILayout.Height(30)))
        {
            UnlockAllEntries();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");

        // 手动同步
        // [注释] ForceSyncUnlockedItems方法不存在，暂时禁用此功能
        /*
        if (GUILayout.Button("强制同步已解锁物品到仓库", GUILayout.Height(30)))
        {
            if (_target != null && Application.isPlaying)
            {
                _target.ForceSyncUnlockedItems();
            }
            else
            {
                Debug.LogWarning("请在运行时使用此功能");
            }
        }
        */

        // 刷新UI
        if (GUILayout.Button("刷新UI", GUILayout.Height(30)))
        {
            if (_target != null && Application.isPlaying)
            {
                _target.TestRefresh();
            }
            else
            {
                Debug.LogWarning("请在运行时使用此功能");
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void CheckIllustratedGuideStatus()
    {
        if (IlluGuideMgr.Instance == null)
        {
            return;
        }
    }

    private void CheckWarehouseStatus()
    {
        var warehouseService = ServiceLocator.Get<IWarehouseService>();
        if (warehouseService == null)
        {
            return;
        }
    }

    private void UnlockAllCubes()
    {
        if (UnlockableItemManager.Instance == null)
        {
            return;
        }

        UnlockableItemManager.Instance.UnlockAllCubes();

        // 显示解锁后的状态
        CheckWarehouseStatus();
    }

    private void UnlockAllDecorations()
    {
        if (UnlockableItemManager.Instance == null)
        {
            return;
        }

        UnlockableItemManager.Instance.UnlockAllDecorations();

        // 显示解锁后的状态
        CheckWarehouseStatus();
    }

    private void UnlockAllEntries()
    {
        if (IlluGuideMgr.Instance == null)
        {
            return;
        }

        IlluGuideMgr.Instance.UnlockAllEntries();

        // 显示解锁后的状态
        CheckIllustratedGuideStatus();
    }
}
