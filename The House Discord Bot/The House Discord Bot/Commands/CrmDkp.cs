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
    public class CrmDkp : ModuleBase<SocketCommandContext>
    {
        [Group("-dkp"), Summary("Users DKP breakdown by PR/EP/GP")]
        public class DkpModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }

            [Command("")]
            public async Task ReturnPlayersPrEpGP()
            {
                List<SocketUser> list = new List<SocketUser>();
                list.Add(Context.Message.Author);
                list.AsReadOnly();

                IReadOnlyCollection<SocketUser> mentionedUsers = list;
                EntityCollection sortedEntityCollection = GetUsersFromIReadOnlyCollection(mentionedUsers, crmService);

                await ReplyAsync(null, false, BuildUsersDKP(sortedEntityCollection, Context.Message.Author).Build());
            }


            [Command("")]
            public async Task ReturnMentionPrEpGp(IUser mentionedUser)
            {
                IReadOnlyCollection<SocketUser> mentionedUsers = Context.Message.MentionedUsers;
                EntityCollection sortedEntityCollection = GetUsersFromIReadOnlyCollection(mentionedUsers, crmService);

                await ReplyAsync(null, false, BuildUsersDKP(sortedEntityCollection, Context.Message.Author).Build());
            }

            [Command("")]
            public async Task ReturnMentionsPrEpGp([Remainder]string test)
            {
                IReadOnlyCollection<SocketUser> mentionedUsers = Context.Message.MentionedUsers;
                EntityCollection sortedEntityCollection = GetUsersFromIReadOnlyCollection(mentionedUsers,crmService);
                
                if (mentionedUsers.Count == 0)
                    await ReplyAsync("You must provide a mention in order to use the multi mention feature");
                else if (mentionedUsers.Count > 10)
                {
                    await ReplyAsync("You've requested too many users, please limit results to 10 at a time.");
                }
                else
                {
                    await ReplyAsync(null, false, BuildUsersDKP(sortedEntityCollection, Context.Message.Author).Build());
                }
            }
            
            [Command("-top")]
            public async Task PrEpGpMentions(int returnRange)
            {
                await ReplyAsync(null, false, BuildUsersDKP(GetTopUserEpGp(returnRange,crmService),Context.Message.Author).Build());
            }
            
            private EntityCollection GetTopUserEpGp(int returnCount, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("lastname","wowc_totalpr", "wowc_totalep", "wowc_totalgp");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
                query.Orders.Add(new OrderExpression("wowc_totalpr", OrderType.Descending));
                query.Orders.Add(new OrderExpression("wowc_totalep", OrderType.Descending));
                query.PageInfo = new PagingInfo();
                query.PageInfo.Count = returnCount;
                query.PageInfo.PageNumber = 1;

                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
            }
            private EntityCollection GetUserEpGp(string[] userName, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("lastname","wowc_totalpr", "wowc_totalep", "wowc_totalgp");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
                query.Criteria.AddCondition("lastname", ConditionOperator.In, userName);
                query.Orders.Add(new OrderExpression("wowc_totalpr", OrderType.Descending));
                query.Orders.Add(new OrderExpression("wowc_totalep", OrderType.Descending));
                
                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
            }
            private EmbedBuilder BuildUsersDKP(EntityCollection providedUser, IUser initializedUser)
            {

                EmbedBuilder prBuilder = new EmbedBuilder();

                string guildNickname = Context.Guild.GetUser(initializedUser.Id).Nickname;
                string userNickname = initializedUser.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                string commentString = "```" + "Name".PadRight(15) + "Total PR".PadLeft(12) + "Total EP".PadLeft(12) + "Total GP".PadLeft(12);
                string mentionGuildOwner = "";

                if (providedUser.Entities.Count == 0)
                {
                    return prBuilder.WithDescription(Context.Guild.Owner.Mention + ", this user doesn't exist, can you look into this?");
                }
                else
                {
                    for (int i = 0; i < providedUser.Entities.Count; i++)
                    {
                        string guildMember = providedUser.Entities[i].GetAttributeValue<string>("lastname");
                        string totalPr = providedUser.Entities[i].GetAttributeValue<Decimal>("wowc_totalpr").ToString("0.##");
                        string totalEp = providedUser.Entities[i].GetAttributeValue<Decimal>("wowc_totalep").ToString("0.##");
                        string TotalGp = providedUser.Entities[i].GetAttributeValue<Decimal>("wowc_totalgp").ToString("0.##");
                        if (guildMember == userName)
                            guildMember = "*" + guildMember;

                        commentString += "\n" + guildMember.PadRight(15, '.') + totalPr.ToString().PadLeft(12, '.') + totalEp.ToString().PadLeft(12, '.') + TotalGp.ToString().PadLeft(12, '.');

                    }
                }
                
                commentString += "```";
                prBuilder.WithDescription(mentionGuildOwner + "\n" + commentString);
                return prBuilder;
            }
            private EntityCollection GetUsersFromIReadOnlyCollection(IReadOnlyCollection<SocketUser> mentionedUsers, IOrganizationService service)
            {
                EntityCollection results;
                string[] userSearch= new string[mentionedUsers.Count];
                for (int i = 0; i < mentionedUsers.Count; i++)
                {
                    string guildNickname = Context.Guild.GetUser(mentionedUsers.ElementAt(i).Id).Nickname;
                    string userNickname = mentionedUsers.ElementAt(i).Username;

                    string userName = guildNickname == null ? userNickname : guildNickname;
                    
                    userSearch[i] = userName;
                        
                }
                results = GetUserEpGp(userSearch, crmService);
                
                return results;
            }
        }
        
    }
}
