using System.Collections.Generic;

namespace ClientBase
{
    /// <summary>
    /// Dictionary Extensions
    /// </summary>
    public static class DictionaryExtensions
    {
        #region AddIfNotExists

        /// <summary>
        /// Method that adds the given key and value to the given dictionary only if the key is NOT present in the dictionary.
        /// This will be useful to avoid repetitive "if(!containskey()) then add" pattern of coding.
        /// </summary>
        /// <param name="dict">The given dictionary.</param>
        /// <param name="key">The given key.</param>
        /// <param name="value">The given value.</param>
        /// <param name="bAssert">Need assert.</param>
        /// <returns>True if added successfully, false otherwise.</returns>
        /// <typeparam name="TKey">Refers the TKey type.</typeparam>
        /// <typeparam name="TValue">Refers the TValue type.</typeparam>
        public static bool AddIfNotExists<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value, bool bAssert = true)
        {
            if (dict == null)
            {
                if (bAssert)
                    Assert.Fail("DictionaryExtensions::Dic is nil");
                return false;
            }

            if (dict.ContainsKey(key))
            {
                if (bAssert)
                    Assert.Fail("DictionaryExtensions::Dic already exist key " + key.ToString());
                return false;
            }

            dict.Add(key, value);
            return true;
        }

        /// <summary>
        /// Get value from dictionary safely
        /// </summary>
        /// <typeparam name="TKey">Refers the TKey type</typeparam>
        /// <typeparam name="TValue">Refers the TValue type</typeparam>
        /// <param name="dict">The given dictionary</param>
        /// <param name="key">The given key</param>
        /// <param name="bAssert">Need Assert or not</param>
        /// <returns></returns>
        public static TValue GetSafely<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, bool bAssert = true)
        {
            if (dict == null)
            {
                if (bAssert)
                    Assert.Fail("DictionaryExtensions::Dic is nil");
                return default(TValue);
            }

            TValue result;
            if (dict.TryGetValue(key, out result))
                return result;

            if (bAssert)
                Assert.Fail("DictionaryExtensions::Dic key {0} not exist".F(key));

            return default(TValue);
        }

        // AddIfNotExists

        #endregion

        #region AddOrReplace

        /// <summary>
        /// Method that adds the given key and value to the given dictionary if the key is NOT present in the dictionary.
        /// If present, the value will be replaced with the new value.
        /// </summary>
        /// <param name="dict">The given dictionary.</param>
        /// <param name="key">The given key.</param>
        /// <param name="value">The given value.</param>
        /// <typeparam name="TKey">Refers the Key type.</typeparam>
        /// <typeparam name="TValue">Refers the Value type.</typeparam>
        public static void AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        // AddOrReplace

        #endregion

        #region AddRange

        /// <summary>
        /// Method that adds the list of given KeyValuePair objects to the given dictionary. If a key is already present in the dictionary,
        /// then an error will be thrown.
        /// </summary>
        /// <param name="dict">The given dictionary.</param>
        /// <param name="kvpList">The list of KeyValuePair objects.</param>
        /// <typeparam name="TKey">Refers the TKey type.</typeparam>
        /// <typeparam name="TValue">Refers the TValue type.</typeparam>
        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dict,
            List<KeyValuePair<TKey, TValue>> kvpList)
        {
            foreach (var kvp in kvpList)
            {
                dict.Add(kvp.Key, kvp.Value);
            }
        }

        // AddRange

        #endregion
    }
}