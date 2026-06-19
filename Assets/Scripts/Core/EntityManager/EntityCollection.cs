using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 通用实体集合管理器 - 替代双重列表设计
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public class EntityCollection<T> where T : class
{
        private readonly Dictionary<int, T> _entityDictionary = new Dictionary<int, T>();
        private readonly List<T> _entityList = new List<T>();
        private readonly Func<T, int> _getIdFunc;
        private IComparer<T> _currentSorter;

        /// <summary>
        /// 实体数量
        /// </summary>
        public int Count => _entityList.Count;

        /// <summary>
        /// 获取所有实体（只读列表）
        /// </summary>
        public IReadOnlyList<T> Entities => _entityList.AsReadOnly();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="getIdFunc">获取实体ID的函数</param>
        public EntityCollection(Func<T, int> getIdFunc)
        {
            _getIdFunc = getIdFunc ?? throw new ArgumentNullException(nameof(getIdFunc));
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        public bool Add(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            int id = _getIdFunc(entity);

            if (_entityDictionary.ContainsKey(id))
            {
                return false; // 实体已存在
            }

            _entityDictionary[id] = entity;
            _entityList.Add(entity);
            return true;
        }

        /// <summary>
        /// 移除实体
        /// </summary>
        public bool Remove(T entity)
        {
            if (entity == null)
            {
                return false;
            }

            int id = _getIdFunc(entity);

            if (!_entityDictionary.Remove(id))
            {
                return false;
            }

            _entityList.Remove(entity);
            return true;
        }

        /// <summary>
        /// 根据ID移除实体
        /// </summary>
        public bool RemoveById(int id)
        {
            if (_entityDictionary.TryGetValue(id, out T entity))
            {
                _entityDictionary.Remove(id);
                _entityList.Remove(entity);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        public T GetById(int id)
        {
            _entityDictionary.TryGetValue(id, out T entity);
            return entity;
        }

        /// <summary>
        /// 根据ID查找实体
        /// </summary>
        public bool TryGetById(int id, out T entity)
        {
            return _entityDictionary.TryGetValue(id, out entity);
        }

        /// <summary>
        /// 检查是否包含指定实体
        /// </summary>
        public bool Contains(T entity)
        {
            if (entity == null)
            {
                return false;
            }

            int id = _getIdFunc(entity);
            return _entityDictionary.ContainsKey(id);
        }

        /// <summary>
        /// 检查是否包含指定ID的实体
        /// </summary>
        public bool ContainsId(int id)
        {
            return _entityDictionary.ContainsKey(id);
        }

        /// <summary>
        /// 根据索引获取实体
        /// </summary>
        public T GetByIndex(int index)
        {
            if (index < 0 || index >= _entityList.Count)
            {
                return null;
            }
            return _entityList[index];
        }

        /// <summary>
        /// 清空所有实体
        /// </summary>
        public void Clear()
        {
            _entityDictionary.Clear();
            _entityList.Clear();
        }

        /// <summary>
        /// 排序实体列表
        /// </summary>
        public void Sort(IComparer<T> comparer)
        {
            _currentSorter = comparer;
            _entityList.Sort(comparer);
        }

        /// <summary>
        /// 使用比较函数排序
        /// </summary>
        public void Sort(Comparison<T> comparison)
        {
            _entityList.Sort(comparison);
        }

        /// <summary>
        /// 使用当前排序器重新排序
        /// </summary>
        public void Resort()
        {
            if (_currentSorter != null)
            {
                _entityList.Sort(_currentSorter);
            }
        }

        /// <summary>
        /// 遍历所有实体（使用快照避免在遍历期间修改集合）
        /// </summary>
        public void ForEach(Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // 创建快照，避免在action执行期间修改集合导致异常
            var snapshot = _entityList.ToArray();
            foreach (var entity in snapshot)
            {
                action(entity);
            }
        }

        /// <summary>
        /// 查找满足条件的第一个实体
        /// </summary>
        public T Find(Predicate<T> predicate)
        {
            return _entityList.Find(predicate);
        }

        /// <summary>
        /// 查找所有满足条件的实体
        /// </summary>
        public List<T> FindAll(Predicate<T> predicate)
        {
            return _entityList.FindAll(predicate);
        }

        /// <summary>
        /// 获取所有实体的值集合（字典）
        /// </summary>
        public Dictionary<int, T>.ValueCollection GetDictionaryValues()
        {
            return _entityDictionary.Values;
        }
}
