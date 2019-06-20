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
    public class CrmRequests : ModuleBase<SocketCommandContext> 
    {
        [Group("-dkp"), Summary("Users DKP breakdown by PR/EP/GP")]
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
                string guildNickname = Context.Guild.GetUser(providedUser.Id).Nickname;
                string userNickname = providedUser.Username;

                string userName = guildNickname == null ? userNickname : guildNickname;
                var userNameMention = providedUser.Mention;

                EntityCollection userInfo = GetUserEpGp(userName, crmService);
                EmbedBuilder prBuilder = new EmbedBuilder();
                if (userInfo.Entities.Count == 0)
                {
                    prBuilder.WithDescription("I could not find a DKP record for " + userNameMention + " in <The House> CRM.  Please contact " +
                        Context.Guild.Owner.Mention + " to create a record for this user.");
                }
                else
                {
                    prBuilder.WithDescription("**Here is the DKP breakdown for** " + userNameMention +
                    "\n\n**PR: **" + userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalpr").ToString("0.##") +
                    "\n**EP: **" + userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalep").ToString("0.##") +
                    "\n**GP: **" + userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalgp").ToString("0.##") + "\n"
                    )
                    ;
                }

                return prBuilder.Build();
            }
        }
        
        [Group("-gb"), Summary("Guild Bank record commands")]
        public class GuildBankModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }

            [Command("-highneed"), Summary("Searches the guild bank for high need mats.")]
            public async Task guildBank()
            {
                await ReplyAsync(null, false, BuildGuildBankHighNeedList(crmService));
            }

            [Command("-search"), Summary("Searches the guild bank with string criteria.")]
            public async Task guildBank([Remainder] string itemSearch)
            {
                await ReplyAsync(null, false, BuildGuildBankList(itemSearch, crmService));
            }

            private Embed BuildGuildBankList(string itemSearch, IOrganizationService crmService)
            {
                EntityCollection guildBankRecord = GetGuildBankRecords(itemSearch, crmService);
                EmbedBuilder prBuilder = GuildBankEmbedBuilder(guildBankRecord);

                if (guildBankRecord.Entities.Count == 0)
                {
                    prBuilder.WithDescription("I could not find a guild bank record matching criteria " + itemSearch +
                        " in the guild bank.  Please make sure you spelled the item name or part of the item name correctly and try again.");
                    return prBuilder.Build();
                }
                prBuilder.WithTitle("Here is what I found with your search criteria **\""+itemSearch+"\"**");
                return prBuilder.Build();
                
            }
            private Embed BuildGuildBankHighNeedList(IOrganizationService crmService)
            {
                EntityCollection guildBankRecord = GetGuildBankHighNeedRecords(crmService);

                EmbedBuilder prBuilder = GuildBankEmbedBuilder(guildBankRecord);

                if (guildBankRecord.Entities.Count == 0)
                {
                    prBuilder.WithDescription("There doesn't seem to be any guild bank records in high need right now, please check back later.");
                    return prBuilder.Build();
                }
                prBuilder.WithTitle("Below is a list of high need items for the guild bank.");
                return prBuilder.Build();

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
            private static EntityCollection GetGuildBankRecords(string itemSearch, IOrganizationService service)
            {
                QueryExpression query = new QueryExpression("wowc_guildbankrecord");
                query.ColumnSet.AddColumns("wowc_name", "wowc_inventory", "wowc_highneed");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("wowc_name", ConditionOperator.Like, "%"+itemSearch+"%");
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
                query.Orders.Add(new OrderExpression("wowc_name",OrderType.Ascending));

                EntityCollection results = service.RetrieveMultiple(query);
                return results;

            }
        }

        [Group("-recipe"), Summary("User recipe database")]
        public class UserRecipes : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }

            [Command("-add"), Summary("Add's a recipe to the current user.")]
            public async Task addRecipe([Remainder] string itemSearch)
            {
                var author = Context.Message.Author;
                string guildNickname = Context.Guild.GetUser(author.Id).Nickname;
                string userNickname = author.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                AssociateRecords(crmService, GetUserInformation(userName, crmService),GetItemInformation(itemSearch, crmService));
                await ReplyAsync("test", false, null);
            }

            [Command("-remove"), Summary("Removes a recipe to the current user.")]
            public async Task removeRecipe([Remainder] string itemSearch)
            {
                var author = Context.Message.Author;
                string guildNickname = Context.Guild.GetUser(author.Id).Nickname;
                string userNickname = author.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                DisassociateRecords(crmService, GetUserInformation(userName, crmService),GetItemInformation(itemSearch, crmService));
                await ReplyAsync("test", false, null);
            }

            
            [Command("-search"), Summary("Searches users that know this recipe.")]
            public async Task searchRecipe([Remainder] string itemSearch)
            {
                RetrieveUsersWithRecipe(crmService, itemSearch);
                await ReplyAsync("test", false, null);
            }
            
            private static EntityCollection GetItemInformation(string itemSearch, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("wowc_loot");
                query.ColumnSet.AddColumns("wowc_lootid");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("wowc_name", ConditionOperator.Like, "%" + itemSearch + "%");
                query.Criteria.AddCondition("wowc_slot", ConditionOperator.Equal, 257260010);

                EntityCollection results = crmService.RetrieveMultiple(query);
                return results;
            }

            private static EntityCollection GetUserInformation(string userName, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("lastname", "wowc_primaryprofession", "wowc_secondaryprofession");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("lastname", ConditionOperator.Like, "%" + userName + "%");
                query.Orders.Add(new OrderExpression("lastname", OrderType.Ascending));

                EntityCollection results = crmService.RetrieveMultiple(query);
                return results;
            }

            private static void AssociateRecords(IOrganizationService crmService, EntityCollection contact, EntityCollection wowc_loot)
            {
                var contactRef = new EntityReference("contact", contact.Entities[0].GetAttributeValue<Guid>("contactid"));
                var wowc_lootRef = new EntityReference("wowc_loot", wowc_loot.Entities[0].GetAttributeValue<Guid>("wowc_lootid"));
                var wowc_lootCollection = new EntityReferenceCollection();
                wowc_lootCollection.Add(wowc_lootRef);


                var relationship = new Relationship("wowc_contact_wowc_loot");

                crmService.Associate(contactRef.LogicalName, contactRef.Id, relationship, wowc_lootCollection);
            }

            private static void DisassociateRecords(IOrganizationService crmService, EntityCollection contact, EntityCollection wowc_loot)
            {
                var contactRef = new EntityReference("contact", contact.Entities[0].GetAttributeValue<Guid>("contactid"));
                var wowc_lootRef = new EntityReference("wowc_loot", wowc_loot.Entities[0].GetAttributeValue<Guid>("wowc_lootid"));
                var wowc_lootCollection = new EntityReferenceCollection();
                wowc_lootCollection.Add(wowc_lootRef);

                var relationship = new Relationship("wowc_contact_wowc_loot");

                crmService.Disassociate(contactRef.LogicalName, contactRef.Id, relationship, wowc_lootCollection);
            }

            private static EntityCollection RetrieveUsersWithRecipe(IOrganizationService crmService, string itemSearch)
            {
                string entity1 = "wowc_loot";
                string entity2 = "contact";
                string relationshipEntityName = "wowc_contact_wowc_loot";

                QueryExpression query = new QueryExpression(entity1);
                query.ColumnSet = new ColumnSet("wowc_name","wowc_lootid","wowc_slot");

                LinkEntity linkEntity1 = new LinkEntity(entity1, relationshipEntityName, "wowc_lootid", "wowc_lootid", JoinOperator.Inner);
                LinkEntity linkEntity2 = new LinkEntity(relationshipEntityName, entity2, "contactid", "contactid", JoinOperator.Inner);
                linkEntity2.Columns.AddColumns("lastname");
                linkEntity1.LinkEntities.Add(linkEntity2);
                query.LinkEntities.Add(linkEntity1);
                query.Criteria.AddCondition("wowc_name", ConditionOperator.Like, "%" + itemSearch + "%");
                query.Criteria.AddCondition("wowc_slot", ConditionOperator.Equal, 257260010);
                 
                EntityCollection results = crmService.RetrieveMultiple(query);
                return results;
            }
        }
    }
}
