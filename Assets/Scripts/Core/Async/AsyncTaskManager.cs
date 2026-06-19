using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Common;

/// <summary>
/// 异步任务管理器 - 提供Unity协程和Task的桥接
/// </summary>
public class AsyncTaskManager : MonoSingleton<AsyncTaskManager>
{
    private readonly Queue<Func<IEnumerator>> _coroutineQueue = new Queue<Func<IEnumerator>>();
    private readonly List<Coroutine> _runningCoroutines = new List<Coroutine>();

    private bool _isProcessingQueue = false;

    public override void Init()
    {
        base.Init();
        Log.Info("异步任务管理器已初始化", "AsyncTaskManager");
    }

    #region Task转Coroutine

    /// <summary>
    /// 将Task转换为Coroutine并执行
    /// </summary>
    public Coroutine RunTask(Task task, Action onComplete = null, Action<Exception> onError = null)
    {
        return StartCoroutine(WaitForTask(task, onComplete, onError));
    }

    /// <summary>
    /// 等待Task完成
    /// </summary>
    private IEnumerator WaitForTask(Task task, Action onComplete, Action<Exception> onError)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted && task.Exception != null)
        {
            onError?.Invoke(task.Exception);
            ExceptionHandler.Instance.HandleException(task.Exception, "AsyncTask", "AsyncTaskManager");
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 将Task<T>转换为Coroutine并执行
    /// </summary>
    public Coroutine RunTask<T>(Task<T> task, Action<T> onComplete = null, Action<Exception> onError = null)
    {
        return StartCoroutine(WaitForTask(task, onComplete, onError));
    }

    /// <summary>
    /// 等待Task<T>完成
    /// </summary>
    private IEnumerator WaitForTask<T>(Task<T> task, Action<T> onComplete, Action<Exception> onError)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted && task.Exception != null)
        {
            onError?.Invoke(task.Exception);
            ExceptionHandler.Instance.HandleException(task.Exception, "AsyncTask<T>", "AsyncTaskManager");
        }
        else
        {
            onComplete?.Invoke(task.Result);
        }
    }

    #endregion

    #region 协程队列管理

    /// <summary>
    /// 加入协程队列（按顺序执行）
    /// </summary>
    public void EnqueueCoroutine(Func<IEnumerator> coroutineFunc)
    {
        _coroutineQueue.Enqueue(coroutineFunc);

        if (!_isProcessingQueue)
        {
            StartCoroutine(ProcessCoroutineQueue());
        }
    }

    /// <summary>
    /// 处理协程队列
    /// </summary>
    private IEnumerator ProcessCoroutineQueue()
    {
        _isProcessingQueue = true;

        while (_coroutineQueue.Count > 0)
        {
            var coroutineFunc = _coroutineQueue.Dequeue();
            yield return StartCoroutine(coroutineFunc());
        }

        _isProcessingQueue = false;
    }

    #endregion

    #region 延迟执行

    /// <summary>
    /// 延迟执行（秒）
    /// </summary>
    public Coroutine DelayedExecute(float delaySeconds, Action action)
    {
        return StartCoroutine(DelayedExecuteCoroutine(delaySeconds, action));
    }

    private IEnumerator DelayedExecuteCoroutine(float delaySeconds, Action action)
    {
        yield return new WaitForSeconds(delaySeconds);

        ExceptionHandler.Instance.TryExecute(action, "DelayedExecute", "AsyncTaskManager");
    }

    /// <summary>
    /// 延迟执行（帧数）
    /// </summary>
    public Coroutine DelayedExecuteFrames(int frames, Action action)
    {
        return StartCoroutine(DelayedExecuteFramesCoroutine(frames, action));
    }

    private IEnumerator DelayedExecuteFramesCoroutine(int frames, Action action)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return null;
        }

        ExceptionHandler.Instance.TryExecute(action, "DelayedExecuteFrames", "AsyncTaskManager");
    }

    #endregion

    #region 批量异步操作

    /// <summary>
    /// 批量执行异步操作（并行）
    /// </summary>
    public async Task RunParallel(params Task[] tasks)
    {
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            ExceptionHandler.Instance.HandleException(ex, "RunParallel", "AsyncTaskManager");
        }
    }

    /// <summary>
    /// 批量执行异步操作（顺序）
    /// </summary>
    public async Task RunSequential(params Task[] tasks)
    {
        foreach (var task in tasks)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Instance.HandleException(ex, "RunSequential", "AsyncTaskManager");
            }
        }
    }

    #endregion

    #region 超时控制

    /// <summary>
    /// 执行带超时的Task
    /// </summary>
    public async Task<bool> RunWithTimeout(Task task, float timeoutSeconds)
    {
        var delayTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));
        var completedTask = await Task.WhenAny(task, delayTask);

        if (completedTask == delayTask)
        {
            Log.Warning($"任务超时 ({timeoutSeconds}秒)", "AsyncTaskManager");
            return false;
        }

        return true;
    }

    #endregion

    #region 清理

    /// <summary>
    /// 停止所有运行的协程
    /// </summary>
    public void StopAllManagedCoroutines()
    {
        foreach (var coroutine in _runningCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        _runningCoroutines.Clear();
        _coroutineQueue.Clear();
        _isProcessingQueue = false;

        Log.Info("已停止所有托管协程", "AsyncTaskManager");
    }

    #endregion
}
