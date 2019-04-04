using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ClassicDB_Item_Scraper
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
            
            /*
            WebClient wc = new WebClient();

            string raw = wc.DownloadString("https://classicdb.ch/?item=16802");
                        
            Regex iLvl = new Regex("(?=<div>Level: )([^\n\r]*)");
            Match iLvlR = iLvl.Match(raw);

            string iLvlResult = iLvlR.ToString().Replace(@"<div>Level: ", "").Replace(@"</div>","");
            
            Console.WriteLine(iLvlResult);
            

            string reader = File.ReadAllText(raw, Encoding.UTF8);
            */



        }
        async static Task MainAsync(string[] args)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync("https://classicdb.ch/?item=16802");
            var pageContents = await response.Content.ReadAsStringAsync();

            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(pageContents);

            IEnumerable<string> listItemHtml = pageDocument.DocumentNode
                .SelectNodes("//div[contains(@class, 'infobox-spacer')]").Select(li => li.OuterHtml);
}
            Console.WriteLine("1");
            Console.ReadLine();
        }
    }
}
