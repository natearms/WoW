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
    public class EPGP : ModuleBase<SocketCommandContext>
    {
        [Group("-ep"), Summary("Effort Point Query Commands")]
        public class EPModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }
            [Command("-donations"), Summary("Donations accepted that reward EP.")]
            public async Task ReturnEPDonations()
            {
                await ReplyAsync(null, false, epFormatEmbed(GetEPDonations(crmService), crmService).Build());
            }
            [Command("-weekly"), Summary("Last 7 days of EP donations total.")]
            public async Task ReturnWeeklyDonationEP()
            {
                var author = Context.Message.Author;
                string guildNickname = Context.Guild.GetUser(author.Id).Nickname;
                string userNickname = author.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                Decimal weeklyDonation = 0;

                EntityCollection donatedRecords = GetWeeklyDonation(userName, crmService);

                for (int i = 0; i < donatedRecords.Entities.Count; i++)
                {
                    weeklyDonation += donatedRecords.Entities[i].GetAttributeValue<Decimal>("wowc_ep");
                }

                if (weeklyDonation > 0)
                {
                    await ReplyAsync("Thank you for your donations! You have donated **" + weeklyDonation.ToString("N3") + " EP** worth of mats to the guild bank in the last 7 days.", false, null);
                }
                else
                {
                    await ReplyAsync("It does look like you've donated anything with in the last 7 days that give EP.", false, null);
                }
                
            }

            private EntityCollection GetEPDonations(IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("wowc_loot");
                query.ColumnSet.AddColumns("wowc_name", "wowc_epvalue");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
                query.Criteria.AddCondition("wowc_marketrate", ConditionOperator.GreaterThan, 0);
                query.Criteria.AddCondition("wowc_epvalue", ConditionOperator.GreaterThan, 0);
                query.Orders.Add(new OrderExpression("wowc_name", OrderType.Ascending));

                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
            }
            private EntityCollection GetWeeklyDonation(string guildMember, IOrganizationService crmService)
            {
                QueryExpression contactQuery = new QueryExpression("contact");
                contactQuery.ColumnSet.AddColumns("lastname");
                contactQuery.Criteria = new FilterExpression();
                contactQuery.Criteria.AddCondition("lastname", ConditionOperator.Equal, guildMember);
                contactQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
                EntityCollection contactResult = crmService.RetrieveMultiple(contactQuery);

                if (contactResult.Entities.Count == 0)
                    return contactResult;

                QueryExpression query = new QueryExpression("wowc_effortpoint");
                query.ColumnSet.AddColumns("wowc_raidmember", "wowc_ep");

                FilterExpression donatedRecords = new FilterExpression(LogicalOperator.And);
                donatedRecords.AddCondition("wowc_efforttype", ConditionOperator.Equal, 257260003);
                donatedRecords.AddCondition("wowc_ep", ConditionOperator.GreaterThan, 0);

                FilterExpression adjustmentRecords = new FilterExpression(LogicalOperator.And);
                adjustmentRecords.AddCondition("wowc_efforttype", ConditionOperator.Equal, 257260006);
                adjustmentRecords.AddCondition("wowc_ep", ConditionOperator.LessThan, 0);
                adjustmentRecords.AddCondition("subject", ConditionOperator.NotLike, "%decay%");

                FilterExpression effortRecordsCombined = new FilterExpression(LogicalOperator.Or);
                effortRecordsCombined.AddFilter(donatedRecords);
                effortRecordsCombined.AddFilter(adjustmentRecords);

                query.Criteria = new FilterExpression(LogicalOperator.And);
                query.Criteria.AddCondition("wowc_raidmember", ConditionOperator.Equal, contactResult.Entities[0].Id);
                query.Criteria.AddCondition("createdon", ConditionOperator.GreaterEqual, DateTime.Now.AddDays(-7));
                query.Criteria.AddFilter(effortRecordsCombined);

                EntityCollection results = crmService.RetrieveMultiple(query);
                return results;

            }
            private EmbedBuilder epFormatEmbed(EntityCollection epRecords, IOrganizationService crmService)
            {
                EmbedBuilder epBuilder = new EmbedBuilder();
                int nameLength = 0;                

                for (int i = 0; i < epRecords.Entities.Count; i++)
                {
                    if(epRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length > nameLength)
                    {
                        nameLength = epRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length;
                    }
                }

                string commentString = "```" + "Name".PadRight(nameLength) + "EP Value".PadLeft(10);

                for (int i = 0; i < epRecords.Entities.Count; i++)
                {
                    string recordName = epRecords.Entities[i].GetAttributeValue<string>("wowc_name");
                    string epValue = epRecords.Entities[i].GetAttributeValue<Decimal>("wowc_epvalue").ToString("N3");

                    commentString += "\n" + recordName.PadRight(nameLength, '.') + epValue.PadLeft(10,'.');
                }
                commentString += "```";

                epBuilder.WithDescription(commentString);

                return epBuilder;
            }
        }
        [Group("-gp"), Summary("GP Value of Item")]
        public class GPModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }
            [Command("-s"), Summary("Return GP Values on items")]
            public async Task ReturnGPValues([Remainder]string itemSearch)
            {
                await ReplyAsync(null, false, GpFormatEmbed(GetGPValues(itemSearch,crmService),crmService).Build());
            }
            private EntityCollection GetGPValues(string itemName, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("wowc_loot");
                query.ColumnSet.AddColumns("wowc_name", "wowc_defaultgp","wowc_huntergpvalue");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
                query.Criteria.AddCondition("wowc_name", ConditionOperator.Like, "%" + itemName + "%");
                query.Orders.Add(new OrderExpression("wowc_name", OrderType.Ascending));

                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
            }
            private EmbedBuilder GpFormatEmbed(EntityCollection gpRecords, IOrganizationService crmService)
            {
                EmbedBuilder gpBuilder = new EmbedBuilder();

                int nameLength = 0;
                bool containsHunterValues = false;

                for (int i = 0; i < gpRecords.Entities.Count; i++)
                {
                    if (gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length > nameLength)
                    {
                        nameLength = gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length >= 30 ? 30 : gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length;

                        if (gpRecords.Entities[i].GetAttributeValue<Decimal>("wowc_huntergpvalue") > 0)
                            containsHunterValues = true;
                    }
                }

                string commentString = "";
                if (containsHunterValues)
                {
                    commentString += "```" + "Name".PadRight(nameLength+2) + "GP Value".PadLeft(10) + "Hunter GP".PadLeft(12);
                }
                else
                {
                    commentString += "```" + "Name".PadRight(nameLength+2) + "GP Value".PadLeft(10);
                }
                

                for (int i = 0; i < gpRecords.Entities.Count; i++)
                {
                    if (containsHunterValues)
                    {
                        string recordName = gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length >= 30 ? gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Substring(0,30) : gpRecords.Entities[i].GetAttributeValue<string>("wowc_name");
                        string gpValue = gpRecords.Entities[i].GetAttributeValue<Decimal>("wowc_defaultgp").ToString("N3");
                        string hunterGpValue = gpRecords.Entities[i].GetAttributeValue<Decimal>("wowc_huntergpvalue").ToString("N3");

                        commentString += "\n" + recordName.PadRight(nameLength+2, '.') + gpValue.PadLeft(10,'.') + hunterGpValue.PadLeft(12,'.');
                    }
                    else
                    {
                        string recordName = gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length >= 30 ? gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Substring(0, 30) : gpRecords.Entities[i].GetAttributeValue<string>("wowc_name");
                        string gpValue = gpRecords.Entities[i].GetAttributeValue<Decimal>("wowc_defaultgp").ToString("N3");

                        commentString += "\n" + recordName.PadRight(nameLength+2, '.') + gpValue.PadLeft(10,'.');
                    }
                    
                }
                commentString += "```";

                gpBuilder.WithDescription(commentString);

                return gpBuilder;
            }
        }
    }
     
}
