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
using System.ServiceModel.Description;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Tooling.Connector;
using AuthenticationType = Microsoft.Xrm.Tooling.Connector.AuthenticationType;

namespace ClassicDB_Item_Scraper
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceClient crmConn = ConnectToCRM();
            IOrganizationService crmService = crmConn.OrganizationServiceProxy;

            Console.WriteLine(crmConn.IsReady);

            List<string> itemStatistics = ParseClassicDB(16802);

            Console.WriteLine("iLvl: " + itemStatistics[0]);
            Console.WriteLine("Rarity: " + itemStatistics[1]);

            Console.WriteLine("Item Name: " + itemStatistics[2]);
            Console.WriteLine("Slot: " + itemStatistics[3]);

            Console.ReadLine();
            /*
            if (crmConn.IsReady)
            {
                Entity loot = new Entity("wowc_loot");
                loot["wowc_name"] = itemStatistics[2];
                loot["wowc_ilvl"] = itemStatistics[0];
                loot["wowc_lootid"] = 16802;
                loot["wowc_rarity"] = ;
                loot["wowc_slot"] = 1;
                crmService.Create(loot);

            }
            */
           

        }
   
        

    static CrmServiceClient ConnectToCRM()
        {
            var crmUrl = "https://discoverxvdemocrm16a.crm.powerobjects.net/NateDemo2016Test";
            var serverUrl = "https://discoverxvdemocrm16a.crm.powerobjects.net";
            var orgName = "NateDemo2016Test";
            var domain = "";
            var userName = "";
            var password = "";

            ClientCredentials credentials = new ClientCredentials();

            credentials.UserName.UserName = userName;

            credentials.UserName.Password = password;
            /*

            var crmUrl = "https://wowepgp.crm.dynamics.com";
            var userName = "nate.arms@wowepgp.onmicrosoft.com";
            var password = "A69S869x27$bAA@N";

            Console.Write("Please Enter a CRM URL: ");
            var crmUrl = Console.ReadLine();

            Console.Write("Please Enter a Username: ");
            var userName = Console.ReadLine();            

            Console.Write("Please Enter a Password: ");
            var password = Console.ReadLine();
            */
            var connectionString = "AuthType=IFD;Url=" + crmUrl + "; Username=" + userName + "; Password=" +
                                   password + "; Domain=" + domain + "; orgName="+orgName+"";

            // Get the CRM connection string and connect to the CRM Organization
            //CrmServiceClient conn = new CrmServiceClient(new NetworkCredential(userName, password, domain), AuthenticationType.IFD, "natedemo2016test.crm.powerobjects.net", "443",orgName);
            
            CrmServiceClient conn = new CrmServiceClient(connectionString);
            IOrganizationService crmService = conn(OrganizationServiceProxy);
            //IOrganizationService crmService = (IOrganizationService)conn.OrganizationWebProxyClient != null ? (IOrganizationService)conn.OrganizationWebProxyClient : (IOrganizationService)conn.OrganizationServiceProxy;

            return conn;

        }

        static List<string> ParseClassicDB(int providedNumber)
        {

            int initializedNumber = providedNumber;
            List<string> itemStats = new List<string>();

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

            itemStats.Add(iLvl);
            itemStats.Add(rarity);
            itemStats.Add(itemName);
            itemStats.Add(slot);
            itemStats.Add(rarityText);
            return itemStats;
            /*
            Console.WriteLine("iLvl: " + iLvl);
            Console.WriteLine("Rarity: " + rarity);

            Console.WriteLine("Item Name: " + itemName);
            Console.WriteLine("Slot: " + slot);

            Console.ReadLine();
            */
        }

    }

}
