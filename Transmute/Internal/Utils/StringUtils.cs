namespace Transmute.Internal.Utils
{
    internal static class StringUtils
    {
        public static string With(this string format, params object[] objects)
        {
            return string.Format(format, objects);
        }
    }
}