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
    public class CrmDkp : ModuleBase<SocketCommandContext>
    {
        [Group("-dkp"), Summary("Users DKP breakdown by PR/EP/GP")]
        public class DkpModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }

            [Command("-s")]
            public async Task ReturnPlayersPrEpGP()
            {
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;

                EntityCollection fetchResults = crmService.RetrieveMultiple(
                    new FetchExpression($@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                          <entity name='contact'>
                                            <attribute name='wowc_totalpr' />
                                            <attribute name='wowc_totalgp' />
                                            <attribute name='wowc_totalep' />
                                            <attribute name='lastname' />
                                            <attribute name='contactid' />
                                            <order attribute='wowc_totalpr' descending='true' />
                                            <filter type='and'>
                                              <condition attribute='statecode' operator='eq' value='0' />
                                              <condition attribute='lastname' operator='like' value='%{triggeredBy}%' />
                                            </filter>
                                          </entity>
                                        </fetch>"));

                if(fetchResults.Entities.Count == 0)
                {
                    await ReplyAsync("I could not find a record for you in The Butler.",false, null);
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.Description = ResultsFormatter.FormatResultsIntoTable(fetchResults, triggeredBy, new string[] { "Name", "Total PR", "Total EP", "Total GP" }, new string[] { "lastname", "wowc_totalpr", "wowc_totalep", "wowc_totalgp" });

                    await ReplyAsync(null, false, embed.Build());
                }
            }
            [Command("-s")]
            public async Task ReturnMentionPrEpGp(IUser mentionedUser)
            {
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;
                var mentionedUserFilter = "";

                foreach (var users in Context.Message.MentionedUsers)
                {
                    string userName = Context.Guild.GetUser(users.Id).Nickname != null ? Context.Guild.GetUser(users.Id).Nickname : users.Username;
                    mentionedUserFilter += "<condition attribute='lastname' operator='like' value='%"+ userName+"%' />";
                }
                EntityCollection fetchResults = crmService.RetrieveMultiple(
                    new FetchExpression($@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='contact'>
                                                <attribute name='wowc_totalpr' />
                                                <attribute name='wowc_totalgp' />
                                                <attribute name='wowc_totalep' />
                                                <attribute name='lastname' />
                                                <attribute name='contactid' />
                                                <order attribute='wowc_totalpr' descending='true' />
                                                <filter type='and'>
                                                  <condition attribute='statecode' operator='eq' value='0' />
                                                  <filter type='or'>
                                                    {mentionedUserFilter}
                                                  </filter>
                                                </filter>
                                              </entity>
                                            </fetch>"));


                if (fetchResults.Entities.Count == 0)
                {
                    await ReplyAsync("I could not find an active record for you in The Butler.", false, null);
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.Description = ResultsFormatter.FormatResultsIntoTable(fetchResults, triggeredBy, new string[] { "Name", "Total PR", "Total EP", "Total GP" }, new string[] { "lastname", "wowc_totalpr", "wowc_totalep", "wowc_totalgp" });

                    await ReplyAsync(null, false, embed.Build());
                }
            }
            
            [Command("-s")]
            public async Task ReturnMentionsPrEpGp([Remainder]string test)
            {
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;
                var mentionedUserFilter = "";

                foreach (var users in Context.Message.MentionedUsers)
                {
                    string userName = Context.Guild.GetUser(users.Id).Nickname != null ? Context.Guild.GetUser(users.Id).Nickname : users.Username;
                    mentionedUserFilter += "<condition attribute='lastname' operator='like' value='%" + userName + "%' />";
                }
                EntityCollection fetchResults = crmService.RetrieveMultiple(
                    new FetchExpression($@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='contact'>
                                                <attribute name='wowc_totalpr' />
                                                <attribute name='wowc_totalgp' />
                                                <attribute name='wowc_totalep' />
                                                <attribute name='lastname' />
                                                <attribute name='contactid' />
                                                <order attribute='wowc_totalpr' descending='true' />
                                                <filter type='and'>
                                                  <condition attribute='statecode' operator='eq' value='0' />
                                                  <filter type='or'>
                                                    {mentionedUserFilter}
                                                  </filter>
                                                </filter>
                                              </entity>
                                            </fetch>"));


                if (fetchResults.Entities.Count == 0)
                {
                    await ReplyAsync("I could not find an active record for you in The Butler.", false, null);
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.Description = ResultsFormatter.FormatResultsIntoTable(fetchResults, triggeredBy, new string[] { "Name", "Total PR", "Total EP", "Total GP" }, new string[] { "lastname", "wowc_totalpr", "wowc_totalep", "wowc_totalgp" });

                    await ReplyAsync(null, false, embed.Build());
                }
            }
            
            [Command("-top")]
            public async Task ReturnTopPrEpGp(int returnRange)
            {
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;

                EntityCollection fetchResults = crmService.RetrieveMultiple(
                    new FetchExpression($@"<fetch top='{returnRange}' version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                            <entity name='contact'>
                                            <attribute name='wowc_totalpr' />
                                            <attribute name='wowc_totalgp' />
                                            <attribute name='wowc_totalep' />
                                            <attribute name='lastname' />
                                            <attribute name='contactid' />
                                            <order attribute='wowc_totalpr' descending='true' />
                                            <filter type='and'>
                                                <condition attribute='statecode' operator='eq' value='0' />
                                            </filter>
                                            </entity>
                                        </fetch>"));

                EmbedBuilder embed = new EmbedBuilder();
                    embed.Description = ResultsFormatter.FormatResultsIntoTable(fetchResults, triggeredBy, new string[] { "Name", "Total PR", "Total EP", "Total GP" }, new string[] { "lastname", "wowc_totalpr", "wowc_totalep", "wowc_totalgp" });

                await ReplyAsync(null, false, embed.Build());
            }

            [Command("-top")]
            public async Task ReturnTopPrEpGp(int returnRange, [Remainder] string classesSpecified)
            {
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;
                string[] strClasses = classesSpecified.Split(' ');
                Int32[] classIds = strClasses.Select(c => getClassId(c)).ToArray();

                if (classIds.All(id => id > 0))
                {
                    try
                    {
                        EmbedBuilder embed = new EmbedBuilder();
                            embed.Description = ResultsFormatter.FormatResultsIntoTable(GetTopUserEpGp(returnRange, classIds, crmService), triggeredBy, new string[] { "Name", "Total PR", "Total EP", "Total GP" }, new string[] { "lastname", "wowc_totalpr", "wowc_totalep", "wowc_totalgp" });

                        await ReplyAsync(null, false, embed.Build());
                    }
                    catch
                    {
                        await ReplyAsync("Sorry to many users to return, please make your query more specific.");
                    }

                }
                else
                    await ReplyAsync("Sorry but " + String.Join(", ", strClasses) + " is not a valid class.");
            }

            [Command("-class")]
            public async Task ReturnClassPrEpGp([Remainder] string classesSpecified)
            {
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;
                int maxReturnCount = 25;
                Int32[] classIds;
                string all = "all";
                
                string[] strClasses = classesSpecified.Split(' ');

                if (String.Equals(all, classesSpecified))
                {
                    await ReturnTopPrEpGp(25);
                }
                else
                {
                    classIds = strClasses.Select(c => getClassId(c)).ToArray();

                    if (classIds.All(id => id > 0))
                    {
                        try
                        {
                            EmbedBuilder embed = new EmbedBuilder();
                            embed.Description = ResultsFormatter.FormatResultsIntoTable(GetTopUserEpGp(maxReturnCount, classIds, crmService), triggeredBy, new string[] { "Name", "Total PR", "Total EP", "Total GP" }, new string[] { "lastname", "wowc_totalpr", "wowc_totalep", "wowc_totalgp" });

                            await ReplyAsync(null, false, embed.Build());
                        }
                        catch
                        {
                            await ReplyAsync("Sorry to many users to return, please make your query more specific.\n\n Try using -dkp -top <class>");
                        }
                    }
                    else
                        await ReplyAsync("Sorry but one of " + String.Join(", ", strClasses) + " is not a valid class.");
                }
            }

            private Int32 getClassId(string classSpecified)
            {
                Classes eClass = Classes.Druid;
                if (Enum.TryParse<Classes>(classSpecified, true, out eClass))
                {
                    //direct match
                    return (Int32)eClass;
                }
                return 0;
            }
            private enum Classes
            {
                Druid = 257260000,
                Hunter,
                Mage,
                Paladin,
                Priest,
                Rogue,
                Shaman,
                Warlock,
                Warrior
            }
            
            private EntityCollection GetTopUserEpGp(int returnCount, Int32[] classesSpecified, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("lastname", "wowc_totalpr", "wowc_totalep", "wowc_totalgp");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
                FilterExpression classFilter = query.Criteria.AddFilter(LogicalOperator.Or);
                foreach (Int32 classSpecified in classesSpecified)
                {
                    classFilter.AddCondition("wowc_class", ConditionOperator.Equal, classSpecified);

                }
                query.Orders.Add(new OrderExpression("wowc_totalpr", OrderType.Descending));
                query.Orders.Add(new OrderExpression("wowc_totalep", OrderType.Descending));
                query.PageInfo = new PagingInfo();
                query.PageInfo.Count = returnCount;
                query.PageInfo.PageNumber = 1;

                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
            }
        }
    }
}
