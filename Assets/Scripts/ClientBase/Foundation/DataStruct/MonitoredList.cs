using System.Collections;
using System.Collections.Generic;
using System;

namespace ClientBase
{
    /// <summary>
    /// Monintored list.
    /// </summary>
    public class MonitoredList<T> : IEnumerable<T>
    {

        private readonly List<T> list = new List<T>();
        public System.Func<T, bool> Verifier;
        public event System.Action<int, T> OnAdd, OnRemove;
        public event System.Action<int, T, T> OnSet;
        public event System.Action OnClear;

        public MonitoredList(System.Func<T, bool> verifier = null)
        {
            this.Verifier = verifier;
        }

        public bool Add(int index, T t)
        {
            if (Verifier != null && !Verifier(t))
            {
                Assert.Fail("Verification failed: " + t);
                return false;
            }

            list.Insert(index, t);
            //OnAdd.InvokeSafely(index, t);
            if (OnAdd != null)
                OnAdd(index, t);
            return true;
        }

        public void Add(T t)
        {
            Add(list.Count, t);
        }

        public void Remove(int index)
        {
            T t = list[index];
            list.RemoveAt(index);
            //OnRemove.InvokeSafely(index, t);
            if (OnRemove != null)
                OnRemove(index, t);
        }

        public bool Remove(T t)
        {
            int index = list.IndexOf(t);
            if (index < 0)
                return false;
            Remove(index);
            return true;
        }

        public void Clear()
        {
            list.Clear();
            if (null != OnClear)
                OnClear();
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                if (Verifier != null && !Verifier(value))
                {
                    Assert.Fail("Verification failed: " + value);
                    return;
                }

                T old = list[index];
                list[index] = value;
                //OnSet.InvokeSafely(index, old, value);
                if (OnSet != null)
                    OnSet(index, old, value);
            }
        }

        public int IndexOf(T t)
        {
            return list.IndexOf(t);
        }

        public int Count { get { return list.Count; } }

        public IEnumerable<T> GetEnumerable()
        {
            return list;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void DefaultSort(Comparison<T> comparison)
        {
            list.Sort(comparison);
        }
    }
}