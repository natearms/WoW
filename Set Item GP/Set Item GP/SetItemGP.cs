using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;

namespace Set_Item_GP
{
    class SetItemGP
    {
        static void Main(string[] args)
        {
            CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString);
            IOrganizationService crmService = crmConn.OrganizationServiceProxy;

            string[] slotTypes = new string[7]
            { "257260000", //2h weapon
                "257260001", //1h weapon
                "257260002", //Head, Chest, Legs
                "257260003", //Shoulder, Hands, Waist, Feet, Trinket
                "257260004", //Wrist, Neck, Back, Finger, Off-hand, Wand, Relic, Bag
                "257260005", //Shield
                "257260006" //Ranged Weapon
            };
            /*
            slotTypes[0] = 257260000; //2h weapon
            slotTypes[1] = 257260001; //1h weapon
            slotTypes[2] = 257260002; //Head, Chest, Legs
            slotTypes[3] = 257260003; //Shoulder, Hands, Waist, Feet, Trinket
            slotTypes[4] = 257260004; //Wrist, Neck, Back, Finger, Off-hand, Wand, Relic, Bag
            slotTypes[5] = 257260005; //Shield
            slotTypes[6] = 257260006; //Ranged Weapon
            */

            QueryExpression itemQuery = new QueryExpression("wowc_loot");
            itemQuery.ColumnSet.AddColumns("wowc_lootid", "wowc_itemid","wowc_ilvl", "wowc_name", "wowc_rarity", "wowc_slot","wowc_defaultgp", "wowc_tankgpvalue", "wowc_huntergpvalue");
            itemQuery.Criteria = new FilterExpression();
            itemQuery.Criteria.AddCondition("wowc_slot", ConditionOperator.In, slotTypes);
            //itemQuery.Criteria.AddCondition("wowc_name", ConditionOperator.Like, "%Arcanist%");

            EntityCollection results = crmService.RetrieveMultiple(itemQuery);

            foreach (var a in results.Entities)
            {
                

                Decimal[] slotModifiers = SlotModifier(a.GetAttributeValue<OptionSetValue>("wowc_slot").Value);
                int rarityValue = RarityValue(a.GetAttributeValue<OptionSetValue>("wowc_rarity").Value);

                double defaultGp = Math.Round((4 * Math.Pow(2, ((double)a.GetAttributeValue<int>("wowc_ilvl")/28 + (rarityValue - 4))) * (double)slotModifiers[0]));
                double hunterGp = Math.Round((4 * Math.Pow(2, ((double)a.GetAttributeValue<int>("wowc_ilvl") / 28 + (rarityValue - 4))) * (double)slotModifiers[1]));
                double tankGp = Math.Round((4 * Math.Pow(2, ((double)a.GetAttributeValue<int>("wowc_ilvl") / 28 + (rarityValue - 4))) * (double)slotModifiers[2]));
                                
                Entity loot = new Entity("wowc_loot");
                loot.Id = a.GetAttributeValue<Guid>("wowc_lootid");
                loot["wowc_defaultgp"] = defaultGp;
                loot["wowc_huntergpvalue"] = hunterGp > 0 ? hunterGp : (double?)null;
                loot["wowc_tankgpvalue"] = tankGp > 0 ? tankGp : (double?)null;

                crmService.Update(loot);

                Console.WriteLine("Updating {0} GP Values - Default GP: {1}, Hunter GP: {2}, Tank GP: {3}"
                    , a.Attributes["wowc_name"], defaultGp, hunterGp, tankGp);

            }
            Console.ReadLine();
        }
        private static Decimal[] SlotModifier(int wowc_slot)
        {
            Decimal defaultSlotMod = 0;
            Decimal hunterSlotMod = 0;
            Decimal tankSlotMod = 0;

            //Item Slot == 2H Weapon
            if (wowc_slot == 257260000)
            {
                defaultSlotMod = 2;
                hunterSlotMod = 1;
            }
            //Item Slot == 1H Weapon
            else if (wowc_slot == 257260001)
            {
                defaultSlotMod = 1.5M;
                hunterSlotMod = .5M;
                tankSlotMod = .5M;
            }
            //Item Slot == Head, Chest, Legs
            else if (wowc_slot == 257260002)
            {
                defaultSlotMod = 1;
            }
            //Item Slot == Shoulder, Hands, Waist, Feet, Trinket
            else if (wowc_slot == 257260003)
            {
                defaultSlotMod = .75M;
            }
            //(Item Slot == Wrist, Neck, Back, Finger, Off-hand, Wand, Relic, Bag
            else if (wowc_slot == 257260004)
            {
                defaultSlotMod = .5M;
            }
            //Item Slot == Shield
            else if (wowc_slot == 257260005)
            {
                defaultSlotMod = .5M;
                tankSlotMod = 1.5m;
            }
            //Item Slot == Ranged Weapon
            else if (wowc_slot == 257260006)
            {
                defaultSlotMod = .5M;
                hunterSlotMod = 1.5M;
            }
            
            Decimal[] results = new Decimal[3] { defaultSlotMod, hunterSlotMod, tankSlotMod };
            return results;
        }
        private static int RarityValue(int rarity)
        {
            int rarityValue = 0;

            switch (rarity)
            {
                case 257260004:
                    rarityValue = 1;
                    break;
                case 257260000:
                    rarityValue = 2;
                    break;
                case 257260001:
                    rarityValue = 3;
                    break;
                case 257260002:
                    rarityValue = 4;
                    break;
                case 257260003:
                    rarityValue = 5;
                    break;
                default:

                    break;
            }

            int results = rarityValue;
            return results;
        }
    }
}
