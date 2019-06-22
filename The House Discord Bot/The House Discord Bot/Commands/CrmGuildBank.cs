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
                await ReplyAsync(null, false, BuildGuildBankHighNeedList(crmService).Build());
            }

            [Command("-s"), Summary("Searches the guild bank with string criteria.")]
            public async Task GuildBankSearch([Remainder] string itemSearch)
            {
                await ReplyAsync(null, false, BuildGuildBankList(itemSearch, crmService).Build());
            }
            [Command("-a"), Summary("Returns audit history for an item.")]
            public async Task GuildBankAuditHistory([Remainder] string itemSearch)
            {
                await ReplyAsync(null, false, GuildBankAuditEmbedBuilder(itemSearch, crmService).Build());
            }

            private EmbedBuilder BuildGuildBankList(string itemSearch, IOrganizationService crmService)
            {
                EntityCollection guildBankRecord = GetGuildBankRecords(itemSearch, crmService);
                EmbedBuilder prBuilder = GuildBankEmbedBuilder(guildBankRecord);

                if (guildBankRecord.Entities.Count == 0)
                {
                    prBuilder.WithDescription("I could not find a guild bank record matching criteria " + itemSearch +
                        " in the guild bank.  Please make sure you spelled the item name or part of the item name correctly and try again.");
                    return prBuilder;
                }
                prBuilder.WithTitle("Here is what I found with your search criteria **\"" + itemSearch + "\"**");
                return prBuilder;

            }
            private EmbedBuilder BuildGuildBankHighNeedList(IOrganizationService crmService)
            {
                EntityCollection guildBankRecord = GetGuildBankHighNeedRecords(crmService);

                EmbedBuilder prBuilder = GuildBankEmbedBuilder(guildBankRecord);

                if (guildBankRecord.Entities.Count == 0)
                {
                    prBuilder.WithDescription("There doesn't seem to be any guild bank records in high need right now, please check back later.");
                    return prBuilder;
                }
                prBuilder.WithTitle("Below is a list of high need items for the guild bank.");
                return prBuilder;

            }
            private EmbedBuilder GuildBankEmbedBuilder(EntityCollection guildBankRecords)
            {
                EmbedBuilder prBuilder = new EmbedBuilder();

                string commentString = "```" + "Item Name".PadRight(35) + "Inventory".PadRight(13) + "High Need";
                for (int i = 0; i < guildBankRecords.Entities.Count; i++)
                {
                    string itemName = guildBankRecords.Entities[i].GetAttributeValue<string>("wowc_name");
                    string inventory = guildBankRecords.Entities[i].GetAttributeValue<int>("wowc_inventory").ToString();
                    string highNeed = guildBankRecords.Entities[i].GetAttributeValue<bool>("wowc_highneed") ? "Yes" : "No";

                    itemName = itemName.Length > 35 ? itemName.Substring(0, 35) : itemName;

                    commentString += "\n" + itemName.PadRight(39, '.') + inventory.ToString() + highNeed.PadLeft(18 - inventory.Length, '.');

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
