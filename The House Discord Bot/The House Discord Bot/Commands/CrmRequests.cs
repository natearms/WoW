using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;

namespace The_House_Discord_Bot.Commands
{
    public class CrmRequests : ModuleBase<SocketCommandContext> 
    {
        [Group("dkp"), Alias("pr, ep, gp"), Summary("Users DKP breakdown by PR/EP/GP")]
        public class DkpModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }
            [Command("")]
            public async Task PrEpGp()
            {
                await ReplyAsync(null, false, BuildUsersDKP(Context.Message.Author));
            }

            [Command("")]
            public async Task PrEpGpMention(IUser mentionedUser)
            {
                await ReplyAsync(null, false, BuildUsersDKP(mentionedUser));
            }

            [Command("")]
            public async Task PrEpGpMentions([Remainder]string test)
            {
                IReadOnlyCollection<SocketUser> mentionedUsers = Context.Message.MentionedUsers;

                if (mentionedUsers.Count == 0)
                    await ReplyAsync("You must provide a mention in order to use the multi mention feature");
                else if (mentionedUsers.Count > 10)
                {
                    await ReplyAsync("You've requested too many users, please limit results to 10 at a time.");
                }
                else
                {
                    for (int i = 0; i < mentionedUsers.Count; i++)
                    {
                        await ReplyAsync(null, false, BuildUsersDKP(mentionedUsers.ElementAt(i)));
                        System.Threading.Thread.Sleep(1000);
                    }
                }

            }
            private EntityCollection GetUserEpGp(string userName, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("wowc_totalpr", "wowc_totalep", "wowc_totalgp");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("lastname", ConditionOperator.Equal, userName);

                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
            }
            private Embed BuildUsersDKP(IUser providedUser)
            {
                //CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString);
                //IOrganizationService crmService = crmConn.OrganizationServiceProxy;
                
                string guildNickname = Context.Guild.GetUser(providedUser.Id).Nickname;
                string userNickname = providedUser.Username;

                string userName = guildNickname == null ? userNickname : guildNickname;
                var userNameMention = providedUser.Mention;

                EntityCollection userInfo = GetUserEpGp(userName, crmService);
                EmbedBuilder prBuilder = new EmbedBuilder();
                if (userInfo.Entities.Count == 0)
                {
                    prBuilder.WithDescription("I could not find a DKP record for " + userNameMention + " in The House CRM.  Please contact " +
                        Context.Guild.Owner.Mention + " to create a record for this user.");
                }
                else
                {
                    prBuilder.WithDescription("**Here is the DKP breakdown for** " + userNameMention +
                    "\n\n**PR: **" + userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalpr").ToString("0.##") +
                    "\n**EP: **" + userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalep").ToString("0.##") +
                    "\n**GP: **" + userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalgp").ToString("0.##") + "\n"
                    )

                    //.WithCurrentTimestamp()
                    ;
                }

                return prBuilder.Build();
            }
        }
        
        [Group("gb")]
        public class GuildBankModule : ModuleBase<SocketCommandContext>
        {

            public IOrganizationService crmService { get; set; }

            [Command("!d"), Summary("Searches the guild bank with string criteria")]
            public async Task guildBank()
            {
                //CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString);
                //IOrganizationService crmService = crmConn.OrganizationServiceProxy;

                await ReplyAsync(null, false, BuildGuildBankHighNeedList(crmService));
                //await ReplyAsync(BuildGuildBankList(itemSearch), false, null);
            }

            [Command("!s"), Summary("Searches the guild bank with string criteria")]
            public async Task guildBank([Remainder] string itemSearch)
            {
                //CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString);
                //IOrganizationService crmService = crmConn.OrganizationServiceProxy;

                await ReplyAsync(null, false, BuildGuildBankList(itemSearch, crmService));
                //await ReplyAsync(BuildGuildBankList(itemSearch), false, null);
            }

            private Embed BuildGuildBankList(string itemSearch, IOrganizationService crmService)
            {
                

                EntityCollection guildBankRecord = GetGuildBankRecords(itemSearch, crmService);

                EmbedBuilder prBuilder = GuildBankEmbedBuilder(guildBankRecord);

                
                if (guildBankRecord.Entities.Count == 0)
                {
                    prBuilder.WithDescription("I could not find a guild bank record matching criteria " + itemSearch +
                        " in the guild bank.  Please make sure you spelt the item name or part of the item name correctly and try again.");
                    return prBuilder.Build();
                }
                prBuilder.WithTitle("Here is what we found with your search criteria **\""+itemSearch+"\"**");
                return prBuilder.Build();
                
            }
            private Embed BuildGuildBankHighNeedList(IOrganizationService crmService)
            {
                EntityCollection guildBankRecord = GetGuildBankHighNeedRecords(crmService);

                EmbedBuilder prBuilder = GuildBankEmbedBuilder(guildBankRecord);

                if (guildBankRecord.Entities.Count == 0)
                {
                    prBuilder.WithDescription("There doesn't seem to be any guild bank records in high demand right now, please check back later.");
                    return prBuilder.Build();
                }
                prBuilder.WithTitle("Below is a list of high need items for the guild bank.");
                return prBuilder.Build();

            }

            private EmbedBuilder GuildBankEmbedBuilder(EntityCollection guildBankRecords)
            {
                //CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString);
                //IOrganizationService crmService = crmConn.OrganizationServiceProxy;

                EmbedBuilder prBuilder = new EmbedBuilder();

                string commentString = "```" + "Item Name".PadRight(35) + "Inventory".PadRight(13) + "High Need";
                 for (int i = 0; i < guildBankRecords.Entities.Count; i++)
                    {
                        string itemName = guildBankRecords.Entities[i].GetAttributeValue<string>("wowc_name");
                        string inventory = guildBankRecords.Entities[i].GetAttributeValue<int>("wowc_inventory").ToString();
                        string highNeed = guildBankRecords.Entities[i].GetAttributeValue<bool>("wowc_highneed") ? "Yes" : "No";

                        commentString += "\n" + itemName.PadRight(39, '.') + inventory.ToString() + highNeed.PadLeft(18 - inventory.Length, '.');

                    }
                    commentString += "```";
                    prBuilder.WithDescription(commentString)
                    
                    ;


                return prBuilder;
                //return commentString; 
            }
            private static EntityCollection GetGuildBankRecords(string itemSearch, IOrganizationService service)
            {
                QueryExpression query = new QueryExpression("wowc_guildbankrecord");
                query.ColumnSet.AddColumns("wowc_name", "wowc_inventory", "wowc_highneed");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("wowc_name", ConditionOperator.Like, "%"+itemSearch+"%");

                EntityCollection results = service.RetrieveMultiple(query);
                return results;

            }
            private static EntityCollection GetGuildBankHighNeedRecords(IOrganizationService service)
            {
                QueryExpression query = new QueryExpression("wowc_guildbankrecord");
                query.ColumnSet.AddColumns("wowc_name", "wowc_inventory", "wowc_highneed");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("wowc_highneed", ConditionOperator.Equal, true);

                EntityCollection results = service.RetrieveMultiple(query);
                return results;

            }
        }

        

    }
}
