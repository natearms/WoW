using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using The_House_Discord_Bot.Utilities;
namespace The_House_Discord_Bot.Commands
{
    public class CrmGuildBank: ModuleBase<SocketCommandContext>
    {

        [Group("-gb"), Summary("Guild Bank record commands")]
        public class GuildBankModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }
            
            [Command("-hn"), Summary("Searches the guild bank for high need mats.")]
            public async Task GuildBankHighNeed()
            {
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;

                EntityCollection fetchResults = crmService.RetrieveMultiple(
                    new FetchExpression($@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='wowc_guildbankrecord'>
                                    <attribute name='wowc_name' />
                                    <attribute name='wowc_inventory' />
                                    <order attribute='wowc_name' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='wowc_highneed' operator='eq' value='1' />
                                    </filter>
                                  </entity>
                                </fetch>")
                    );

                if (fetchResults.Entities.Count == 0)
                {
                    await ReplyAsync("There doesn't seem to be any guild bank records in high need right now, please check back later.", false, null);
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder()
                        .WithDescription(ResultsFormatter.FormatResultsIntoTable(fetchResults, triggeredBy, new string[] { "Item Name", "Stock" }, new string[] { "wowc_name", "wowc_inventory" }));

                    await ReplyAsync("Below is a list of high need items for the guild bank.", false, embed.Build());
                }
            }

            [Command("-s"), Summary("Searches the guild bank with string criteria.")]
            public async Task GuildBankSearch([Remainder] string itemSearch)
            {
                var triggeredBy = Context.Guild.GetUser(Context.Message.Author.Id).Nickname != null ? Context.Guild.GetUser(Context.Message.Author.Id).Nickname : Context.Message.Author.Username;

                EntityCollection fetchResults = crmService.RetrieveMultiple(
                    new FetchExpression($@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                          <entity name='wowc_guildbankrecord'>
                                            <attribute name='wowc_name' />
                                            <attribute name='wowc_inventory' />
                                            <order attribute='wowc_name' descending='false' />
                                            <filter type='and'>
                                              <condition attribute='wowc_name' operator='like' value='%{itemSearch}%' />
                                              <condition attribute='wowc_inventory' operator='gt' value='0' />
                                            </filter>
                                          </entity>
                                        </fetch>")
                    );

                if (fetchResults.Entities.Count == 0)
                {
                    await ReplyAsync("I could not find a guild bank record matching criteria **" + itemSearch + "** in the guild bank.  Please make sure you spelled the item name or part of the item name correctly and try again.", false, null);
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder()
                        .WithDescription(ResultsFormatter.FormatResultsIntoTable(fetchResults, triggeredBy, new string[] { "Item Name", "Stock" }, new string[] { "wowc_name", "wowc_inventory" }));
                    
                    await ReplyAsync("Here is what I found with your search criteria **\"" + itemSearch + "\"**", false, embed.Build());
                }
            }
            [Command("-a"), Summary("Returns audit history for an item.")]
            public async Task GuildBankAuditHistory([Remainder] string itemSearch)
            {
               await ReplyAsync(null, false, GuildBankAuditEmbedBuilder(itemSearch, crmService).Build());
            }
            private EmbedBuilder GuildBankAuditEmbedBuilder(string itemSearch, IOrganizationService service)
            {
                EmbedBuilder prBuilder = new EmbedBuilder();
                EntityCollection guildBankRecord = GetGuildBankRecords(itemSearch, service);
                if (guildBankRecord.Entities.Count > 1)
                    return prBuilder.WithDescription("Your search returned too many guild bank records, please be more specific with your search.");
                if (guildBankRecord.Entities.Count < 1)
                    return prBuilder.WithDescription("I was not able to find any guild bank records with this search criteria.");

                EntityCollection auditHistory = GetGuildBankAuditHistory(guildBankRecord, service);

                string commentString = "```" + "Raid Member".PadRight(15) + "Delta".PadLeft(8) + "Inv.".PadLeft(8);
                int inventoryValue = guildBankRecord.Entities[0].GetAttributeValue<int>("wowc_inventory");
                commentString += "\n" + "Current".PadRight(15, '.') + inventoryValue.ToString().PadLeft(16, '.');
                for (int i = 0; i < auditHistory.Entities.Count; i++)
                {
                    string member = auditHistory.Entities[i].GetAttributeValue<EntityReference>("wowc_raidmember").Name;
                    int inventory = (Int32)auditHistory.Entities[i].GetAttributeValue<Decimal>("wowc_epcount");
                    inventoryValue = inventoryValue - inventory;

                    member = member.Length > 15 ? member.Substring(0, 15) : member;

                    commentString += "\n" + member.PadRight(15, '.') + inventory.ToString().PadLeft(6, '.') + inventoryValue.ToString().PadLeft(10, '.');

                }
                commentString += "```";
                prBuilder.WithDescription(commentString)
                    .WithTitle("Here is the audit history for **" + guildBankRecord.Entities[0].GetAttributeValue<string>("wowc_name") + "**");
                ;

                return prBuilder;
            }
            private static EntityCollection GetGuildBankRecords(string itemSearch, IOrganizationService service)
            {
                QueryExpression query = new QueryExpression("wowc_guildbankrecord");
                query.ColumnSet.AddColumns("wowc_name", "wowc_inventory", "wowc_highneed");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("wowc_name", ConditionOperator.Like, "%" + itemSearch + "%");
                query.Criteria.AddCondition("wowc_inventory", ConditionOperator.GreaterEqual, 1);
                query.Orders.Add(new OrderExpression("wowc_name", OrderType.Ascending));

                EntityCollection results = service.RetrieveMultiple(query);
                return results;

            }
            private static EntityCollection GetGuildBankAuditHistory(EntityCollection itemResult, IOrganizationService service)
            {
                QueryExpression query = new QueryExpression("wowc_effortpoint");
                query.ColumnSet.AddColumns("wowc_raidmember", "wowc_epcount");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("wowc_guildbankrecord", ConditionOperator.Equal, itemResult.Entities[0].GetAttributeValue<Guid>("wowc_guildbankrecordid"));
                query.Orders.Add(new OrderExpression("createdon", OrderType.Descending));

                EntityCollection results = service.RetrieveMultiple(query);

                return results;
            }
        }

    }
}
