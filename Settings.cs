using System;
using System.Diagnostics;
using System.Security.Policy;

namespace XRat
{
    public static class Settings
    {
        public static char prefix = '!';

        public static string token = "TOKEN";
        public static ulong guildId = 999999999999999;
        public static ulong logsChannelID = 199999999999991;
        public static string filename = "BUILDNAME";


        public static bool debug = false; // Disable this before build release (Recomended)

        public static string startLog = "@everyone | XRat By @.avirt";

        public static string browserPath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";

        public static bool enable_startup = true;

        public static string banner = "[᲼](<https://cdn.discordapp.com/attachments/1314236982632644688/1314237084524875856/standard.png?ex=67530a2a&is=6751b8aa&hm=d23856f052467aef9eaa0d6932a31f0972722bc61abdcc1db7d6c0092fcba00d&>)";

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}