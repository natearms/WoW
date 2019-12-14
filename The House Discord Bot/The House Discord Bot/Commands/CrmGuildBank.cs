using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Configuration;
using System.IdentityModel.Metadata;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
namespace The_House_Discord_Bot.Commands
{
    public class CrmGuildBank: ModuleBase<SocketCommandContext>
    {

        [Group("-gb"), Summary("Guild Bank record commands")]
        public class GuildBankModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }

            [Command("-hn"), Summary("Searches the guild bank for high need mats.")]
            public async Task GuildBankHighNeed()
            {
                string responseText = "";
                EntityCollection guildBankRecord = GetGuildBankHighNeedRecords(crmService);

                if (guildBankRecord.Entities.Count == 0)
                {
                    responseText = "There doesn't seem to be any guild bank records in high need right now, please check back later.";
                    await ReplyAsync(responseText, false, null);
                }
                else
                {
                    responseText = "Below is a list of high need items for the guild bank.";
                    await ReplyAsync(responseText, false, GuildBankEmbedBuilder(guildBankRecord).Build());
                }
            }

            [Command("-s"), Summary("Searches the guild bank with string criteria.")]
            public async Task GuildBankSearch([Remainder] string itemSearch)
            {
                string responseText = "";
                EntityCollection guildBankRecord = GetGuildBankRecords(itemSearch, crmService);

                if(guildBankRecord.Entities.Count == 0)
                {
                    responseText = "I could not find a guild bank record matching criteria **" + itemSearch + "** in the guild bank.  Please make sure you spelled the item name or part of the item name correctly and try again.";
                    await ReplyAsync(responseText, false, null);
                }
                else
                {
                    responseText = "Here is what I found with your search criteria **\"" + itemSearch + "\"**";
                    await ReplyAsync(responseText, false, GuildBankEmbedBuilder(guildBankRecord).Build());
                }
            }
            [Command("-a"), Summary("Returns audit history for an item.")]
            public async Task GuildBankAuditHistory([Remainder] string itemSearch)
            {
                await ReplyAsync(null, false, GuildBankAuditEmbedBuilder(itemSearch, crmService).Build());
            }
            private EmbedBuilder GuildBankEmbedBuilder(EntityCollection guildBankRecords)
            {
                EmbedBuilder prBuilder = new EmbedBuilder();
                int itemNameLength = 0;

                for (int i = 0; i < guildBankRecords.Entities.Count; i++)
                {
                    if(guildBankRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length > itemNameLength)
                    {
                        itemNameLength = guildBankRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length >= 45 ? 45 : guildBankRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length;
                    }
                }

                string commentString = "```" + "Item Name".PadRight(itemNameLength) + "Stock".PadLeft(10);
                for (int i = 0; i < guildBankRecords.Entities.Count; i++)
                {
                    string itemName = guildBankRecords.Entities[i].GetAttributeValue<string>("wowc_name");
                    string inventory = guildBankRecords.Entities[i].GetAttributeValue<int>("wowc_inventory").ToString();
                    string highNeed = guildBankRecords.Entities[i].GetAttributeValue<bool>("wowc_highneed") ? "Yes" : "No";

                    itemName = itemName.Length > 45 ? itemName.Substring(0, 45) : itemName;

                    commentString += "\n" + itemName.PadRight(itemNameLength, '.') + inventory.ToString().PadLeft(10,'.');

                }
                commentString += "```";
                prBuilder.WithDescription(commentString)
                ;

                return prBuilder;
            }
            private EmbedBuilder GuildBankAuditEmbedBuilder(string itemSearch, IOrganizationService service)
            {
                EmbedBuilder prBuilder = new EmbedBuilder();
                EntityCollection guildBankRecord = GetGuildBankRecords(itemSearch, service);
                if (guildBankRecord.Entities.Count > 1)
                    return prBuilder.WithDescription("Your search returned too many guild bank records, please be more specific with your search.");
                if (guildBankRecord.Entities.Count < 1)
                    return prBuilder.WithDescription("I was not able to find any guild bank records with this search criteria.");

                EntityCollection auditHistory = GetGuildBankAuditHistory(guildBankRecord, service);

                string commentString = "```" + "Raid Member".PadRight(15) + "Delta".PadLeft(8) + "Inv.".PadLeft(8);
                int inventoryValue = guildBankRecord.Entities[0].GetAttributeValue<int>("wowc_inventory");
                commentString += "\n" + "Current".PadRight(15, '.') + inventoryValue.ToString().PadLeft(16, '.');
                for (int i = 0; i < auditHistory.Entities.Count; i++)
                {
                    string member = auditHistory.Entities[i].GetAttributeValue<EntityReference>("wowc_raidmember").Name;
                    int inventory = (Int32)auditHistory.Entities[i].GetAttributeValue<Decimal>("wowc_epcount");
                    inventoryValue = inventoryValue - inventory;

                    member = member.Length > 15 ? member.Substring(0, 15) : member;

                    commentString += "\n" + member.PadRight(15, '.') + inventory.ToString().PadLeft(6, '.') + inventoryValue.ToString().PadLeft(10, '.');

                }
                commentString += "```";
                prBuilder.WithDescription(commentString)
                    .WithTitle("Here is the audit history for **" + guildBankRecord.Entities[0].GetAttributeValue<string>("wowc_name") + "**");
                ;

                return prBuilder;
            }
            private static EntityCollection GetGuildBankRecords(string itemSearch, IOrganizationService service)
            {
                QueryExpression query = new QueryExpression("wowc_guildbankrecord");
                query.ColumnSet.AddColumns("wowc_name", "wowc_inventory", "wowc_highneed");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("wowc_name", ConditionOperator.Like, "%" + itemSearch + "%");
                query.Criteria.AddCondition("wowc_inventory", ConditionOperator.GreaterEqual, 1);
                query.Orders.Add(new OrderExpression("wowc_name", OrderType.Ascending));

                EntityCollection results = service.RetrieveMultiple(query);
                return results;

            }
            private static EntityCollection GetGuildBankHighNeedRecords(IOrganizationService service)
            {
                QueryExpression query = new QueryExpression("wowc_guildbankrecord");
                query.ColumnSet.AddColumns("wowc_name", "wowc_inventory", "wowc_highneed");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("wowc_highneed", ConditionOperator.Equal, true);
                query.Orders.Add(new OrderExpression("wowc_name", OrderType.Ascending));

                EntityCollection results = service.RetrieveMultiple(query);
                return results;

            }
            private static EntityCollection GetGuildBankAuditHistory(EntityCollection itemResult, IOrganizationService service)
            {
                QueryExpression query = new QueryExpression("wowc_effortpoint");
                query.ColumnSet.AddColumns("wowc_raidmember", "wowc_epcount");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("wowc_guildbankrecord", ConditionOperator.Equal, itemResult.Entities[0].GetAttributeValue<Guid>("wowc_guildbankrecordid"));
                query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));

                EntityCollection results = service.RetrieveMultiple(query);

                return results;
            }
        }

    }
}
