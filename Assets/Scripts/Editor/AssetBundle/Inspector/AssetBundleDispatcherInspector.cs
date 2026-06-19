using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// added by wsh @ 2018.01.06
/// 说明：Assetbundle分发器Inspector，为其提供可视化的编辑界面
/// TODO：
/// 1、还未完成，目前只是做了基本的配置功能
/// </summary>

namespace AssetBundles
{
    [CustomEditor(typeof(DefaultAsset), true)]
    public class AssetBundleDispatcherInspector : Editor
    {
        AssetBundleDispatcherConfig dispatcherConfig = null;
        string packagePath = null;
        string targetAssetPath = null;
        string databaseAssetPath = null;

        static Dictionary<string, bool> inspectorSate = new Dictionary<string, bool>();
        AssetBundleDispatcherFilterType filterType = AssetBundleDispatcherFilterType.Root;
        bool configChanged = false;

        void OnEnable()
        {
            Initialize();
        }

        // 每次选中这个文件夹的时候，我们会调用Initialize;
        void Initialize()
        {
            configChanged = false;
            filterType = AssetBundleDispatcherFilterType.Root; // 默认的打包方式;
            targetAssetPath = AssetDatabase.GetAssetPath(target); // 获取我们选的当前的路径;
            if (!AssetBundleUtility.IsPackagePath(targetAssetPath)) // 这个路径是否在AssetsPackages路径下;
            {
                return;
            }

            // Assets/AssetsPackage/Lua  ---> pakcage path   Lua
            packagePath = AssetBundleUtility.AssetsPathToPackagePath(targetAssetPath); // packagePath

            // 文件对应的数据库目录下 xxx.asset文件;
            databaseAssetPath = AssetBundleInspectorUtils.AssetPathToDatabasePath(targetAssetPath);

            // 加载数据库文件 例如: Lua.asset
            dispatcherConfig = AssetDatabase.LoadAssetAtPath<AssetBundleDispatcherConfig>(databaseAssetPath);
            if (dispatcherConfig != null) // 如果有，就不为null, 之前已经创建，吧数据加载进来;
            {
                dispatcherConfig.Load();
                filterType = dispatcherConfig.Type;
            }
        }

        // 如果读不到数据库文件配置，那么这个时候，走这里绘制一个创建按钮;
        void DrawCreateAssetBundleDispatcher()
        {
            if (GUILayout.Button("Create AssetBundle Dispatcher"))
            {
                // 创建数据库文件路径；
                var dir = Path.GetDirectoryName(databaseAssetPath);
                GameUtility.CheckDirAndCreateWhenNeeded(dir); // 是否存在，如果不存在，就创建一个;

                // 创建一个ScriptableObject 对象;  --->构造函数;, 初始化数据;
                // 所以你创建完以后，默认的初始值, 对象里面初始值决定的;
                var instance = CreateInstance<AssetBundleDispatcherConfig>();
                AssetDatabase.CreateAsset(instance, databaseAssetPath); // 将这个对象实例--->创建到.asset文件里面；
                AssetDatabase.Refresh();

                // 重新同步一下到当前的对象里面;
                Initialize();

                // 调用Repaint时候----》 引发 OnInspectorGUI
                Repaint();
            }
        }

        void DrawFilterItem(AssetBundleCheckerFilter checkerFilter)
        {
            GUILayout.BeginVertical(); 
            var relativePath = GUILayoutUtils.DrawInputField("RelativePath:", checkerFilter.RelativePath, 300f, 80f);
            var objectFilter = GUILayoutUtils.DrawInputField("ObjectFilter:", checkerFilter.ObjectFilter, 300f, 80f);
            if (relativePath != checkerFilter.RelativePath)
            {
                configChanged = true;
                checkerFilter.RelativePath = relativePath;
            }
            if (objectFilter != checkerFilter.ObjectFilter)
            {
                configChanged = true;
                checkerFilter.ObjectFilter = objectFilter;
            }
            GUILayout.EndVertical();
        }

