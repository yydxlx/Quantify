using System.Collections.Generic;
using UnityEngine;

namespace ClientBase
{
    public static class CollectionX
    {
        public static Value RobustGet<Key, Value>(this Dictionary<Key, Value> dic, Key key, bool assert = true)
        {
            Value v;
            if (!dic.TryGetValue(key, out v))
            {
                string msg = "No such key: " + key + ", in " + dic.GetType();

                //if (assert)
                    //Debug.LogError(msg);
            }
            return v;
        }

        public static void RobustSave<Key, Value>(this Dictionary<Key, Value> dic, Key key, Value val)
        {
            if (dic.ContainsKey(key))
                dic[key] = val;
            else
                dic.Add(key, val);
        }

        public static bool AddSafely<Key, Value>(this Dictionary<Key, List<Value>> dic, Key key, Value val)
        {
            if (!dic.ContainsKey(key))
                dic.Add(key, new List<Value>());

            var list = dic[key];
            if (list.Contains(val))
                return false;
            else {
                list.Add(val);
                return true;
            }
        }

        public static bool RemoveSafely<Key, Value>(this Dictionary<Key, List<Value>> dic, Key key, Value val)
        {
            if (!dic.ContainsKey(key))
                return false;

            var list = dic[key];
            if (list.Remove(val))
            {
                if (list.Count == 0)
                    dic.Remove(key);
                return true;
            }
            else
                return false;
        }

        public static bool RemoveOneValueSafely<Key, Value>(this Dictionary<Key, List<Value>> dic, Value val)
        {
            foreach (var pair in dic)
            {
                if (pair.Value.Remove(val))
                {
                    if (pair.Value.Count == 0)
                        dic.Remove(pair.Key);
                    return true;
                }
            }
            return false;
        }

        public static bool HasKey<Key, Value>(this Dictionary<Key, List<Value>> dic, Key key)
        {
            if (!dic.ContainsKey(key))
                return false;
            if (dic[key].Count == 0)
            {
                dic.Remove(key);
                return false;
            }
            else
                return true;
        }

        public static bool HasValue<Key, Value>(this Dictionary<Key, List<Value>> dic, Key key, Value val)
        {
            if (!dic.ContainsKey(key))
                return false;
            if (dic[key].Count == 0)
            {
                dic.Remove(key);
                return false;
            }
            else
                return dic[key].Contains(val);
        }
    }

    public static class DelegateX
    {
        public static void InvokeSafely(this System.Action action)
        {
            if (action != null)
                try
                {
                    action();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(action, e.ToString()));
                }
        }
        public static void InvokeSafely(this System.Action<object> action,object Parmas)
        {
            if (action != null)
                try
                {
                    action(Parmas);
                }
                catch (System.Exception e)
                {
                    //Debug.LogError("Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(action, e.ToString()));
                }
        }

        public static void InvokeSafely<T>(this System.Action<T> action, T t)
        {
            if (action != null)
                try
                {
                    action(t);
                }
                catch (System.Exception e)
                {
                    Debug.LogError( "Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F( action, e.ToString()));
                }
        }

        public static void InvokeSafely<T1, T2>(this System.Action<T1, T2> action, T1 t1, T2 t2)
        {
            if (action != null)
                try
                {
                    action(t1, t2);
                }
                catch (System.Exception e)
                {
                    Debug.LogError( "Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(action, e.ToString()));
                }
        }

        public static void InvokeSafely<T1, T2, T3>(this System.Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        {
            if (action != null)
                try
                {
                    action(t1, t2, t3);
                }
                catch (System.Exception e)
                {
                    Debug.LogError( "Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(action, e.ToString()));
                }
        }

        public static void InvokeSafely<T1, T2, T3, T4>(this System.Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            if (action != null)
                try
                {
                    action(t1, t2, t3, t4);
                }
                catch (System.Exception e)
                {
                    Debug.LogError( "Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(action, e.ToString()));
                }
        }

        public static TResult InvokeSafely<TResult>(this System.Func<TResult> func)
        {
            if (func != null)
                try
                {
                    return func();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(func, e.ToString()));
                }
            return default(TResult);
        }

        public static TResult InvokeSafely<T1, TResult>(this System.Func<T1, TResult> func, T1 t1)
        {
            if (func != null)
                try
                {
                    return func(t1);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(func, e.ToString()));
                }
            return default(TResult);
        }

        public static TResult InvokeSafely<T1, T2, TResult>(this System.Func<T1, T2, TResult> func, T1 t1, T2 t2)
        {
            if (func != null)
                try
                {
                    return func(t1, t2);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(func, e.ToString()));
                }
            return default(TResult);
        }

        public static TResult InvokeSafely<T1, T2, T3, TResult>(this System.Func<T1, T2, T3, TResult> func, T1 t1, T2 t2, T3 t3)
        {
            if (func != null)
                try
                {
                    return func(t1, t2, t3);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(func, e.ToString()));
                }
            return default(TResult);
        }

        public static TResult InvokeSafely<T1, T2, T3, T4, TResult>(this System.Func<T1, T2, T3, T4, TResult> func, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            if (func != null)
                try
                {
                    return func(t1, t2, t3, t4);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(func, e.ToString()));
                }
            return default(TResult);
        }

        public static void CallSafely<T>(System.Action<T> func, T param)
        {
            if (func != null)
                try
                {
                    func(param);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(func, e.ToString()));
                }
        }

        public static void CallSafely(System.Action func)
        {
            if (func != null)
                try
                {
                    func();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Exception thrown while invoking {0}. Stack trace:\n{1}\n======End stack trace=====".F(func, e.ToString()));
                }
        }
    }
}