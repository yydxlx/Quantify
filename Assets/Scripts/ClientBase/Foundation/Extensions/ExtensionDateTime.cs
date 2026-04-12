using System;

namespace ClientBase
{
    public static class ExtensionDateTime
    {
        public static string ToStringFileFormat(this DateTime dateTime)
        {
            return dateTime.ToString("MM/dd/yyyy") + "_" + dateTime.ToString("HH:mm");
        }
    }
}