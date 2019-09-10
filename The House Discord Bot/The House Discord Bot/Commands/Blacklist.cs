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
        [Group("-bl"), Summary("Blacklist commands")]
        public class BlackListGroup : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }

            [Command("-n"), Summary("Report a blacklist event.")]
            public async Task BlacklistUser(string blacklistUser, [Remainder]string issueDescriptionText)
            {
                #region Validate that the User has permissions
                bool approved = false;
                foreach (SocketRole role in ((SocketGuildUser)Context.Message.Author).Roles)
                {
                    if (role.Id == 584755014648725524 || role.Id == 584754688423886858)
                    {
                        approved = true;
                    }
                }
                if (!approved)
                {
                    await Context.Channel.SendMessageAsync("Sorry but you do not have permissions to use this command.");
                    return;
                }
                #endregion

                if(Context.Message.Attachments.Count != 1)
                {
                    await ReplyAsync("You must provide proof in order to use this function.  When adding the attachment you will be prompted to add a comment, enter the command here.");
                    return;
                }
                string actionTakenText = "";
                string attachmentUrl = Context.Message.Attachments.ElementAt(0).Url;

                SocketUser author = Context.Message.Author;
                string guildNickname = Context.Guild.GetUser(author.Id).Nickname;
                string userNickname = author.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                EntityCollection crmBlacklistUser = SearchBlacklistUser(blacklistUser, crmService);

                if (crmBlacklistUser.Entities.Count == 0)
                {
                    Guid createdBlackListUser = CreateBlacklistUser(blacklistUser, crmService);
                    CreateBlacklistRecord(createdBlackListUser, userName, issueDescriptionText, attachmentUrl, crmService);
                    actionTakenText = "Created a new record for " + blacklistUser + " and added a new issue entry";
                }
                else if (crmBlacklistUser.Entities.Count == 1)
                {
                    CreateBlacklistRecord(crmBlacklistUser.Entities[0].GetAttributeValue<Guid>("wowc_blacklistid"), userName, issueDescriptionText, attachmentUrl, crmService);
                    actionTakenText = "Added a new issue entry for " + blacklistUser;
                }
                else
                {
                    actionTakenText = "We found multiple matches for this person in CRM and did not do anything. " + Context.Guild.Owner.Mention;
                }

                await ReplyAsync(actionTakenText, false, null);
            }
            [Command("-s"), Summary("Searches to see if someone was blacklisted.")]
            public async Task SearchBlacklistedUsers(string blacklistUser)
            {
                #region Validate that the User has permissions
                bool approved = false;
                foreach (SocketRole role in ((SocketGuildUser)Context.Message.Author).Roles)
                {
                    if (role.Id == 584755014648725524 || role.Id == 584754688423886858)
                    {
                        approved = true;
                    }
                }
                if (!approved)
                {
                    await Context.Channel.SendMessageAsync("Sorry but you do not have permissions to use this command.");
                    return;
                }
                #endregion

                EntityCollection crmBlacklistUser = SearchBlacklistUser(blacklistUser, crmService);

                if (crmBlacklistUser.Entities.Count == 0)
                {
                    await ReplyAsync("It looks like " + blacklistUser + " does not have any blacklisted records in CRM.", false, null);
                }
                else if(crmBlacklistUser.Entities.Count == 1)
                {
                    await ReplyAsync(null, false, BuildBlackListEmbed(blacklistUser, RetrieveBlacklistNotes(crmBlacklistUser.Entities[0].GetAttributeValue<Guid>("wowc_blacklistid"), crmService)).Build());
                }
                else
                {
                    await ReplyAsync("Duplicate blacklist users were found in CRM for "+ blacklistUser +"." + Context.Guild.Owner.Mention, false, null);
                }
            }

        }

        private static EmbedBuilder BuildBlackListEmbed(string blacklistUser, EntityCollection blacklistNotes)
        {
            EmbedBuilder embed = new EmbedBuilder();

            embed.WithTitle("Blacklist results for " + blacklistUser);

            int reportCount = blacklistNotes.Entities.Count > 25 ? 25 : blacklistNotes.Entities.Count;


            for (int i = 0; i < reportCount; i++)
            {
                string reportedBy = blacklistNotes.Entities[i].GetAttributeValue<string>("subject");
                string issueText = blacklistNotes.Entities[i].GetAttributeValue<string>("notetext").Length > 1024 ? blacklistNotes.Entities[i].GetAttributeValue<string>("notetext").Substring(0,1024) : blacklistNotes.Entities[i].GetAttributeValue<string>("notetext");
                DateTime dateReported = blacklistNotes.Entities[i].GetAttributeValue<DateTime>("createdon");
                embed.AddField(reportedBy + " on " + dateReported.ToShortDateString(),  issueText, false);
            
            }
            
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
        private static void CreateBlacklistRecord(Guid blacklistGuid,string createdBy, string issueText, string attachmentUrl, IOrganizationService service)
        {
            string noteText = attachmentUrl == "" ? issueText: attachmentUrl + "\n " + issueText;
            Entity blacklistEntry = new Entity("annotation");
            Guid blacklistEntryGuid = Guid.NewGuid();

            blacklistEntry.Id = blacklistEntryGuid;
            blacklistEntry.Attributes["subject"] = createdBy;
            blacklistEntry.Attributes["notetext"] = noteText;
            blacklistEntry.Attributes["objectid"] = new EntityReference("wowc_blacklist", blacklistGuid);

            service.Create(blacklistEntry);
        }
    }
}
