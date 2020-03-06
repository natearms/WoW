using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace The_House_Discord_Bot.Utilities
{
    static class StringUtils
    {
        public static string ReplaceSpecialCharactersWithString(string str, string replacement)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", replacement, RegexOptions.Compiled);
        }
    }

}
