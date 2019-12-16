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
using The_House_Discord_Bot.Utilities;

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
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;

                EmbedBuilder embed = new EmbedBuilder();
                    embed.Description = ResultsFormatter.FormatResultsIntoTable(GetEPDonations(crmService), triggeredBy, new string[] { "Name", "EP Value"}, new string[] { "wowc_name", "wowc_epvalue"});

                await ReplyAsync(null, false, embed.Build());
            }
            [Command("-weekly"), Summary("Last 7 days of EP donations total.")]
            public async Task ReturnWeeklyDonationEP()
            {
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;

                EntityCollection contactSearch = GetUserGuid(triggeredBy, crmService);

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
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;

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
                EntityCollection fetchResults = crmService.RetrieveMultiple(
                    new FetchExpression(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
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
                                </fetch>"));

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
        }
        [Group("-gp"), Summary("GP Value of Item")]
        public class GPModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }
            [Command("-s"), Summary("Return GP Values on items")]
            public async Task ReturnGPValues([Remainder]string itemSearch)
            {
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;
                bool hunterGp = false;

                EntityCollection fetchResults = crmService.RetrieveMultiple(
                    new FetchExpression($@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='wowc_loot'>
                                                <attribute name='wowc_name' />
                                                <attribute name='wowc_huntergpvalue' />
                                                <attribute name='wowc_defaultgp' />
                                                <attribute name='wowc_lootid' />
                                                <order attribute='wowc_name' descending='false' />
                                                <filter type='and'>
                                                  <condition attribute='statecode' operator='eq' value='0' />
                                                  <condition attribute='wowc_name' operator='like' value='%{itemSearch}%' />
                                                  <condition attribute='wowc_defaultgp' operator='gt' value='0' />
                                                </filter>
                                              </entity>
                                            </fetch>"));

               if (fetchResults.Entities.Count == 0)
                {
                    await ReplyAsync("I could not find any records with your search in The Butler.", false, null);
                }
                else
                {
                    foreach (var entity in fetchResults.Entities)
                    {
                        if (entity.Contains("wowc_huntergpvalue"))
                        {
                            hunterGp = true;
                        }
                    }

                    EmbedBuilder embed = new EmbedBuilder();
                        embed.Description = ResultsFormatter.FormatResultsIntoTable(fetchResults, triggeredBy, hunterGp ? new string[] { "Name", "GP Value", "Hunter GP"} : new string[] { "Name", "GP Value" }, hunterGp ? new string[] { "wowc_name", "wowc_defaultgp", "wowc_huntergpvalue"} : new string[] { "wowc_name", "wowc_defaultgp"});

                    await ReplyAsync(null, false, embed.Build());
                }
            }
        }
    }    
}
