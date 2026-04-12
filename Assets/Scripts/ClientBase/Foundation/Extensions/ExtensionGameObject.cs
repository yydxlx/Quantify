using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ClientBase
{
    public static class Vector3X
    {
        public static void Copy(ref Vector3 dest, Vector3 src, bool includeZ = true)
        {
            dest.x = src.x;
            dest.y = src.y;
            if (includeZ)
                dest.z = src.z;
        }

        public static void Set(ref Vector3 v, params float[] coords)
        {
            if (coords.Length == 0)
                return;
            else if (coords.Length > 3)
                throw new System.Exception("Vector3 has only three dimensions");

            if (coords.Length >= 1)
                v.x = coords[0];
            if (coords.Length >= 2)
                v.y = coords[1];
            if (coords.Length >= 3)
                v.z = coords[2];
        }

        public static Vector3 Lerp(Vector3 from, Vector3 to, params float[] t)
        {
            float x = Mathf.Lerp(from.x, to.x, t[0]);
            float y = Mathf.Lerp(from.y, to.y, t[1]);
            float z = Mathf.Lerp(from.z, to.z, t[2]);
            return new Vector3(x, y, z);
        }

        public static Vector3 Ratio(Vector3 from, Vector3 to, params float[] t)
        {
            float x = ratio(from.x, to.x, t[0]);
            float y = ratio(from.y, to.y, t[1]);
            float z = ratio(from.z, to.z, t[2]);
            return new Vector3(x, y, z);
        }

        private static float ratio(float from, float to, float t)
        {
            return (to - from) * t + from;
        }

        public static Vector3 MaxValue
        {
            get { return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue); }
        }

        public static Vector3 MinValue
        {
            get { return new Vector3(float.MinValue, float.MinValue, float.MinValue); }
        }

        public static Vector3 NullValue
        {
            get { return new Vector3(float.MaxValue - 1, float.MaxValue - 1, float.MaxValue - 1); }
        }
    }

    public static class Vector2X
    {
        public static Vector2 Lerp(Vector2 from, Vector2 to, params float[] t)
        {
            float x = Mathf.Lerp(from.x, to.x, t[0]);
            float y = Mathf.Lerp(from.y, to.y, t[1]);
            return new Vector2(x, y);
        }
    }

    public static class ColorX
    {
        //public static Color Lerp(Color from, Color to, params float[] t)
        //{
        //    float r = Mathf.Lerp(from.r, to.r, t[0]);
        //    float g = Mathf.Lerp(from.g, to.g, t[1]);
        //    float b = Mathf.Lerp(from.b, to.b, t[2]);
        //    float a = Mathf.Lerp(from.a, to.a, t[3]);
        //    return new Color(r, g, b, a);
        //}

        public static Color Lerp(this Color from, Color to, float t)
        {
            float r = Mathf.Lerp(from.r, to.r, t);
            float g = Mathf.Lerp(from.g, to.g, t);
            float b = Mathf.Lerp(from.b, to.b, t);
            float a = Mathf.Lerp(from.a, to.a, t);
            return new Color(r, g, b, a);
        }

        public static Vector3 ToVector3(this Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }
    }

    public static class TextureX
    {
        public static long TexRunningMemSize(this Texture pTex)
        {
            if (pTex == null)
                return 0;
            Texture2D l_tex = pTex as Texture2D;
            if (l_tex != null)
            {
                float l_pixSize = TexPixelSizeBytes(l_tex.format);
                return (long)(pTex.height * pTex.width * l_pixSize);
            }
            else
            {
                return pTex.height * pTex.width * 4;
            }
        }

        private static float TexPixelSizeBytes(TextureFormat pFormat)
        {
            switch (pFormat)
            {
                case TextureFormat.ARGB32:
                    return 4;
                case TextureFormat.ARGB4444:
                    return 2;
                case TextureFormat.RGBA32:
                    return 4;
                case TextureFormat.RGB24:
                    return 3;
                case TextureFormat.RGBA4444:
                    return 2;
                case TextureFormat.PVRTC_RGB2:
                    return 0.25f;
                case TextureFormat.PVRTC_RGBA2:
                    return 0.25f;
                case TextureFormat.PVRTC_RGB4:
                    return 0.5f;
                case TextureFormat.PVRTC_RGBA4:
                    return 0.5f;
                case TextureFormat.ETC_RGB4:
                    return 0.5f;
                case TextureFormat.ETC2_RGBA8:
                    return 1;
                case TextureFormat.ETC2_RGB:
                    return 0.5f;
                default:
                    return 4;//默认是rgba32
            }
        }
    }

    public static class Extensions
    {
        /// <summary>compares the squared magnitude of target - second to given float value</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(this Vector3 target, Vector3 second, float sqrMagnitudePrecision)
        {
            return (target - second).sqrMagnitude < sqrMagnitudePrecision;
        }

        /// <summary>compares the squared magnitude of target - second to given float value</summary>        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(this Vector2 target, Vector2 second, float sqrMagnitudePrecision)
        {
            return (target - second).sqrMagnitude < sqrMagnitudePrecision;
        }

        /// <summary>compares the angle between target and second to given float value</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(this Quaternion target, Quaternion second, float maxAngle)
        {
            return Quaternion.Angle(target, second) < maxAngle;
        }

        /// <summary>compares two floats and returns true of their difference is less than floatDiff</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(this float target, float second, float floatDiff)
        {
            return Mathf.Abs(target - second) < floatDiff;
        }
    }
}
