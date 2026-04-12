namespace ClientBase
{
    public static class ExtensionString
    {

        public static string F(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        public static double ToDouble(this string s)
        {
            return double.Parse(s);
        }

        public static float ToFloat(this string s)
        {
            return float.Parse(s);
        }

        public static string AddWWWFilePrefix(this string s)
        {
            return "file://" + s;
        }
    }
}