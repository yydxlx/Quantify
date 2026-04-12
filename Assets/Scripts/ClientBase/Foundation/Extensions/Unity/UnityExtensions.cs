using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ClientBase
{
    /// <summary>
    /// Extension methods for common functions
    /// </summary>
    public static class UnityExtensions
    {
        #region ToV3String

        /// <summary>
        /// Converts a Vector3 to a string in X, Y, Z format
        /// </summary>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static string ToV3String(this Vector3 v3)
        {
            return string.Format("{0}, {1}, {2}", v3.x, v3.y, v3.z);
        }

        // ToV3String

        #endregion

        #region ToStringParentHierarchy

        /// <summary>
        /// Returns the name of the parent objects
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static string ToStringParentHierarchy(this GameObject go)
        {
            // exit if null
            if (go == null)
                return string.Empty;

            string ReturnName = string.Empty;

            // get the parent name first
            if (go.transform.parent != null)
                ReturnName = go.transform.parent.gameObject.ToStringParentHierarchy();

            // add this game oject to the return string
            return string.Format("{0}{1}",
                (!string.IsNullOrEmpty(ReturnName)) ? string.Format("{0} > ", ReturnName) : string.Empty,
                go.name);
        }

        /// <summary>
        /// To the string related path.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string ToStringRelatedPath(this GameObject obj)
        {
            List<string> pathComponents = new List<string>();
            Transform target = obj.transform;
            while (target != null)
            {
                pathComponents.Add(target.gameObject.name);
                target = target.parent;
            }

            pathComponents.Reverse();

            StringBuilder sb = new StringBuilder();
            foreach (string pathComponent in pathComponents)
                sb.Append(pathComponent).Append('/');
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        #endregion

        #region UnityStringToBytes

        /// <summary>
        /// Converts a string to bytes, in a Unity friendly way
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte[] UnityStringToBytes(this string source)
        {
            // exit if null
            if (string.IsNullOrEmpty(source))
                return null;

            // convert to bytes
            using (MemoryStream compMemStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(compMemStream, Encoding.UTF8))
                {
                    writer.Write(source);
                    writer.Close();

                    return compMemStream.ToArray();
                }
            }
        }

        // UnityStringToBytes

        #endregion

        #region UnityBytesToString

        /// <summary>
        /// Converts a byte array to a Unicode string, in a Unity friendly way
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string UnityBytesToString(this byte[] source)
        {
            // exit if null
            if (source.IsNullOrEmpty())
                return string.Empty;

            // read from bytes
            using (MemoryStream compMemStream = new MemoryStream(source))
            {
                using (StreamReader reader = new StreamReader(compMemStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        // UnityBytesToString

        #endregion
    }
}