using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 模组打包器 —— 一键校验 + 封包
/// 菜单：Tools/Mod/打包模组
/// 设计文档：《模组工具链方案.md》第 1 节（阶段八 8.1）
///
/// 校验规则与运行时对齐：
/// - manifest 解析复用运行时数据模型 ModManifest（Newtonsoft 反序列化）
/// - 资源目录白名单 = ModManager.RESOURCE_DIRECTORIES + Localization/Fonts/Source
/// - DLL 路径解析与 ModScriptLoader 一致：Plugins/Server|Client 优先，Plugins 根兜底
/// </summary>
public class ModPackagerWindow : EditorWindow
{
    // 与运行时 RESOURCE_DIRECTORIES 对齐的目录白名单
    private static readonly string[] WhitelistDirs =
    {
        "Image", "Mesh", "Material", "Animation", "Sound",
        "ConfigData", "UI", "Plugins", "Localization", "Fonts", "Source"
    };

    // 允许打包的顶层文件（README 按前缀匹配，单独处理）
    private static readonly string[] WhitelistTopFiles = { "manifest.json", "preview.png" };

    // 打包时自动剔除的垃圾目录/后缀/文件
    private static readonly string[] JunkDirNames = { "obj", "bin", ".vs", ".git", "Library", "Temp" };
    private static readonly string[] JunkExtensions = { ".pdb", ".meta", ".cache", ".user", ".suo" };
    private static readonly string[] JunkFiles = { ".DS_Store", "thumbs.db", "desktop.ini" };

    private string _modRoot = "";
    private string _localLibPath = "";
    private Vector2 _scroll;
    private List<string> _report = new List<string>();
    private string _lastZipPath = "";

    [MenuItem("Tools/Mod/打包模组")]
    static void Init()
    {
        var window = GetWindow<ModPackagerWindow>("模组打包器");
        window.minSize = new Vector2(520, 420);
    }

    void OnEnable()
    {
        _modRoot = SessionState.GetString("ModPackager.Root", "");
        _localLibPath = SessionState.GetString("ModPackager.LocalLib", "");
    }

    void OnGUI()
    {
        GUILayout.Space(6);
        GUILayout.Label("-------------[模组打包器]-------------");

        // 模组根目录
        EditorGUILayout.BeginHorizontal();
        _modRoot = EditorGUILayout.TextField("模组目录", _modRoot);
        if (GUILayout.Button("选择…", GUILayout.Width(60)))
        {
            var picked = EditorUtility.OpenFolderPanel("选择模组根目录（含 manifest.json）", _modRoot, "");
            if (!string.IsNullOrEmpty(picked))
            {
                _modRoot = picked.Replace('/', '\\');
                SessionState.SetString("ModPackager.Root", _modRoot);
            }
        }
        EditorGUILayout.EndHorizontal();

        // 本地模组库（依赖检查用）
        _localLibPath = EditorGUILayout.TextField("本地模组库", _localLibPath);
        if (string.IsNullOrEmpty(_localLibPath) && !string.IsNullOrEmpty(_modRoot))
        {
            var parent = Directory.GetParent(_modRoot);
            if (parent != null)
            {
                EditorGUILayout.HelpBox("本地模组库为空时，默认使用模组目录的上一级（" + parent.FullName + "）做依赖检查。", MessageType.None);
            }
        }

        GUILayout.Space(8);

        // 操作按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("仅校验", GUILayout.Height(30)))
        {
            var report = new List<string>();
            ValidateAll(report);
            _report = report;
            _scroll = Vector2.zero;
        }
        GUI.enabled = !string.IsNullOrEmpty(_modRoot);
        if (GUILayout.Button("校验并打包", GUILayout.Height(30)))
        {
            var report = new List<string>();
            if (ValidateAll(report))
            {
                Package(report);
            }
            else
            {
                report.Add("❌ 校验存在阻断错误，未打包。请修复后重试。");
            }
            _report = report;
            _scroll = Vector2.zero;
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(_lastZipPath) && File.Exists(_lastZipPath))
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("产出: " + Path.GetFileName(_lastZipPath));
            if (GUILayout.Button("打开所在目录", GUILayout.Width(110)))
            {
                EditorUtility.RevealInFinder(_lastZipPath);
            }
            EditorGUILayout.EndHorizontal();
        }

