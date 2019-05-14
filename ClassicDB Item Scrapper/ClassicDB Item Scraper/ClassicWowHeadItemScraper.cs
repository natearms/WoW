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
using Microsoft.Xrm.Sdk.Query;
using AuthenticationType = Microsoft.Xrm.Tooling.Connector.AuthenticationType;

namespace ClassicDB_Item_Scraper
{
    class ClassicWowHeadItemScraper
    {
        static void Main(string[] args)
        {
            int startingNumber = 16000;
            int endingNumber = 24080;

            //BuildCsvFile(startingNumber, endingNumber);
            InsertIntoCRM(startingNumber, endingNumber);

            //Console.ReadLine();
        }

        static List<string> ParseClassicWowHead(int providedNumber)
        {
            int initializedNumber = providedNumber;
            List<string> itemStats = new List<String>();

            String URLString = @"https://classic.wowhead.com/item=" + initializedNumber + "&xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(URLString);

            XmlElement root = doc.DocumentElement;
            XmlNode nodeInfo = root.SelectSingleNode("descendant::item");
            if (root.InnerText == "Item not found!" ||
                Int32.Parse(doc.GetElementsByTagName("level")[0].InnerText) < 60 ||
                Int32.Parse(doc.GetElementsByTagName("quality")[0].Attributes[0].InnerText) < 3 ||
                Int32.Parse(doc.GetElementsByTagName("inventorySlot")[0].Attributes[0].InnerText) == 0 ||
                Int32.Parse(doc.GetElementsByTagName("quality")[0].Attributes[0].InnerText) == 6
                )
            {
                itemStats.Add("Item not found or skipped!");
            }
            else
            {
                var xmlDoc = new HtmlDocument();
                xmlDoc.OptionEmptyCollection = true;
                xmlDoc.LoadHtml(doc.GetElementsByTagName("htmlTooltip")[0].InnerText);

                var itemId = doc.GetElementsByTagName("item")[0].Attributes[0].InnerText;
                var itemName = doc.GetElementsByTagName("name")[0].InnerText.Replace(',', ' ');
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
                var slotType = xmlDoc?.DocumentNode?.SelectSingleNode("//span[@class='q1']")?.InnerText;

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

                if (inventorySlot == "17")
                {
                    //2H weapon
                    crmSlot = "257260000";
                }
                else if (inventorySlot == "13" || inventorySlot == "21" || inventorySlot == "22")
                {
                    //1H Weapon
                    crmSlot = "257260001";
                }
                else if (inventorySlot == "1" || inventorySlot == "5" || inventorySlot == "7")
                {
                    //Head, Chest, Legs
                    crmSlot = "257260002";
                }
                else if (inventorySlot == "3" || inventorySlot == "6" || inventorySlot == "8" || inventorySlot == "10" || inventorySlot == "12")
                {
                    //Shoulder, Waist, Feet, Hands, Trinket
                    crmSlot = "257260003";
                }
                else if (inventorySlot == "2" || inventorySlot == "9" || inventorySlot == "11" || inventorySlot == "16" || inventorySlot == "23" || slotType == "Wand" || inventorySlot == "18" || inventorySlot == "28" || inventorySlot == "25")
                {
                    //Neck, Wrist, Finger, Back, Off-Hand, Wand, Relic, Bag
                    crmSlot = "257260004";
                }
                else if (inventorySlot == "14")
                {
                    //Shield
                    crmSlot = "257260005";
                }
                else if (inventorySlot == "15" && subClassId != "19")
                {
                    //Ranged Weapon (without wand)
                    crmSlot = "257260006";
                }
                else
                {
                    //default to lowest value slot
                    crmSlot = "257260004";
                }

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
                itemStats.Add(slotType);
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
           
        }

        private static void InsertIntoCRM(int start, int end)
        {
            CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString);
            IOrganizationService crmService = crmConn.OrganizationServiceProxy;


            Console.WriteLine(crmConn.IsReady);

            for (int i = start; i < end; i++)
            {
                List<string> itemStats = ParseClassicWowHead(i);
                Console.WriteLine("Parsing Item: " + i);
                

                if (itemStats[0] == "Item not found or skipped!")
                {
                }
                else
                {
                    if (itemStats[8] == "14")
                    {
                        itemStats[8] = "257260000";
                    }

                    Guid defaultBU = new Guid("{FDF40AEB-1C6E-E911-A99F-000D3A1ABFB7}");
                    OptionSetValue rarity = new OptionSetValue(Int32.Parse(itemStats[10]));
                    OptionSetValue slot = new OptionSetValue(Int32.Parse(itemStats[11]));

                    QueryExpression query = new QueryExpression("wowc_loot");
                    query.ColumnSet.AddColumns("wowc_itemid", "wowc_lootid");
                    query.Criteria = new FilterExpression();
                    query.Criteria.AddCondition("wowc_itemid", ConditionOperator.Equal, itemStats[0]);

                    EntityCollection results = crmService.RetrieveMultiple(query);

                    int totalrecords = results.Entities.Count;

                    if (totalrecords == 0)
                    {
                        try
                        {
                            Entity loot = new Entity("wowc_loot");
                            loot["wowc_name"] = itemStats[1];
                            loot["wowc_ilvl"] = Int32.Parse(itemStats[2]);
                            loot["wowc_itemid"] = itemStats[0];
                            loot["wowc_rarity"] = rarity;
                            loot["wowc_slot"] = slot;
                            //loot["businessunitid"] = new EntityReference("businessunit", defaultBU);
                            //loot["timezonecode"] = 33;
                            crmService.Create(loot);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            throw;
                        }
                    }
                    else
                    {
                        foreach (var a in results.Entities)
                        {
                            if (a.Contains("wowc_itemid") && a["wowc_itemid"] != null)
                            {
                                Console.WriteLine("{0} {1}", a.GetAttributeValue<string>("wowc_itemid"), a.GetAttributeValue<Guid>("wowc_lootid"));
                                try
                                {
                                    Entity loot = new Entity("wowc_loot");
                                    loot.Id = a.GetAttributeValue<Guid>("wowc_lootid");
                                    loot["wowc_name"] = itemStats[1];
                                    loot["wowc_ilvl"] = Int32.Parse(itemStats[2]);
                                    loot["wowc_itemid"] = itemStats[0];
                                    loot["wowc_rarity"] = rarity;
                                    loot["wowc_slot"] = slot;

                                    crmService.Update(loot);
                                    
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    throw;
                                }
                            }
                            else
                            {

                            }
                        }
                       
                    }
                    
                }
            }
        }

        private static void BuildCsvFile(int start, int end)
        {
            var csv = new StringBuilder();
            csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}" +
                ",{7},{8},{9},{10},{11},{12}", 
                "itemId", "itemName", "itemLvl", "quality", "qualityName", "classId", "classIdName", 
                "subClassId", "inventorySlot", "inventorySlotName", "crmRarity", "crmSlot", "slotType"));

            for (int i = start; i < end; i++)
            {
                List<string> itemStats = ParseClassicWowHead(i);
                Console.WriteLine("Parsing Item: " + i);
                if (itemStats[0]== "Item not found or skipped!")
                {
                }
                else
                {
                    var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6}" +
                        ",{7},{8},{9},{10},{11},{12}",
                        itemStats[0], itemStats[1], itemStats[2], itemStats[3], itemStats[4], itemStats[5], itemStats[6],
                        itemStats[7], itemStats[8], itemStats[9], itemStats[10], itemStats[11], itemStats[12]);
                    csv.AppendLine(newLine);
                }   
            }
            File.WriteAllText(@"C:\GitHub\natearms\WoW\ClassicDB Item Scrapper\ClassicDB Item Scraper\Files\test.csv", csv.ToString());
        }

    }

}
