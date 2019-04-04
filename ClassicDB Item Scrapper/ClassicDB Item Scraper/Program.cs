using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace ClassicDB_Item_Scraper
{
    class Program
    {
        static void Main(string[] args)
        {
            WebClient wc = new WebClient();

            string raw = wc.DownloadString("https://classicdb.ch/?item=16802");

            //System.Web.e

            Regex iLvl = new Regex("(?=<div>Level: )([^\n\r]*)");
            Match iLvlR = iLvl.Match(raw);

            string iLvlResult = iLvlR.ToString().Replace(@"<div>Level: ", "").Replace(@"</div>","");
            //string iLvlReplaced = iLvlR[0].replace(@"<div>Level: ", "");
            Console.WriteLine(iLvlResult);
            //string webData = Encoding.UTF8.GetString(raw);

            string reader = File.ReadAllText(raw, Encoding.UTF8);
            //StreamWriter writer = File.WriteAllText(@"C:\GitHub\natearms\WoW\ClassicDB Item Scrapper\ClassicDB Item Scraper\Files\ClassicDb-Parse.txt", reader);
        }
    }
}