        // 校验报告
        GUILayout.Space(8);
        GUILayout.Label("-------------[校验报告]-------------");
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        if (_report.Count == 0)
        {
            GUILayout.Label("尚无结果。选择模组目录后点击「仅校验」或「校验并打包」。", GUILayout.Height(40));
        }
        else
        {
            foreach (var line in _report)
            {
                var style = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
                style.margin.bottom = 2;
                GUILayout.Label(line, style);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    #region 校验

    /// <summary>
    /// 全量校验。返回 false 表示存在阻断错误（不可打包），true 表示可打包（可含警告）。
    /// </summary>
    private bool ValidateAll(List<string> report)
    {
        report.Add("ℹ️ 模组目录: " + (string.IsNullOrEmpty(_modRoot) ? "(未选择)" : _modRoot));

        // 1. 目录与 manifest 存在性
        if (string.IsNullOrEmpty(_modRoot) || !Directory.Exists(_modRoot))
        {
            report.Add("❌ 模组目录不存在，无法校验。");
            return false;
        }
        var manifestPath = Path.Combine(_modRoot, "manifest.json");
        if (!File.Exists(manifestPath))
        {
            report.Add("❌ 缺少 manifest.json（模组根目录必须包含）。");
            return false;
        }
        report.Add("✅ manifest.json 存在");

        // 2. manifest 解析（复用运行时数据模型，保证"打包器认可的 = 运行时认可的"）
        ModManifest manifest;
        try
        {
            var json = File.ReadAllText(manifestPath);
            manifest = JsonConvert.DeserializeObject<ModManifest>(json);
        }
        catch (System.Exception e)
        {
            report.Add("❌ manifest.json 解析失败: " + e.Message);
            return false;
        }
        if (manifest == null)
        {
            report.Add("❌ manifest.json 解析结果为空。");
            return false;
        }
        report.Add("✅ manifest.json 解析成功 (Newtonsoft → ModManifest)");

        string errMsg;
        if (!manifest.Validate(out errMsg))
        {
            report.Add("❌ 必填字段缺失: " + errMsg);
            return false;
        }
        report.Add("✅ 必填字段齐全: Id=" + manifest.Id + ", Name=" + manifest.Name + ", Version=" + manifest.Version);

        // 3. Id 格式建议（非阻断）
        if (!Regex.IsMatch(manifest.Id, "^[A-Za-z0-9_.\\-]+$") || !manifest.Id.Contains("."))
        {
            report.Add("⚠️ Id 建议使用「com.作者.名称」的反向域名格式（当前: " + manifest.Id + "），不影响打包。");
        }

        bool blocking = false;

        // 4. DLL 存在性（路径解析与 ModScriptLoader 一致：Plugins/Server|Client 优先，Plugins 根兜底）
        blocking |= ValidateDlls(manifest, "Server", report);
        blocking |= ValidateDlls(manifest, "Client", report);

        // 5. 顶层内容扫描：白名单外的目录/文件 → 警告（不打包）
        foreach (var dir in Directory.GetDirectories(_modRoot))
        {
            var name = Path.GetFileName(dir);
            if (!WhitelistDirs.Contains(name))
            {
                report.Add("⚠️ 未知目录 «" + name + "» 不在白名单内，玩家侧运行时不会读取它，打包时将被跳过。");
            }
        }
        foreach (var file in Directory.GetFiles(_modRoot))
        {
            var name = Path.GetFileName(file);
            var isReadme = name.ToLowerInvariant().StartsWith("readme");
            if (!WhitelistTopFiles.Contains(name) && !isReadme)
            {
                report.Add("⚠️ 未知文件 «" + name + "» 不会被打包（只打包 manifest.json / preview.png / README* / 白名单目录）。");
            }
        }

        // 6. 依赖检查（本地模组库，找不到只警告不阻断——工坊订阅的依赖不在本地）
        var libPath = string.IsNullOrEmpty(_localLibPath) ? GetDefaultLibPath() : _localLibPath;
        if (manifest.Dependencies != null && manifest.Dependencies.Count > 0)
        {
            var localIds = CollectLocalModIds(libPath);
            foreach (var dep in manifest.Dependencies)
            {
                if (localIds.Contains(dep))
                {
                    report.Add("✅ 依赖 " + dep + " 在本地模组库中找到");
                }
                else
                {
                    report.Add("⚠️ 依赖 " + dep + " 未在本地模组库（" + libPath + "）中找到。若它发布在创意工坊则无碍；若拼写错误，玩家将无法正常加载。");
                }
            }
        }
        else
        {
            report.Add("ℹ️ 无依赖声明");
        }

        // 7. 垃圾文件统计（自动剔除）
        var junkCount = CountJunkFiles(_modRoot);
        if (junkCount > 0)
        {
            report.Add("ℹ️ 检测到 " + junkCount + " 个编译产物/垃圾文件（obj/bin/*.pdb/*.meta 等），打包时将自动剔除。");
        }

        if (blocking)
        {
            report.Add("❌ 存在阻断错误（见上），请修复后重试。");
        }
        else
        {
            report.Add("✅ 校验完成，可以打包。");
        }
        return !blocking;
    }

    /// <summary>校验 Scripts 中声明的一侧 DLL 是否真实存在。返回 true 表示有阻断错误。</summary>
    private bool ValidateDlls(ModManifest manifest, string side, List<string> report)
    {
        bool blocking = false;
        var dlls = side == "Server" ? manifest.Scripts?.Server : manifest.Scripts?.Client;
        if (dlls == null || dlls.Count == 0)
        {
            report.Add("ℹ️ 无 " + side + " 端 DLL 声明");
            return false;
        }
        foreach (var dll in dlls)
        {
            var sidePath = Path.Combine(_modRoot, "Plugins", side, dll);
            var rootPath = Path.Combine(_modRoot, "Plugins", dll);
            if (File.Exists(sidePath))
            {
                report.Add("✅ " + side + " DLL 存在: Plugins/" + side + "/" + dll);
            }
            else if (File.Exists(rootPath))
            {
                report.Add("✅ " + side + " DLL 存在: Plugins/" + dll + "（兼容旧结构）");
            }
            else
            {
                report.Add("❌ " + side + " DLL 缺失: manifest 声明了 «" + dll + "»，但 Plugins/" + side + "/ 和 Plugins/ 下都不存在。");
                blocking = true;
            }
        }
        return blocking;
    }

    private string GetDefaultLibPath()
    {
        if (string.IsNullOrEmpty(_modRoot)) return "";
        var parent = Directory.GetParent(_modRoot);
        return parent != null ? parent.FullName : _modRoot;
    }

    /// <summary>收集本地模组库里所有 manifest 声明的 Id（一层子目录）。</summary>
    private static HashSet<string> CollectLocalModIds(string libPath)
    {
        var ids = new HashSet<string>();
        if (string.IsNullOrEmpty(libPath) || !Directory.Exists(libPath)) return ids;
        foreach (var dir in Directory.GetDirectories(libPath))
        {
            var mf = Path.Combine(dir, "manifest.json");
            if (!File.Exists(mf)) continue;
            try
            {
                var m = JsonConvert.DeserializeObject<ModManifest>(File.ReadAllText(mf));
                if (m != null && !string.IsNullOrEmpty(m.Id)) ids.Add(m.Id);
            }
            catch
            {
                // 库里其他模组的 manifest 坏了不影响本校验，跳过
            }
        }
        return ids;
    }

    private static int CountJunkFiles(string root)
    {
        int count = 0;
        foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
        {
            if (IsJunkDir(dir, root)) count++;
        }
        foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        {
            if (IsJunkFile(file)) count++;
        }
        return count;
    }

    #endregion

    #region 打包

    private void Package(List<string> report)
    {
        var manifest = JsonConvert.DeserializeObject<ModManifest>(
            File.ReadAllText(Path.Combine(_modRoot, "manifest.json")));

        var parent = Directory.GetParent(_modRoot);
        if (parent == null)
        {
            report.Add("❌ 无法确定输出目录（模组目录没有上级目录？）");
            return;
        }
        var zipName = SanitizeFileName(manifest.Name) + "_v" + SanitizeFileName(manifest.Version) + ".zip";
        var zipPath = Path.Combine(parent.FullName, zipName);
        if (File.Exists(zipPath)) File.Delete(zipPath);

        int fileCount = 0;
        using (var fs = new FileStream(zipPath, FileMode.Create))
        using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            // 顶层文件：manifest.json / preview.png / README*
            foreach (var file in Directory.GetFiles(_modRoot))
            {
                var name = Path.GetFileName(file);
                var isReadme = name.ToLowerInvariant().StartsWith("readme");
                if (WhitelistTopFiles.Contains(name) || isReadme)
                {
                    zip.CreateEntryFromFile(file, name);
                    fileCount++;
                }
            }
            // 白名单目录（剔除垃圾）
            foreach (var dirName in WhitelistDirs)
            {
                var dirPath = Path.Combine(_modRoot, dirName);
                if (!Directory.Exists(dirPath)) continue;
                fileCount += AddDirToZip(zip, dirPath, dirName, report);
            }
        }

        var sizeKb = (new FileInfo(zipPath).Length + 1023) / 1024;
        _lastZipPath = zipPath;
        SessionState.SetString("ModPackager.LastZip", zipPath);
        report.Add("✅ 打包完成: " + zipName + "（" + fileCount + " 个文件, " + sizeKb + " KB）");
        report.Add("ℹ️ 产出位置: " + zipPath);
        report.Add("ℹ️ 该 zip 可手动解压进游戏 Mods/ 目录安装；创意工坊上传时建议先解压为文件夹再传（工坊走文件夹结构）。");
    }

    /// <summary>递归把目录加入 zip，返回加入的文件数。跳过垃圾目录/垃圾文件。</summary>
    private static int AddDirToZip(ZipArchive zip, string absDir, string relDir, List<string> report)
    {
        int count = 0;
        foreach (var file in Directory.GetFiles(absDir))
        {
            if (IsJunkFile(file))
            {
                report.Add("ℹ️ 剔除垃圾文件: " + relDir + "/" + Path.GetFileName(file));
                continue;
            }
            var entryName = relDir + "/" + Path.GetFileName(file);
            zip.CreateEntryFromFile(file, entryName);
            count++;
        }
        foreach (var dir in Directory.GetDirectories(absDir))
        {
            var name = Path.GetFileName(dir);
            if (JunkDirNames.Contains(name))
            {
                report.Add("ℹ️ 剔除编译产物目录: " + relDir + "/" + name + "/");
                continue;
            }
            count += AddDirToZip(zip, dir, relDir + "/" + name, report);
        }
        return count;
    }

    private static bool IsJunkDir(string dirPath, string root)
    {
        var rel = dirPath.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var parts = rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(p => JunkDirNames.Contains(p));
    }

    private static bool IsJunkFile(string filePath)
    {
        var name = Path.GetFileName(filePath);
        if (JunkFiles.Contains(name.ToLowerInvariant())) return true;
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return JunkExtensions.Contains(ext);
    }

    private static string SanitizeFileName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "mod";
        var invalid = Path.GetInvalidFileNameChars();
        var name = invalid.Aggregate(raw, (current, c) => current.Replace(c, '_'));
        return name.Trim();
    }

    #endregion
}
