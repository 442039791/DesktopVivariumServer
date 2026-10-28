using System.Collections;
using UnityEngine;
using Common;

/// <summary>
/// 服务器窗口闸门（单一职责）
/// 只回答一个问题：服务器窗口什么时候允许显示？
///
/// 判定依据（按优先级）：
/// 1. 首个客户端就绪（复用客户端的 SyncWindowPosition 回报）→ 立即放行
/// 2. 无需等待（没有生态缸/客户端可等）→ 立即放行
/// 3. 超时兜底（客户端卡死/启动失败）→ 强制放行
///
/// 解耦约定：本类只做判定，不直接操作任何 Windows API；
/// 窗口的实际显示由 WallpaperManager.AllowWindowReveal() 完成。
/// </summary>
public class ServerWindowGate : MonoSingleton<ServerWindowGate>
{
    [Tooltip("等待客户端就绪的超时时间（秒），超时后强制显示服务器窗口")]
    [SerializeField] private float revealTimeoutSeconds = 20f;

    /// <summary>窗口是否已放行显示</summary>
    public bool IsRevealed { get; private set; }

    private bool _initialized;
    private Coroutine _timeoutRoutine;

    /// <summary>
    /// 由 GameInitializer 在拉起客户端进程前调用（仅打包版会走到这里）
    /// expectedClientCount <= 0 表示没有客户端可等，立即放行显示
    /// </summary>
    public void Initialize(int expectedClientCount)
    {
        if (_initialized) return;
        _initialized = true;

        if (expectedClientCount <= 0)
        {
            Reveal("没有需要等待的客户端");
            return;
        }

        Debug.Log($"[ServerWindowGate] 服务器窗口进入隐藏状态，等待 {expectedClientCount} 个客户端就绪（超时 {revealTimeoutSeconds}s 兜底）");
        _timeoutRoutine = StartCoroutine(RevealOnTimeout());
    }

    /// <summary>
    /// 客户端就绪通知（由 SyncWindowPositionCommand 在收到回报后调用）
    /// 策略：首个客户端就绪即放行，让服务器 UI 与生态缸窗口一起出现
    /// </summary>
    public void NotifyClientReady(int clientId)
    {
        if (IsRevealed) return;
        Reveal($"客户端 {clientId} 已就绪（首个就绪即显示策略）");
    }

    /// <summary>
    /// 超时兜底：客户端卡死或启动失败时保证服务器窗口最终能显示
    /// </summary>
    private IEnumerator RevealOnTimeout()
    {
        yield return new WaitForSeconds(revealTimeoutSeconds);
        Reveal($"等待客户端就绪超时（{revealTimeoutSeconds}s），强制显示");
    }

    private void Reveal(string reason)
    {
        if (IsRevealed) return;
        IsRevealed = true;

        if (_timeoutRoutine != null)
        {
            StopCoroutine(_timeoutRoutine);
            _timeoutRoutine = null;
        }

        Debug.Log($"[ServerWindowGate] 放行服务器窗口显示，原因: {reason}");
        if (WallpaperManager.Instance != null)
        {
            WallpaperManager.Instance.AllowWindowReveal();
        }
        else
        {
            Debug.LogError("[ServerWindowGate] WallpaperManager 不存在，无法显示服务器窗口！");
        }
    }
}
