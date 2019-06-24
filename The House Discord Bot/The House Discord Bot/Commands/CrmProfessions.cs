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
    public class CrmProfessions : ModuleBase<SocketCommandContext>
    {

        [Group("-prof"), Summary("User recipe database")]
        public class UserRecipes : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }

            [Command("-a"), Summary("Add's a recipe to the current user.")]
            public async Task AddRecipe([Remainder] string itemSearch)
            {
                var author = Context.Message.Author;
                string guildNickname = Context.Guild.GetUser(author.Id).Nickname;
                string userNickname = author.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                await ReplyAsync(AssociateRecords(crmService, GetUserInformation(userName, crmService), GetItemInformation(itemSearch, crmService), userName, Context.Guild.Owner), false, null);
            }

            [Command("-r"), Summary("Removes a recipe to the current user.")]
            public async Task RemoveRecipe([Remainder] string itemSearch)
            {
                var author = Context.Message.Author;
                string guildNickname = Context.Guild.GetUser(author.Id).Nickname;
                string userNickname = author.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                await ReplyAsync(DisassociateRecords(crmService, GetUserInformation(userName, crmService), GetItemInformation(itemSearch, crmService), userName, Context.Guild.Owner), false, null);
            }
            [Command("-s"), Summary("Searches users that know this recipe.")]
            public async Task SearchRecipe([Remainder] string itemSearch)
            {
                //RetrieveUsersWithRecipe(crmService, itemSearch);
                await ReplyAsync(null, false, RecipeSearchEmbedBuilder(crmService, itemSearch, Context.Guild.Owner).Build());
            }
            [Command("-s"), Summary("Returns recipes that this user knows.")]
            public async Task SearchRecipe(IUser providedUser)
            {
                string guildNickname = Context.Guild.GetUser(providedUser.Id).Nickname;
                string userNickname = providedUser.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                await ReplyAsync(null, false, UserRecipeResults(crmService, GetUserInformation(userName, crmService), userName, Context.Guild.Owner).Build());
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
                    return results = "Created recipe relationship between **" + contact.Entities[0].GetAttributeValue<string>("lastname") + "** and **" + wowc_loot.Entities[0].GetAttributeValue<string>("wowc_name") + "**";
                }
                else
                    return results = "Hmm, this shouldn't have happened...";
            }

            private static string DisassociateRecords(IOrganizationService crmService, EntityCollection contact, EntityCollection wowc_loot, string mentionedUser, IUser guildOwner)
            {
                string results = "";
                if (contact.Entities.Count < 1)
                    return results = "Sorry, I could not find you in CRM, please make sure that your server nickname matches your WoW character name and try again. " + guildOwner.Mention + ", can you make sure " + mentionedUser + " exists?";
                else if (contact.Entities.Count > 1)
                    return results = "There seems to be more than one of you in CRM... " + guildOwner.Mention + " could you look into this?";
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

            private static EmbedBuilder RecipeSearchEmbedBuilder(IOrganizationService crmService, string itemSearch, IUser guildOwner)
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
                    return prBuilder.WithDescription("I could not find " + mentionedUser + " in CRM... Hey" + guildOwner.Mention + " could you make sure " + mentionedUser + " exists in CRM");
                else if (contact.Entities.Count > 1)
                    return prBuilder.WithDescription(guildOwner.Mention + ", there seems to be more than 1 " + contact.Entities[0].GetAttributeValue<string>("lastname") + " in CRM...");

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
                else if (results.Entities.Count >= 1)
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