        void DrawFilterTypesList(List<AssetBundleCheckerFilter> checkerFilters)
        {
            GUILayout.BeginVertical(EditorStyles.textField);
            GUILayout.Space(3);

            EditorGUILayout.Separator();
            for (int i = 0; i < checkerFilters.Count; i++)
            {
                var curFilter = checkerFilters[i];
                var relativePath = string.IsNullOrEmpty(curFilter.RelativePath) ? "root" : curFilter.RelativePath;
                var objectFilter = string.IsNullOrEmpty(curFilter.ObjectFilter) ? "all" : curFilter.ObjectFilter;
                var filterType = relativePath + ": <" + objectFilter + ">";
                var stateKey = "CheckerFilters" + i.ToString();
                if (GUILayoutUtils.DrawRemovableSubHeader(1, filterType, inspectorSate, stateKey, () =>
                {
                    configChanged = true;
                    checkerFilters.RemoveAt(i);
                    i--;
                }))
                {
                    DrawFilterItem(curFilter);
                }
                EditorGUILayout.Separator();
            }
            if (GUILayout.Button("Add"))
            {
                configChanged = true;
                checkerFilters.Add(new AssetBundleCheckerFilter("", "t:prefab"));
            }
            EditorGUILayout.Separator();

            GUILayout.Space(3);
            GUILayout.EndVertical();
        }

        void DrawAssetDispatcherConfig()
        {
            GUILayoutUtils.BeginContents(false);

            GUILayoutUtils.DrawProperty("PathName:", AssetBundleUtility.AssetsPathToPackagePath(targetAssetPath), 300f, 80f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("FilterType:", GUILayout.MaxWidth(80f));

            // 打包的模式
            var selectType = (AssetBundleDispatcherFilterType)EditorGUILayout.EnumPopup(filterType);
            if (selectType != filterType)
            {
                filterType = selectType;
                configChanged = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            var filtersCount = dispatcherConfig.CheckerFilters.Count;
            if (GUILayoutUtils.DrawSubHeader(0, "CheckerFilters:", inspectorSate, "CheckerFilters", filtersCount.ToString()))
            {
                DrawFilterTypesList(dispatcherConfig.CheckerFilters);
            }

            Color color = GUI.color;
            if (configChanged)
            {
                GUI.color = color * new Color(1, 1, 0.5f);
            }
            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply")) // 同步到数据库;
            {
                Apply();
            }
            GUI.color = new Color(1, 0.5f, 0.5f);
            if (GUILayout.Button("Remove")) // 删除数据库文件;
            {
                Remove();
            }
            GUI.color = color;
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            GUILayoutUtils.EndContents(false);
        }

        void Apply()
        {
            dispatcherConfig.PackagePath = packagePath;
            dispatcherConfig.Type = filterType;
            dispatcherConfig.Apply();
            EditorUtility.SetDirty(dispatcherConfig);
            AssetDatabase.SaveAssets();

            // 刷新编辑器;
            Initialize();
            Repaint();
            configChanged = false;
        }

        void Remove()
        {
            bool checkRemove = EditorUtility.DisplayDialog("Remove Warning",
                "Sure to remove the AssetBundle dispatcher ?",
                "Confirm", "Cancel");
            if (!checkRemove)
            {
                return;
            }
            // 删除数据库文件;databaseAssetPath
            GameUtility.SafeDeleteFile(databaseAssetPath);
            AssetDatabase.Refresh();

            // 
            Initialize();  
            Repaint(); // 调用一下OnIntxxGUI() ---> create 按钮
            configChanged = false;
        }

        void DrawAssetBundleDispatcherInspector()
        {
            // 创建一个Layout面板出来;
            if (GUILayoutUtils.DrawHeader("AssetBundle Dispatcher : ", inspectorSate, "DispatcherConfig", true, false))
            {
                DrawAssetDispatcherConfig();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // 检查一下，这个路径，是否为AssetsPackage
            if (!AssetBundleInspectorUtils.CheckMaybeAssetBundleAsset(targetAssetPath)) { // 其它文件夹下路径, return;
                return;
            }
            
            GUI.enabled = true;

            if (dispatcherConfig == null) // 数据库配置为null;
            {
                DrawCreateAssetBundleDispatcher(); // 创建按钮, 视图
            }
            else // 绘制编辑按钮;
            {
                DrawAssetBundleDispatcherInspector(); // 编辑配置模式的一个视图
            }
        }
        
        void OnDisable()
        {
            if (configChanged)
            {
                bool checkApply = EditorUtility.DisplayDialog("Modify Warning",
                    "You have modified the AssetBundle dispatcher setting, Apply it ?",
                    "Confirm", "Cancel");
                if (checkApply)
                {
                    Apply();
                }
            }
            dispatcherConfig = null;
            inspectorSate.Clear();
        }
    }
}