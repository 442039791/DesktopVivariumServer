using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;
using System.Collections.Concurrent;

public class CoroutinePool : MonoSingleton<CoroutinePool>
{
    private ConcurrentQueue<Func<IEnumerator>> taskQueue = new ConcurrentQueue<Func<IEnumerator>>();
    private LinkedList<KeyValuePair<Coroutine, Func<IEnumerator>>> runningCoroutines = new LinkedList<KeyValuePair<Coroutine, Func<IEnumerator>>>();



    private int minCoroutineCount = 5;
    private int maxCoroutineCount = 20;
    private float checkInterval = 1f;
    private float lastCheckTime = 0f;
    private bool needsToCheckQueue = false;
    void Update()
    {
        // 动态调整检查间隔
        float dynamicCheckInterval = CalculateDynamicCheckInterval();

        if (needsToCheckQueue || Time.time - lastCheckTime > dynamicCheckInterval)
        {
            if (needsToCheckQueue)
            {
                StartCoroutinesIfNeeded();
                needsToCheckQueue = false;
            }

            AdjustCoroutineCount();
            lastCheckTime = Time.time;
        }
    }


    // 根据任务数量计算动态检查间隔
    private float CalculateDynamicCheckInterval()
    {
        // 基于任务数量调整检查频率
        int taskCount = taskQueue.Count;

        if (taskCount > 50) return checkInterval * 0.5f; // 任务较多时更频繁检查
        if (taskCount > 20) return checkInterval * 0.75f; // 适中的任务量
        return checkInterval; // 默认检查频率
    }

    public void EnqueueTask(Action action)
    {
        Func<IEnumerator> taskWrapper = () => TaskWrapper(action);
        taskQueue.Enqueue(taskWrapper);

        // 通知主线程检查队列并启动协程
        needsToCheckQueue = true;
    }
    private IEnumerator TaskWrapper(Action action)
    {
        action();
        yield return null;
    }

    private void StartCoroutinesIfNeeded()
    {
        while (runningCoroutines.Count < maxCoroutineCount && taskQueue.Count > 0)
        {
            if (taskQueue.TryDequeue(out var taskWrapper))
            {
                var coroutine = StartCoroutine(RunTask(taskWrapper));
                var coroutineEntry = new KeyValuePair<Coroutine, Func<IEnumerator>>(coroutine, taskWrapper);
                runningCoroutines.AddLast(coroutineEntry);
            }
        }
    }


    private IEnumerator RunTask(Func<IEnumerator> task)
    {
        var coroutineEnumerator = task();
        while (true)
        {
            object current;
            try
            {
                if (coroutineEnumerator.MoveNext() == false)
                    break;
                current = coroutineEnumerator.Current;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Coroutine encountered an error: {ex.Message}");
                break; // 停止协程
            }
            yield return current;
        }

        // 移除已完成或出错的协程
        RemoveCompletedCoroutine(task);
    }



    private void AdjustCoroutineCount()
    {
        int optimalCoroutineCount = CalculateOptimalCoroutineCount();

        if (runningCoroutines.Count < optimalCoroutineCount)
        {
            StartCoroutinesIfNeeded();
        }
        else if (runningCoroutines.Count > optimalCoroutineCount)
        {
            ReduceCoroutineCount(optimalCoroutineCount);
        }
    }
    private int CalculateOptimalCoroutineCount()
    {
        // 你可以根据需要调整这里的算法
        int desiredCount = Mathf.Clamp(taskQueue.Count, minCoroutineCount, maxCoroutineCount);
        return desiredCount;
    }
    private void ReduceCoroutineCount(int targetCount)
    {
        while (runningCoroutines.Count > targetCount)
        {
            var lastNode = runningCoroutines.Last;
            if (lastNode != null)
            {
                StopCoroutine(lastNode.Value.Key);
                runningCoroutines.RemoveLast();
            }
        }
    }
    private void RemoveCompletedCoroutine(Func<IEnumerator> task)
    {
        var node = runningCoroutines.First;
        while (node != null)
        {
            var nextNode = node.Next;
            if (node.Value.Value == task)
            {
                runningCoroutines.Remove(node);
                break;
            }
            node = nextNode;
        }
    }

    void OnDestroy()
    {
        foreach (var coroutine in runningCoroutines)
        {
            StopCoroutine(coroutine.Key);
        }
        runningCoroutines.Clear();
    }

}

