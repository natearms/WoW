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

            [Command("-s")]
            public async Task ReturnPlayersPrEpGP()
            {
                List<SocketUser> list = new List<SocketUser>();
                list.Add(Context.Message.Author);
                IReadOnlyCollection<SocketUser> mentionedUsers = list;

                EntityCollection sortedEntityCollection = GetUsersFromIReadOnlyCollection(mentionedUsers, crmService);

                await ReplyAsync(null, false, BuildUsersDKP(sortedEntityCollection, mentionedUsers, Context.Message.Author).Build());
            }


            [Command("-s")]
            public async Task ReturnMentionPrEpGp(IUser mentionedUser)
            {
                IReadOnlyCollection<SocketUser> mentionedUsers = Context.Message.MentionedUsers;
                EntityCollection sortedEntityCollection = GetUsersFromIReadOnlyCollection(mentionedUsers, crmService);

                await ReplyAsync(null, false, BuildUsersDKP(sortedEntityCollection, mentionedUsers, Context.Message.Author).Build());
            }

            [Command("-s")]
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
                    await ReplyAsync(null, false, BuildUsersDKP(sortedEntityCollection, mentionedUsers, Context.Message.Author).Build());
                }
            }
            
            [Command("-top")]
            public async Task ReturnTopPrEpGp(int returnRange)
            {
                IReadOnlyCollection<SocketUser> mentionedUsers = new List<SocketUser>();

                await ReplyAsync(null, false, BuildUsersDKP(GetTopUserEpGp(returnRange,crmService), mentionedUsers, Context.Message.Author).Build());
            }

            [Command("-class")]
            public async Task ReturnClassPrEpGp([Remainder]params string[] classesSpecified)
            {
                
                IReadOnlyCollection<SocketUser> mentionedUsers = new List<SocketUser>();

                Int32[] classIds = classesSpecified.Select(c => getClassId(c)).ToArray();

                if (classIds.All(id=> id > 0))
                {
                    await ReplyAsync(null, false, BuildUsersDKP(GetClassEpGp(classIds, crmService), mentionedUsers, Context.Message.Author).Build());
                }
                    
                else
                    await ReplyAsync("Sorry but one of " + String.Join(", ", classesSpecified) + " is not a valid class.");
                
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
                Priest,
                Rogue,
                Shaman,
                Warlock,
                Warrior
            }
            private EntityCollection GetUsersFromIReadOnlyCollection(IReadOnlyCollection<SocketUser> mentionedUsers, IOrganizationService service)
            {
                EntityCollection results;
                string[] userSearch = new string[mentionedUsers.Count];
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
            private EntityCollection GetUserEpGp(string[] userName, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("lastname", "wowc_totalpr", "wowc_totalep", "wowc_totalgp");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
                query.Criteria.AddCondition("lastname", ConditionOperator.In, userName);
                query.Orders.Add(new OrderExpression("wowc_totalpr", OrderType.Descending));
                query.Orders.Add(new OrderExpression("wowc_totalep", OrderType.Descending));

                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
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
            private EntityCollection GetClassEpGp(Int32[] classesSpecified, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("lastname", "wowc_totalpr", "wowc_totalep", "wowc_totalgp");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
                foreach (Int32 classSpecified in classesSpecified)
                {
                    query.Criteria.AddCondition("wowc_class", ConditionOperator.Equal, classSpecified);
                }

                query.Orders.Add(new OrderExpression("wowc_totalpr", OrderType.Descending));
                query.Orders.Add(new OrderExpression("wowc_totalep", OrderType.Descending));
                
                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
            }
            
            private EmbedBuilder BuildUsersDKP(EntityCollection providedUser, IReadOnlyCollection<SocketUser> mentionedCollection, IUser author)
            {
                bool usersFound = false;

                string authorGuildNickname = Context.Guild.GetUser(author.Id).Nickname;
                string authorNickname = author.Username;
                string authorUserName = authorGuildNickname == null ? authorNickname : authorGuildNickname;

                EmbedBuilder prBuilder = new EmbedBuilder();

                string missingUsers = null;
                string commentString = "```" + "Name".PadRight(15) + "Total PR".PadLeft(12) + "Total EP".PadLeft(15) + "Total GP".PadLeft(12);
                string missingUserCommentString = null;

                for (int i = 0; i < providedUser.Entities.Count; i++)
                {
                    string guildMember = providedUser.Entities[i].GetAttributeValue<string>("lastname");
                    string totalPr = providedUser.Entities[i].GetAttributeValue<Decimal>("wowc_totalpr").ToString("0.###");
                    string totalEp = providedUser.Entities[i].GetAttributeValue<Decimal>("wowc_totalep").ToString("0.###");
                    string TotalGp = providedUser.Entities[i].GetAttributeValue<Decimal>("wowc_totalgp").ToString("0.###");
                    if (guildMember == authorUserName)
                        guildMember = "*" + guildMember;

                    commentString += "\n" + guildMember.PadRight(15, '.') + totalPr.ToString().PadLeft(12, '.') + totalEp.ToString().PadLeft(15, '.') + TotalGp.ToString().PadLeft(12, '.');

                }

                for (int iMentions = 0; iMentions < mentionedCollection.Count; iMentions++)
                {
                    bool userFound = false;

                    string guildNickname = Context.Guild.GetUser(mentionedCollection.ElementAt(iMentions).Id).Nickname;
                    string userNickname = mentionedCollection.ElementAt(iMentions).Username;
                    string userName = guildNickname == null ? userNickname : guildNickname;

                    for (int iEntities = 0; iEntities < providedUser.Entities.Count; iEntities++)
                    {
                        string providedUserResult = providedUser.Entities[iEntities].GetAttributeValue<string>("lastname");

                        if (userName.ToLower() == providedUserResult.ToLower())
                        {
                            userFound = true;
                            usersFound = true;
                            break;
                        }
                    }
                    if (!userFound)
                    {
                        if (missingUsers == null)
                        {
                            missingUsers += userName;
                            missingUserCommentString += "\n **" + userName + "** was not found in CRM.";
                        }
                        
                        else
                        {
                            missingUsers += ", " + userName;
                            missingUserCommentString += "\n **" + userName + "** was not found in CRM.";
                        }
                            
                    }
                }
                commentString += "```";

                if (usersFound && missingUsers != null)
                    prBuilder.WithDescription(commentString + missingUserCommentString + "\n\n" + Context.Guild.Owner.Mention +", could you please look into this.");
                else if (providedUser.Entities.Count > 0 && missingUsers == null)
                    prBuilder.WithDescription(commentString);
                else
                    prBuilder.WithDescription(Context.Guild.Owner.Mention +" **"+ missingUsers + "** was not found in CRM.");

                    return prBuilder;
            }
            
        }
        
    }
}
