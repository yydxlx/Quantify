using System;
using System.Text;
using System.Collections.Generic;

namespace ClientBase
{
    public static class GenericListX
    {
#pragma warning disable 649
        public static bool IsCorrectIndex<T>(this IList<T> list, int index)
        {
            return index >= 0 && index < list.Count;
        }

        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        public static void RobustAdd<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }

        public static string ToRmText<T>(this IList<T> list)
        {
            if (list == null)
                return "NULL";
            else
            {
                int index = 0;
                string result = string.Empty;
                foreach (var text in list)
                {
                    var _text = (text == null) ? "Null" : text.ToString();

                    if (index == 0)
                        result += _text;
                    else
                        result += "," + _text;
                    index++;
                }
                return result;
            }
        }

        public static T GetOrDefault<T>(this IList<T> list, int index, T t = default(T))
        {
            return list.IsCorrectIndex(index) ? list[index] : t;
        }

        /// <summary>
        /// 返回第一个Item
        /// </summary>
        /// <param name="list">List.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T First<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
            else
                return list[0];
        }

        public static T Last<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
            else
                return list[list.Count - 1];
        }

        public static void AddAt<T>(this IList<T> list, int index, T v)
        {
            if (index < list.Count)
            {
                list[index] = v;
            }
            else
            {
                int count = index - list.Count;
                for (int i = 0; i < count; ++i)
                {
                    list.Add(default(T));
                }
                list.Add(v);
            }
        }

        /// <summary>
        /// 返回有序序列中第一个大于等于比较器的下标,类似std::lower_bound
        /// http://en.cppreference.com/w/cpp/algorithm/lower_bound
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static int LowerBound<T>(this IList<T> list, Func<T, int> comparer)
        {
            int first = 0;
            int count = list.Count;
            int it, step;

            while (count > 0)
            {
                it = first;
                step = count / 2;
                it += step;
                if (comparer(list[it]) < 0)
                {
                    first = ++it;
                    count -= step + 1;
                }
                else
                {
                    count = step;
                }
            }
            return first;
        }

        public static int BinarySearch<T>(this IList<T> list, Func<T, int> comparer)
        {
            int first = list.LowerBound(comparer);
            return first != list.Count && comparer(list[first]) == 0 ? first : -1;
        }

        public static T BinaryFind<T>(this IList<T> list, Func<T, int> comparer)
        {
            int index = list.BinarySearch(comparer);
            return index == -1 ? default(T) : list[index];
        }

        public static void ClearSafely<T>(this IList<T> list)
        {
            if (list != null)
                list.Clear();
        }

        public static void AddSafely<T>(this IList<T> list, T item)
        {
            if (list == null)
                list = new List<T>();
            list.Add(item);
        }
    }

    public static class GenericDictionaryX
    {

        public static T GetODefaultCharPerLinerDefault<K, T>(this IDictionary<K, T> dict, K key, T t = default(T))
        {
            T v;
            if (dict.TryGetValue(key, out v))
            {
                return v;
            }
            return t;
        }

        public static void AddItem<K, T>(this IDictionary<K, IList<T>> dict, K key, T t)
        {
            if (!dict.ContainsKey(key))
            {
                IList<T> items = new List<T>();
                items.Add(t);
                dict.Add(key, items);
            }
            else
            {
                dict[key].Add(t);
            }
        }

        public static bool PushToList(this IDictionary<string, List<string>> list, string key, string data)
        {
            if (list.ContainsKey(key))
            {
                if (list[key].Contains(data))
                    return false;
                else
                {
                    list[key].Add(data);
                    return true;
                }
            }
            else
            {
                List<string> newList = new List<string>();
                newList.Add(data);
                list.Add(key, newList);
                return true;
            }
        }

    }

    public static class GenericIEnumerableX
    {
        public static string GetPrettyString<T>(this IEnumerable<T> l)
        {
            return l.GetPrettyString(i => i.ToString());
        }

        public static string GetPrettyString<T>(this IEnumerable<T> l, Func<T, string> func)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            foreach (var i in l)
            {
                sb.Append(func(i));
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
            return sb.ToString();
        }
    }

    public static class ArrayX
    {
        public static bool Contains<T>(this T[] array, T item)
        {
            if (array == null)
                return false;

            foreach (var content in array)
            {
                if (content.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        public static T[] Append<T>(this T[] array, T[] appendArray)
        {
            T[] result = new T[array.Length + appendArray.Length];
            for (int index = 0; index < array.Length; index++)
                result[index] = array[index];
            for (int index = 0; index < appendArray.Length; index++)
                result[array.Length + index] = appendArray[index];
            return result;
        }
    }
}
