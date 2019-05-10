using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Xml;
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
            /*
            CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString);
            IOrganizationService crmService = crmConn.OrganizationServiceProxy;
            
            Console.WriteLine(crmConn.IsReady);
            */
            //List<string> itemStatistics = ParseClassicDB(16802);

            int startingNumber = 16000;
            int endingNumber = 22000;

            BuildCsvFile(startingNumber, endingNumber);

            //List<string> itemStatistics = ParseClassicWowHead(1);
            /*
            Console.WriteLine("Item Name: " + itemStatistics[0]);
            Console.WriteLine("iLvl: " + itemStatistics[1]);
            Console.WriteLine("Rarity: " + itemStatistics[2]);
            Console.WriteLine("Rarity Name: " + itemStatistics[3]);
            Console.WriteLine("Slot: " + itemStatistics[4]);
            Console.WriteLine("Slot Name: " + itemStatistics[5]);
            */

            

            //Console.ReadLine();
            /*
            
            */
           

        }

        static List<string> ParseClassicWowHead(int providedNumber)
        {
            int initializedNumber = providedNumber;
            List<string> itemStats = new List<String>();

            String URLString = @"https://classic.wowhead.com/item=" + initializedNumber + "&xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(URLString);

            XmlElement root = doc.DocumentElement;
            //XmlNode nodeInfo = root.SelectSingleNode("descendant::item");
            if (root.InnerText == "Item not found!" || 
                Int32.Parse(doc.GetElementsByTagName("level")[0].InnerText) < 60 || 
                Int32.Parse(doc.GetElementsByTagName("quality")[0].Attributes[0].InnerText) < 3)
            {
                itemStats.Add("Item not found or skipped!");
            }
            else
            {
                var itemId = doc.GetElementsByTagName("item")[0].Attributes[0].InnerText;
                var itemName = doc.GetElementsByTagName("name")[0].InnerText;
                var itemLvl = doc.GetElementsByTagName("level")[0].InnerText;
                var quality = doc.GetElementsByTagName("quality")[0].Attributes[0].InnerText;
                var qualityName = doc.GetElementsByTagName("quality")[0].InnerText;
                var classId = doc.GetElementsByTagName("class")[0].Attributes[0].InnerText;
                var classIdName = doc.GetElementsByTagName("class")[0].InnerText;
                var subClassId = doc.GetElementsByTagName("subclass")[0].Attributes[0].InnerText;
                var inventorySlot = doc.GetElementsByTagName("inventorySlot")[0].Attributes[0].InnerText;
                var inventorySlotName = doc.GetElementsByTagName("inventorySlot")[0].InnerText;
                var crmRarity = "";
                var crmSlot = "";

                switch (quality)
                {
                    case "2":
                        crmRarity = "257260000";
                        break;
                    case "3":
                        crmRarity = "257260001";
                        break;
                    case "4":
                        crmRarity = "257260002";
                        break;
                    case "5":
                        crmRarity = "257260003";
                        break;
                    default:
                        
                        break;
                }
                /*
                switch (slot)
                {
                    case "2":
                        crmRarity = "257260000";
                        break;
                    case "3":
                        crmRarity = "257260001";
                        break;
                    case "4":
                        crmRarity = "257260002";
                        break;
                    case "5":
                        crmRarity = "257260003";
                        break;
                    default:
                        Console.WriteLine("Rarity Value Not Set");
                        break;
                }
                */


                //XmlNodeList xnList = doc.ChildNodes("/wowhead/item");

                //string iLvl = XmlNode.xnList["level"].InnerText;

                //itemStats.Add(node.InnerText);

                itemStats.Add(itemId);
                itemStats.Add(itemName);
                itemStats.Add(itemLvl);
                itemStats.Add(quality);
                itemStats.Add(qualityName);
                itemStats.Add(classId);
                itemStats.Add(classIdName);
                itemStats.Add(subClassId);
                itemStats.Add(inventorySlot);
                itemStats.Add(inventorySlotName);
                itemStats.Add(crmRarity);
                itemStats.Add(crmSlot);
            }
            

            return itemStats;
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

        private static void InsertIntoCRM()
        {
            /*
            if (crmConn.IsReady)
            {
                Entity loot = new Entity("equipment");
                loot["name"] = itemStatistics[0];
                loot["wowc_ilvl"] = itemStatistics[1];
                loot["wowc_lootid"] = 16802;
                loot["wowc_rarity"] = ;
                loot["wowc_slot"] = 1;
                crmService.Create(loot);

            }
            */
        }

        private static void BuildCsvFile(int start, int end)
        {
            var csv = new StringBuilder();
            csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", 
                "itemId", "itemName", "itemLvl", "quality", "qualityName", "classId", "classIdName", 
                "subClassId", "inventorySlot", "inventorySlotName", "crmRarity", "crmSlot"));

            for (int i = start; i < end; i++)
            {
                List<string> itemStatistics = ParseClassicWowHead(i);
                Console.WriteLine("Parsing Item: " + i);
                if (itemStatistics[0]== "Item not found or skipped!")
                {
                    
                }
                else
                {
                    var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                        itemStatistics[0], itemStatistics[1], itemStatistics[2], itemStatistics[3], itemStatistics[4], itemStatistics[5], itemStatistics[6],
                        itemStatistics[7], itemStatistics[8], itemStatistics[9], itemStatistics[10], itemStatistics[11]);
                    csv.AppendLine(newLine);
                }
                
            }

            File.WriteAllText(@"c:\test.csv", csv.ToString());
        }

    }

}
