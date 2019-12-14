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

                EntityCollection contactSearch = GetUserGuid(userName, crmService);

                if (contactSearch.Entities.Count == 0)
                {
                    await ReplyAsync("I could not find a record for you in The Butler.",false,null);
                }
                else if (contactSearch.Entities.Count == 1)
                {
                    Decimal weeklyDonation = 0;

                    EntityCollection donatedRecords = GetWeeklyDonation(contactSearch.Entities[0].GetAttributeValue<Guid>("contactid"), crmService);

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
                        await ReplyAsync("It doesn't look like you've donated anything within the last 7 days that rewards EP.", false, null);
                    }
                }
                
                
            }
            [Command("-topweekly"),Summary("List of users who danted this week.")]
            public async Task ReturnTopWeeklyDonationEP()
            {
                var donations = new List<Tuple<string, Decimal>>();
                EntityCollection usersWhoDonated = GetUsersWhoDonated(crmService);

                for (int i = 0; i < usersWhoDonated.Entities.Count; i++)
                {
                    Decimal epDonation = 0;
                    EntityCollection memberDonations = GetWeeklyDonation(usersWhoDonated.Entities[i].Id, crmService);
                    for (int d = 0; d < memberDonations.Entities.Count; d++)
                    {
                        epDonation += memberDonations.Entities[d].GetAttributeValue<Decimal>("wowc_ep");
                    }
                    if(epDonation > 0 && usersWhoDonated.Entities[i].GetAttributeValue<string>("lastname").ToLower() != "guild bank")
                    {
                        donations.Add(new Tuple<string, Decimal>(usersWhoDonated.Entities[i].GetAttributeValue<string>("lastname"), epDonation));
                    }
                }

                donations = donations.OrderByDescending(t => t.Item2).ToList();

                EmbedBuilder topWeeklyDonations = new EmbedBuilder();
                int nameLength = 0;

                foreach (var userDonation in donations)
                {
                    if (userDonation.Item1.Length > nameLength)
                    {
                        nameLength = userDonation.Item1.Length;
                    }
                }

                string commentString = "```" + "Name".PadRight(nameLength) + "Total".PadLeft(10);
                foreach (var userDonation in donations)
                {
                    string recordName = userDonation.Item1;
                    string epValue = userDonation.Item2.ToString("N3");

                    commentString += "\n" + recordName.PadRight(nameLength, '.') + epValue.PadLeft(10, '.');
                }
                
                commentString += "```";

                topWeeklyDonations.WithDescription(commentString);

                await ReplyAsync("Here are the results of who has donated in the last 7 days.", false, topWeeklyDonations.Build());
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
            private EntityCollection GetUsersWhoDonated(IOrganizationService crmService)
            {
                var fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
                                  <entity name='contact'>
                                    <attribute name='lastname' />
                                    <attribute name='contactid' />
                                    <order attribute='lastname' descending='true' />
                                    <link-entity name='wowc_effortpoint' from='wowc_raidmember' to='contactid' link-type='inner' alias='ad'>
                                      <filter type='and'>
                                        <condition attribute='wowc_efforttype' operator='eq' value='257260003' />
                                        <condition attribute='wowc_ep' operator='gt' value='0' />
                                        <condition attribute='createdon' operator='last-seven-days' />
                                      </filter>
                                    </link-entity>
                                  </entity>
                                </fetch>";
                var fetchExpression = new FetchExpression(fetchXml);
                EntityCollection fetchResults = crmService.RetrieveMultiple(fetchExpression);

                return fetchResults;
            }
            private EntityCollection GetUserGuid(string guildMember, IOrganizationService crmService)
            {
                QueryExpression contactQuery = new QueryExpression("contact");
                contactQuery.ColumnSet.AddColumns("lastname", "contactid");
                contactQuery.Criteria = new FilterExpression();
                contactQuery.Criteria.AddCondition("lastname", ConditionOperator.Equal, guildMember);
                contactQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
                EntityCollection contactResult = crmService.RetrieveMultiple(contactQuery);

                return contactResult;
            }
            private EntityCollection GetWeeklyDonation(Guid contactGuid, IOrganizationService crmService)
            {
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
                query.Criteria.AddCondition("wowc_raidmember", ConditionOperator.Equal, contactGuid);
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
