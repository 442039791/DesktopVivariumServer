using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 定时任务系统 - 管理所有定时执行的任务
/// </summary>
public class TimerSystem : MonoBehaviour
{
        private readonly Dictionary<int, TimerTask> _timerTasks = new Dictionary<int, TimerTask>();
        private readonly Queue<TimerTask> _tasksToAdd = new Queue<TimerTask>();
        private readonly Queue<int> _tasksToRemove = new Queue<int>();
        private readonly object _lockObject = new object();
        private int _nextTaskId = 1;
        private bool _isUpdating = false;

        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="interval">执行间隔（秒）</param>
        /// <param name="callback">回调函数</param>
        /// <param name="executeImmediately">是否立即执行一次</param>
        /// <returns>任务ID，用于取消任务</returns>
        public int AddTimer(float interval, Action callback, bool executeImmediately = false)
        {
            TimerTask task;
            int taskId;

            lock (_lockObject)
            {
                taskId = _nextTaskId++;
                task = new TimerTask
                {
                    Id = taskId,
                    Interval = interval,
                    Callback = callback,
                    LastExecuteTime = executeImmediately ? -interval : Time.time
                };

                if (_isUpdating)
                {
                    // Update执行期间，加入待添加队列
                    _tasksToAdd.Enqueue(task);
                }
                else
                {
                    // 非Update期间，直接添加
                    _timerTasks[taskId] = task;
                }
            }

            return taskId;
        }

        /// <summary>
        /// 取消定时任务
        /// </summary>
        public void RemoveTimer(int taskId)
        {
            lock (_lockObject)
            {
                if (_isUpdating)
                {
                    // Update执行期间，加入待删除队列
                    _tasksToRemove.Enqueue(taskId);
                }
                else
                {
                    // 非Update期间，直接删除
                    _timerTasks.Remove(taskId);
                }
            }
        }

        /// <summary>
        /// 清除所有定时任务
        /// </summary>
        public void ClearAllTimers()
        {
            lock (_lockObject)
            {
                _timerTasks.Clear();
                _tasksToAdd.Clear();
                _tasksToRemove.Clear();
            }
        }

        /// <summary>
        /// 暂停指定任务
        /// </summary>
        public void PauseTimer(int taskId)
        {
            lock (_lockObject)
            {
                if (_timerTasks.TryGetValue(taskId, out var task))
                {
                    task.IsPaused = true;
                }
            }
        }

        /// <summary>
        /// 恢复指定任务
        /// </summary>
        public void ResumeTimer(int taskId)
        {
            lock (_lockObject)
            {
                if (_timerTasks.TryGetValue(taskId, out var task))
                {
                    task.IsPaused = false;
                }
            }
        }

        private void Update()
        {
            float currentTime = Time.time;
            TimerTask[] taskSnapshot;

            // 使用lock保护创建快照的过程
            lock (_lockObject)
            {
                _isUpdating = true;
                taskSnapshot = _timerTasks.Values.ToArray();
            }

            try
            {
                // 执行所有任务（此时不持有锁，回调可以安全调用Timer方法）
                foreach (var task in taskSnapshot)
                {
                    if (task.IsPaused)
                    {
                        continue;
                    }

                    if (currentTime - task.LastExecuteTime >= task.Interval)
                    {
                        try
                        {
                            task.Callback?.Invoke();
                            task.LastExecuteTime = currentTime;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"定时任务执行失败 (ID: {task.Id}): {e.Message}");
                        }
                    }
                }
            }
            finally
            {
                lock (_lockObject)
                {
                    _isUpdating = false;

                    // 处理待删除的任务
                    while (_tasksToRemove.Count > 0)
                    {
                        int taskId = _tasksToRemove.Dequeue();
                        _timerTasks.Remove(taskId);
                    }

                    // 处理待添加的任务
                    while (_tasksToAdd.Count > 0)
                    {
                        var task = _tasksToAdd.Dequeue();
                        _timerTasks[task.Id] = task;
                    }
                }
            }
        }

        /// <summary>
        /// 定时任务数据结构
        /// </summary>
        private class TimerTask
        {
            public int Id { get; set; }
            public float Interval { get; set; }
            public Action Callback { get; set; }
            public float LastExecuteTime { get; set; }
            public bool IsPaused { get; set; }
        }
}
