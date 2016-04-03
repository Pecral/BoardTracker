using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardTracker.Extensions
{
    public static class EncodingExtensions
    {
        public static string DecodeUTF8String(this string utf8Str)
        {
            System.Text.Encoding iso = System.Text.Encoding.GetEncoding("ISO-8859-1");
            System.Text.Encoding utf8 = System.Text.Encoding.UTF8;

            byte[] utfBytes = utf8.GetBytes(utf8Str);

            byte[] isoBytes = System.Text.Encoding.Convert(utf8, iso, utfBytes);

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            return encoding.GetString(isoBytes);

        }
    }
}
