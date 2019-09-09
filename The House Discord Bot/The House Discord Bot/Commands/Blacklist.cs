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
    public class Blacklist
    {   
        [Group("-bl")]
        public class BlackListGroup : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }

            [Command(""), Summary("Black list commands")]
            public async Task BlacklistUser(string blacklistUser, [Remainder]string issueDescriptionText)
            {
                string actionTakenText = "";

                SocketUser author = Context.Message.Author;
                string guildNickname = Context.Guild.GetUser(author.Id).Nickname;
                string userNickname = author.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                EntityCollection crmBlacklistUser = SearchBlacklistUser(blacklistUser, crmService);

                if (crmBlacklistUser.Entities.Count == 0)
                {
                    Guid createdBlackListUser = CreateBlacklistUser(blacklistUser, crmService);
                    CreateBlacklistRecord(createdBlackListUser, userName, issueDescriptionText, crmService);
                    actionTakenText = "Created a new record for " + blacklistUser + " and added a new issue entry";
                }
                else if (crmBlacklistUser.Entities.Count == 1)
                {
                    CreateBlacklistRecord(crmBlacklistUser.Entities[0].GetAttributeValue<Guid>("wowc_blacklistid"), userName, issueDescriptionText, crmService);
                    actionTakenText = "Added a new issue entry for " + blacklistUser;
                }
                else
                {
                    actionTakenText = "We found multiple matches for this person in CRM and did not do anything. " + Context.Guild.Owner.Mention;
                }

                await ReplyAsync(actionTakenText, false, null);
            }
            [Command("-s"), Summary("Black list commands")]
            public async Task SearchBlacklistedUsers(string blacklistUser)
            {
                EntityCollection crmBlacklistUser = SearchBlacklistUser(blacklistUser, crmService);

                if (crmBlacklistUser.Entities.Count == 0)
                {
                    await ReplyAsync("It looks like " + blacklistUser + " does not have any blacklisted records in CRM.", false, null);
                }
                else if(crmBlacklistUser.Entities.Count == 1)
                {
                    EntityCollection blacklistEntryResults = RetrieveBlacklistNotes(crmBlacklistUser.Entities[0].GetAttributeValue<Guid>("wowc_blacklistid"), crmService);

                    /*
                     * Need to finish building this out
                     */
                }
                else
                {
                    await ReplyAsync("Duplicate blacklist users were found in CRM. " + Context.Guild.Owner.Mention, false, null);
                }
            }

        }



        private EmbedBuilder BuildBlackListEmbed(EntityCollection blacklistUser, EntityCollection blacklistNotes)
        {
            EmbedBuilder embed = new EmbedBuilder();

            /*
             * Need to finish building this out
             */
            return embed;
        }

        private static EntityCollection SearchBlacklistUser(string blacklistUser, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("wowc_blacklist");
            query.ColumnSet.AddColumns("wowc_name");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_name", ConditionOperator.Equal, blacklistUser);
            
            EntityCollection results = service.RetrieveMultiple(query);
            return results;
        }
        private static EntityCollection RetrieveBlacklistNotes(Guid blacklistUser, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("annotation");
            query.ColumnSet.AddColumns("subject", "notetext", "createdon");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("objectid", ConditionOperator.Equal, blacklistUser);
            query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));

            EntityCollection results = service.RetrieveMultiple(query);
            return results;
        }
        private static Guid CreateBlacklistUser(string blacklistUser, IOrganizationService service)
        {
            Entity blacklist = new Entity("wowc_blacklist");
            Guid blacklistGuid = Guid.NewGuid();

            blacklist.Id = blacklistGuid;
            blacklist.Attributes["wowc_name"] = blacklistUser;

            service.Create(blacklist);

            return blacklistGuid;
        }
        private static void CreateBlacklistRecord(Guid blacklistGuid,string createdBy, string issueText, IOrganizationService service)
        {
            Entity blacklistEntry = new Entity("annotation");
            Guid blacklistEntryGuid = Guid.NewGuid();

            blacklistEntry.Id = blacklistEntryGuid;
            blacklistEntry.Attributes["subject"] = "Reported by: " + createdBy;
            blacklistEntry.Attributes["notetext"] = issueText;
            blacklistEntry.Attributes["objectid"] = new EntityReference("wowc_blacklist", blacklistGuid);

            service.Create(blacklistEntry);
        }
    }
}
