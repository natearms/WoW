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
                List<SocketUser> list = new List<SocketUser>();
                list.Add(Context.Message.Author);
                list.AsReadOnly();

                IReadOnlyCollection<SocketUser> queriedUser = list;
                await ReplyAsync(null, false, BuildUsersDKP(queriedUser).Build());
            }
            
            
            [Command("")]
            public async Task PrEpGpMention(IUser mentionedUser)
            {
                IReadOnlyCollection<SocketUser> mentionedUsers = Context.Message.MentionedUsers;
                await ReplyAsync(null, false, BuildUsersDKP(mentionedUsers).Build());
            }

            [Command("")]
            public async Task PrEpGpMentions([Remainder]string test)
            {
                IReadOnlyCollection<SocketUser> mentionedUsers = Context.Message.MentionedUsers;

                if (mentionedUsers.Count == 0)
                    await ReplyAsync("You must provide a mention in order to use the multi mention feature");
                else if (mentionedUsers.Count>10)
                {
                    await ReplyAsync("You've requested too many users, please limit results to 10 at a time.");
                }
                else
                {
                    await ReplyAsync(null, false, BuildUsersDKP(mentionedUsers).Build());
                }
            }
            /*
            [Command("-top")]
            public async Task PrEpGpMentions(int returnRange)
            {
                EntityCollection topUsers = GetTopUserEpGp(returnRange, crmService);
                

                await ReplyAsync(null, false, BuildUsersDKP(mentionedUsers).Build());
                
            }
            */
            private EntityCollection GetTopUserEpGp(int returnCount, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("wowc_totalpr", "wowc_totalep", "wowc_totalgp");
                query.Criteria = new FilterExpression();
                //query.Criteria.AddCondition("lastname", ConditionOperator.Equal, userName);
                query.Orders.Add(new OrderExpression("wowc_totalpr", OrderType.Descending));
                query.PageInfo.Count = returnCount;

                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
            }
            private EntityCollection GetUserEpGp(string userName, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("wowc_totalpr", "wowc_totalep", "wowc_totalgp");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("lastname", ConditionOperator.Equal, userName);
                query.Orders.Add(new OrderExpression("wowc_totalpr", OrderType.Descending));

                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
            }
            private EmbedBuilder BuildUsersDKP(IReadOnlyCollection<SocketUser> providedUser)
            {
                
                EmbedBuilder prBuilder = new EmbedBuilder();
                
                string commentString = "```" + "Name".PadRight(15) + "Total PR".PadLeft(12) + "Total EP".PadLeft(12) + "Total GP".PadLeft(12);
                string mentionGuildOwner = "";
                for (int i = 0; i < providedUser.Count; i++)
                {
                    
                    string guildNickname = Context.Guild.GetUser(providedUser.ElementAt(i).Id).Nickname;
                    string userNickname = providedUser.ElementAt(i).Username;

                    string userName = guildNickname == null ? userNickname : guildNickname;
                    EntityCollection userInfo = GetUserEpGp(userName, crmService);

                    if (userInfo.Entities.Count == 0)
                    {
                        commentString += ("\n"+ userName.PadRight(15,'.') + "doesn't seem to be in CRM.....".PadLeft(36,'.'));
                        mentionGuildOwner = Context.Guild.Owner.Mention +", one or more users are not in CRM."; 
                    }
                    else
                    {
                        string totalPr = userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalpr").ToString("0.##");
                        string totalEp = userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalep").ToString("0.##");
                        string TotalGp = userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalgp").ToString("0.##");

                        commentString += "\n" + userName.PadRight(15, '.') + totalPr.ToString().PadLeft(12, '.') + totalEp.ToString().PadLeft(12, '.') + TotalGp.ToString().PadLeft(12, '.');

                    }
                }
                commentString += "```";
                prBuilder.WithDescription(mentionGuildOwner +"\n"+ commentString)
                ;
                return prBuilder;
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

                await ReplyAsync(AssociateRecords(crmService, GetUserInformation(userName, crmService), GetItemInformation(itemSearch, crmService), userName,Context.Guild.Owner), false, null);
            }

            [Command("-remove"), Summary("Removes a recipe to the current user.")]
            public async Task removeRecipe([Remainder] string itemSearch)
            {
                var author = Context.Message.Author;
                string guildNickname = Context.Guild.GetUser(author.Id).Nickname;
                string userNickname = author.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                await ReplyAsync(DisassociateRecords(crmService, GetUserInformation(userName, crmService), GetItemInformation(itemSearch, crmService), userName, Context.Guild.Owner), false, null);
            }
            [Command("-search"), Summary("Searches users that know this recipe.")]
            public async Task searchRecipe([Remainder] string itemSearch)
            {
                //RetrieveUsersWithRecipe(crmService, itemSearch);
                await ReplyAsync(null, false, RecipeSearchEmbedBuilder(crmService,itemSearch, Context.Guild.Owner).Build());
            }
            [Command("-search"), Summary("Returns recipes that this user knows.")]
            public async Task searchRecipe(IUser providedUser)
            {
                string guildNickname = Context.Guild.GetUser(providedUser.Id).Nickname;
                string userNickname = providedUser.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                await ReplyAsync(null, false, UserRecipeResults(crmService, GetUserInformation(userName,crmService), userName, Context.Guild.Owner).Build());
            }
            private static EntityCollection GetItemInformation(string itemSearch, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("wowc_loot");
                query.ColumnSet.AddColumns("wowc_lootid", "wowc_name");
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
                query.Criteria.AddCondition("lastname", ConditionOperator.Equal, userName);
                query.Orders.Add(new OrderExpression("lastname", OrderType.Ascending));

                EntityCollection results = crmService.RetrieveMultiple(query);
                return results;
            }

            private static string AssociateRecords(IOrganizationService crmService, EntityCollection contact, EntityCollection wowc_loot, string mentionedUser, IUser guildOwner)
            {
                string results = "";
                if (contact.Entities.Count < 1)
                    return results = "Sorry, I could not find you in CRM, please make sure that your server nickname matches your WoW character name and try again. " + guildOwner.Mention + ", can you make sure " + mentionedUser + " exists?";
                else if (contact.Entities.Count > 1)
                    return results = "There seems to be more than one of you in CRM... " + guildOwner.Mention + ", could you look into this?";
                else if (wowc_loot.Entities.Count < 1)
                    return results = "I could not find the item you are searching for.";
                else if (wowc_loot.Entities.Count > 1)
                    return results = "I found too many items with this search criteria, please be more specific.";

                var relationship = new Relationship("wowc_contact_wowc_loot");
                var contactRef = new EntityReference("contact", contact.Entities[0].GetAttributeValue<Guid>("contactid"));
                var wowc_lootRef = new EntityReference("wowc_loot", wowc_loot.Entities[0].GetAttributeValue<Guid>("wowc_lootid"));
                var wowc_lootCollection = new EntityReferenceCollection();
                wowc_lootCollection.Add(wowc_lootRef);
                if (wowc_loot.Entities.Count == 1 && contact.Entities.Count == 1)
                {
                    crmService.Associate(contactRef.LogicalName, contactRef.Id, relationship, wowc_lootCollection);
                    return results = "Created recipe relationship between **" + contact.Entities[0].GetAttributeValue<string>("lastname") + "** and **" + wowc_loot.Entities[0].GetAttributeValue<string>("wowc_name")+"**";
                }
                else
                    return results = "Hmm, this shouldn't have happened...";   
            }

            private static string DisassociateRecords(IOrganizationService crmService, EntityCollection contact, EntityCollection wowc_loot, string mentionedUser, IUser guildOwner)
            {
                string results = "";
                if (contact.Entities.Count < 1)
                    return results = "Sorry, I could not find you in CRM, please make sure that your server nickname matches your WoW character name and try again. " +guildOwner.Mention+", can you make sure "+mentionedUser+" exists?";
                else if (contact.Entities.Count > 1)
                    return results = "There seems to be more than one of you in CRM... " + guildOwner.Mention +" could you look into this?";
                else if (wowc_loot.Entities.Count < 1)
                    return results = "I could not find the item you are searching for.";
                else if (wowc_loot.Entities.Count > 1)
                    return results = "I found too many items with this search criteria, please be more specific.";

                var relationship = new Relationship("wowc_contact_wowc_loot");
                var contactRef = new EntityReference("contact", contact.Entities[0].GetAttributeValue<Guid>("contactid"));
                var wowc_lootRef = new EntityReference("wowc_loot", wowc_loot.Entities[0].GetAttributeValue<Guid>("wowc_lootid"));
                var wowc_lootCollection = new EntityReferenceCollection();
                wowc_lootCollection.Add(wowc_lootRef);

                if (wowc_loot.Entities.Count == 1 && contact.Entities.Count == 1)
                {
                    crmService.Disassociate(contactRef.LogicalName, contactRef.Id, relationship, wowc_lootCollection);
                    return results = "Removed recipe relationship between **" + contact.Entities[0].GetAttributeValue<string>("lastname") + "** and **" + wowc_loot.Entities[0].GetAttributeValue<string>("wowc_name") + "**";
                }
                else
                    return results = "Hmm, this shouldn't have happened...";
            }

            private EmbedBuilder RecipeSearchEmbedBuilder(IOrganizationService crmService, string itemSearch, IUser guildOwner)
            {
                EmbedBuilder prBuilder = new EmbedBuilder();

                string entity1 = "wowc_loot";
                string entity2 = "contact";
                string relationshipEntityName = "wowc_contact_wowc_loot";

                QueryExpression query = new QueryExpression(entity1);
                query.ColumnSet = new ColumnSet("wowc_name", "wowc_lootid", "wowc_slot");

                LinkEntity linkEntity1 = new LinkEntity(entity1, relationshipEntityName, "wowc_lootid", "wowc_lootid", JoinOperator.Inner);
                LinkEntity linkEntity2 = new LinkEntity(relationshipEntityName, entity2, "contactid", "contactid", JoinOperator.Inner);
                linkEntity2.Columns.AddColumns("lastname");
                linkEntity2.Orders.Add(new OrderExpression("lastname", OrderType.Ascending));
                linkEntity1.LinkEntities.Add(linkEntity2);
                query.LinkEntities.Add(linkEntity1);
                query.Criteria.AddCondition("wowc_name", ConditionOperator.Like, "%" + itemSearch + "%");
                query.Criteria.AddCondition("wowc_slot", ConditionOperator.Equal, 257260010);
                query.Orders.Add(new OrderExpression("wowc_name", OrderType.Ascending));
                
                EntityCollection results = crmService.RetrieveMultiple(query);

                if (results.Entities.Count < 1)
                    return prBuilder.WithDescription("It seems like no one has what you are looking for, or you mispelled what you were looking for.");
                else
                {
                    string commentString = "```" + "Guild Member".PadRight(13) + "Item Name".PadLeft(40);
                    for (int i = 0; i < results.Entities.Count; i++)
                    {
                        var playerName = results.Entities[i].GetAttributeValue<AliasedValue>("contact2.lastname").Value.ToString();
                        string recipe = results.Entities[i].GetAttributeValue<string>("wowc_name").ToString();
                        recipe = recipe.Substring(recipe.LastIndexOf(':') + 1).TrimStart(' ');

                        playerName = playerName.Length > 12 ? playerName.Substring(0, 12) : playerName;
                        recipe = recipe.Length > 40 ? recipe.Substring(0, 40) : recipe;

                        commentString += "\n" + playerName.PadRight(13, '.') + recipe.ToString().PadLeft(40, '.');

                    }
                    commentString += "```";
                    prBuilder.WithDescription(commentString)
                        ;
                }

                return prBuilder;
            }
            private static EmbedBuilder UserRecipeResults(IOrganizationService crmService, EntityCollection contact, string mentionedUser, IUser guildOwner)
            {
                EmbedBuilder prBuilder = new EmbedBuilder();

                if (contact.Entities.Count < 1)
                    return prBuilder.WithDescription("I could not find "+mentionedUser+" in CRM... Hey" + guildOwner.Mention+" could you make sure "+mentionedUser+" exists in CRM" );
                else if(contact.Entities.Count > 1)
                    return prBuilder.WithDescription(guildOwner.Mention+", there seems to be more than 1 " + contact.Entities[0].GetAttributeValue<string>("lastname")+" in CRM...");

                string entity1 = "wowc_loot";
                string entity2 = "contact";
                string relationshipEntityName = "wowc_contact_wowc_loot";

                QueryExpression query = new QueryExpression(entity1);
                query.ColumnSet = new ColumnSet("wowc_name", "wowc_lootid", "wowc_slot");

                LinkEntity linkEntity1 = new LinkEntity(entity1, relationshipEntityName, "wowc_lootid", "wowc_lootid", JoinOperator.Inner);
                LinkEntity linkEntity2 = new LinkEntity(relationshipEntityName, entity2, "contactid", "contactid", JoinOperator.Inner);
                linkEntity2.Columns.AddColumns("lastname");
                linkEntity2.LinkCriteria.AddCondition("lastname", ConditionOperator.Like, "%" + contact.Entities[0].GetAttributeValue<string>("lastname") + "%");
                linkEntity1.LinkEntities.Add(linkEntity2);
                query.LinkEntities.Add(linkEntity1);
                //query.Criteria.AddCondition("wowc_name", ConditionOperator.Like, "%" + itemSearch + "%");
                //query.Criteria.AddCondition("wowc_slot", ConditionOperator.Equal, 257260010);
                query.Orders.Add(new OrderExpression("wowc_name", OrderType.Ascending));

                EntityCollection results = crmService.RetrieveMultiple(query);

                if (results.Entities.Count < 1)
                    prBuilder.WithDescription("It doesn't look like " + contact.Entities[0].GetAttributeValue<string>("lastname") + " has any known recipes.");
                else if(results.Entities.Count >= 1)
                {
                    string commentString = "```";
                    for (int i = 0; i < results.Entities.Count; i++)
                    {
                        string recipe = results.Entities[i].GetAttributeValue<string>("wowc_name").ToString();
                        //recipe = recipe.Substring(recipe.LastIndexOf(':') + 1).TrimStart(' ');
                        commentString += "\n" + recipe.ToString();

                    }
                    commentString += "```";
                    prBuilder.WithDescription(commentString)
                        .WithTitle(contact.Entities[0].GetAttributeValue<string>("lastname") + " has listed the following recipes as known.");
                }
                return prBuilder;
            }
        }
    }
}
