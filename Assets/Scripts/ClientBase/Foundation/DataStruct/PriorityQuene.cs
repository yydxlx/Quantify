using System.Collections.Generic;

namespace ClientBase
{
    public interface IPriorityObj
    {
        int Priority { get; }
    }
    /// <summary>
    /// 优先级队列，需继承IPriorityObject接口，优先级越大越靠前
    /// 优先级相同的对象，插入时间晚的靠前
    /// </summary>
    public class PriorityQuene
    {
        private List<IPriorityObj> _List = new List<IPriorityObj>();
        private bool _dirty = false;

        /// <summary>
        /// 将对象压入队列
        /// </summary>
        /// <param name="pObj"></param>
        public void Push(IPriorityObj pObj)
        {
            Assert.AssertNotNull(pObj, "Push object to priority list must not null");
            _List.Add(pObj);
            _dirty = true;
        }
        /// <summary>
        /// 将优先级最高的对象弹出队列
        /// </summary>
        /// <param name="pObj"></param>
        /// <returns></returns>
        public bool Pop(out IPriorityObj pObj)
        {
            pObj = null;
            if (_List.Count <= 0)
                return false;

            if (_dirty) Resort();

            pObj = _List[0];
            _List.RemoveAt(0);
            return true;
        }

        public List<IPriorityObj> ToList()
        {
            List<IPriorityObj> l_list = new List<IPriorityObj>();
            for (int i = 0; i < _List.Count; i++)
            {
                l_list.Add(_List[i]);
            }
            return l_list;
        }

        public void Clear()
        {
            _List.Clear();
        }

        private void Resort()
        {
            if (_List.Count > 0 && _dirty)
            {
                _List.Sort(ComparePriority);
            }
            _dirty = false;
        }

        private static int ComparePriority(IPriorityObj pA, IPriorityObj pB)
        {
            if (pA == null) return -1;
            if (pB == null) return 1;
            return pA.Priority - pB.Priority;
        }
    }
}
