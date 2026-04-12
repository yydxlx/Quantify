using System;
using System.Collections.Generic;

namespace ClientBase
{
    /// <summary>
    /// Event管理器
    /// </summary>
    public class EventManager : Singleton<EventManager>
    {
        private Dictionary<string, LinkedList<Action<object>>> events = new Dictionary<string, LinkedList<Action<object>>>();

        /// <summary>
        /// 注册事件及回调
        /// </summary>
        /// <param name="key">事件Key</param>
        /// <param name="onEvent">回调</param>
        public void Regist(string key, Action<object> onEvent)
        {
            if (events.ContainsKey(key))
            {
                var list = events[key];
                if (list.Contains(onEvent))
                {
                    UnityEngine.Debug.LogError("event注册失败，定义了相同的方法，event名字: " + key);
                }

                list.AddLast(onEvent);
            }
            else
            {
                var list = new LinkedList<Action<object>>();
                list.AddLast(onEvent);
                events.Add(key, list);
            }
        }

        /// <summary>
        /// 解注册事件
        /// </summary>
        /// <param name="key">事件Key</param>
        /// <param name="onEvent">回调</param>
        public void Unregist(string key, Action<object> onEvent)
        {
            if (events.ContainsKey(key))
            {
                var list = events[key];
                if (!list.Contains(onEvent))
                {
                    // 尚未想明白同一个event注册多个同地址事件是处理什么样的逻辑，所以这里直接打印error log
                    UnityEngine.Debug.LogError("解注册失败，没有定义过event: " + key);
                }

                list.Remove(onEvent);
                if (list.Count == 0)
                    events.Remove(key);
            }
            else
            {
                UnityEngine.Debug.LogError("解注册失败，没有这个key. Key: " + key);
            }
        }

        /// <summary>
        /// 发射事件
        /// </summary>
        /// <param name="key">事件Key</param>
        /// <param name="data">数据</param>
        public void Emit(string key, object data = null)
        {
           
            if (events.ContainsKey(key))
            {
                var list = events[key];

                LinkedListNode<Action<object>> ActionList = list.First;
                ActionList.Value(data);
                while (ActionList.Next != null)
                {
                    ActionList = ActionList.Next;
                    ActionList.Value(data);
                }
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            events.Clear();
        }
    }
}