namespace ClientBase
{
    public static class ExtensionMath
    {
        public static float Lerp(this float t1, float t2, float ratio)
        {
            return t1 + (t2 - t1) * ratio;
        }

    }
}
