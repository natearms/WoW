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
using System.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace ClassicDB_Item_Scraper
{
    class Program
    {
        static void Main(string[] args)
        {
            var item

            

        }

        private static void ConnectToCRM()
        {
            var crmUrl = "https://wowepgp.crm.dynamics.com";
            var userName = "nate.arms@wowepgp.onmicrosoft.com";
            var password = "A69S869x27$bAA@N";
            /*
            Console.Write("Please Enter a CRM URL: ");
            var crmUrl = Console.ReadLine();

            Console.Write("Please Enter a Username: ");
            var userName = Console.ReadLine();            

            Console.Write("Please Enter a Password: ");
            var password = Console.ReadLine();
            */
            var connectionString = "AuthType=Office365;Url="+crmUrl+"; Username="+userName+"; Password="+password+"";

            // Get the CRM connection string and connect to the CRM Organization
            CrmServiceClient crmConn = new CrmServiceClient(connectionString);
            IOrganizationService crmService = crmConn.OrganizationServiceProxy;
            
            if (crmConn.IsReady)
            {
                Entity loot = new Entity("wowc_loot");
                loot["wowc_name"] = "Joe's New Account";
                loot["wowc_ilvl"] = 66;
                loot["wowc_lootid"] = 50;
                loot["wowc_rarity"] = 1;
                loot["wowc_slot"] = 1;
                crmService.Create(loot);

            }
            
        }

        private static void ParseClassiDB()
        {

            int initializedNumber = 0;

            var html = @"https://classicdb.ch/?item=" + initializedNumber;
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(html);

            var iLvl = doc.DocumentNode.SelectSingleNode("//table[@class='infobox']//tr//td//ul//li//div").InnerHtml.Replace("Level: ", "");
            var rarity = doc.DocumentNode.SelectSingleNode("//div[@id='tooltip" + initializedNumber + "-generic']//*//b").Attributes["class"].Value;
            var itemName = doc.DocumentNode.SelectSingleNode("//div[@id='tooltip" + initializedNumber + "-generic']//*//b").InnerText;
            var slot = doc.DocumentNode.SelectSingleNode("//div[@id='tooltip" + initializedNumber + "-generic']//*//table[@width='100%']//td").InnerText;
            var rarityText = "n/a";

            if (rarity == "q2")
            {
                rarityText = "Uncommon";
            }
            else if (rarity == "q3")
            {
                rarityText = "Rare";
            }
            else if (rarity == "q4")
            {
                rarityText = "Epic";
            }
            else if (rarity == "q5")
            {
                rarityText = "Legendary";
            }
            else
            {
                Console.WriteLine("Rarity Value Not Set");
            }

            Console.WriteLine("iLvl: " + iLvl);
            Console.WriteLine("Rarity: " + rarity);

            Console.WriteLine("Item Name: " + itemName);
            Console.WriteLine("Slot: " + slot);

            Console.ReadLine();
        }

    }

}
