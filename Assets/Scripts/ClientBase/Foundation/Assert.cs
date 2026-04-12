#define AssertExceptionMode

using System.Diagnostics;

namespace ClientBase
{
    public static class Assert
    {
        internal static void Fail(int skipStackFrames, string message, params object[] args)
        {
            if (args.Length != 0)
                message = string.Format(message, args);
            UnityEngine.Debug.LogError(message);
            //RmException.Throw(message, null);
        }

        [Conditional("EnableAssert")]
        public static void Fail(string message, params object[] args)
        {
            Fail(1, message, args);
        }

        [Conditional("EnableAssert")]
        public static void AssertTrue(bool expression)
        {
            if (!expression)
                Fail(1, "The expression is expected to be true while get false");
        }

        [Conditional("EnableAssert")]
        public static void AssertTrue(bool expression, string message, params object[] args)
        {
            if (!expression)
                Fail(1, message, args);
        }

        [Conditional("EnableAssert")]
        public static void AssertStringNotNull(string str, string msg = null)
        {
            if (string.IsNullOrEmpty(str))
                Fail(1, msg ?? "The string is empty or null, which is unexpected");
        }

        [Conditional("EnableAssert")]
        public static void AssertInRange(int value, int min, int max)
        {
            if (value < min || value > max)
                Fail(1, "{0}@[{1}, {2}]", value, min, max);
        }

        [Conditional("EnableAssert")]
        public static void AssertInRange(float value, float min, float max)
        {
            if (value < min || value > max)
                Fail(1, "{0}@[{1}, {2}]", value, min, max);
        }

        [Conditional("EnableAssert")]
        public static void AssertAtLeast(int value, int min)
        {
            if (value < min)
                Fail(1, "Invalid value {0}. Min {1}", value, min);
        }

        [Conditional("EnableAssert")]
        public static void AssertAtMost(int value, int max)
        {
            if (value > max)
                Fail(1, "Invalid value {0}. Max {1}", value, max);
        }

        [Conditional("EnableAssert")]
        public static void AssertNotNull(object obj)
        {
            AssertNotNull(obj, null);
        }

        [Conditional("EnableAssert")]
        public static void AssertNotNull(object obj, string pWrongPut)
        {
            if (obj == null)
            {
                if (!string.IsNullOrEmpty(pWrongPut))
                    Fail(1, pWrongPut);
                else
                    Fail(1, "The object is null, which is unexpected");
            }
        }

        [Conditional("EnableAssert")]
        public static void AssertEquals(object value, object expectation)
        {
            if (!value.Equals(expectation))
                Fail(1, "Expect \"{0}\" but get \"{1}\"", expectation, value);
        }

        [Conditional("EnableAssert")]
        public static void AssertIs<T>(object obj) where T : class
        {
            T ret = obj as T;
            if (ret == null)
                Fail(1, "The object is not a " + typeof(T).Name);
        }

    }

}
