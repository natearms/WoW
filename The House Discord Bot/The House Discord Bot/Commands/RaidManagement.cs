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
    public class RaidManagement : ModuleBase<SocketCommandContext>
    {
        [Group ("-raid"), Summary("Raid management commands.")]
        public class RaidModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }
            [Command("-active"), Summary("Build active raid group.")]
            public async Task BuildActiveRaidGroup([Remainder] string activeString)
            {
                bool approved = false;
                foreach (SocketRole role in ((SocketGuildUser)Context.Message.Author).Roles)
                {
                    if (role.Id == 584754688423886858)
                    {
                        approved = true;
                    }
                }
                if (!approved)
                {
                    await Context.Channel.SendMessageAsync("Sorry but you do not have permissions to use this command.");
                    return;
                }

                string[] raidMembers = Array.ConvertAll(activeString.Split(','), p => p.Trim());

                for (int i = 0; i < raidMembers.Length; i++)
                {
                    EntityCollection userInfo = GetUserInformation(raidMembers[i], crmService);
                    if (userInfo.Entities.Count == 0)
                    {
                        await ReplyAsync("I could not find a Guild Member record for **" + raidMembers[i] +"**", false, null);
                    }
                    else if(userInfo.Entities.Count == 1)
                    {
                        Entity contact = new Entity("contact");
                        contact.Id = userInfo.Entities[0].Id;
                        contact["parentcustomerid"] = new EntityReference("account", new Guid("BA455092-778A-E911-A81A-000D3A3B53C4"));

                        crmService.Update(contact);
                    }
                    else if(userInfo.Entities.Count > 1)
                    {
                        await ReplyAsync("Duplicate guild member records were found for **" + raidMembers[i] + "*", false, null);
                    }
                }

                await ReplyAsync("Active Raid Team count = " + GetTotalMembers(new Guid("BA455092-778A-E911-A81A-000D3A3B53C4"), crmService).Entities.Count, false, null);


            }
            [Command("-standby"), Summary("Build standby raid group.")]
            public async Task BuildStandbyRaidGroup([Remainder] string standbyString)
            {
                bool approved = false;
                foreach (SocketRole role in ((SocketGuildUser)Context.Message.Author).Roles)
                {
                    if (role.Id == 584754688423886858)
                    {
                        approved = true;
                    }
                }
                if (!approved)
                {
                    await Context.Channel.SendMessageAsync("Sorry but you do not have permissions to use this command.");
                    return;
                }

                string[] raidMembers = Array.ConvertAll(standbyString.Split(','), p => p.Trim());

                for (int i = 0; i < raidMembers.Length; i++)
                {
                    EntityCollection userInfo = GetUserInformation(raidMembers[i], crmService);
                    if (userInfo.Entities.Count == 0)
                    {
                        await ReplyAsync("I could not find a Guild Member record for **" + raidMembers[i] + "**", false, null);
                    }
                    else if (userInfo.Entities.Count == 1)
                    {
                        Entity contact = new Entity("contact");
                        contact.Id = userInfo.Entities[0].Id;
                        contact["parentcustomerid"] = new EntityReference("account", new Guid("9AEC1299-778A-E911-A81A-000D3A3B53C4"));

                        crmService.Update(contact);
                    }
                    else if (userInfo.Entities.Count > 1)
                    {
                        await ReplyAsync("Duplicate guild member records were found for **" + raidMembers[i] + "*", false, null);
                    }
                }

                await ReplyAsync("Standby Raid Team count = " + GetTotalMembers(new Guid("9AEC1299-778A-E911-A81A-000D3A3B53C4"), crmService).Entities.Count, false, null);
            }
            [Command("-remove"), Summary("Remove member from group.")]
            public async Task RemoveFromRaidGroup([Remainder] string removalString)
            {
                bool approved = false;
                foreach (SocketRole role in ((SocketGuildUser)Context.Message.Author).Roles)
                {
                    if (role.Id == 584754688423886858)
                    {
                        approved = true;
                    }
                }
                if (!approved)
                {
                    await Context.Channel.SendMessageAsync("Sorry but you do not have permissions to use this command.");
                    return;
                }

                string[] raidMembers = Array.ConvertAll(removalString.Split(','), p => p.Trim());

                for (int i = 0; i < raidMembers.Length; i++)
                {
                    EntityCollection userInfo = GetUserInformation(raidMembers[i], crmService);
                    if (userInfo.Entities.Count == 0)
                    {
                        await ReplyAsync("I could not find a Guild Member record for **" + raidMembers[i] + "**", false, null);
                    }
                    else if (userInfo.Entities.Count == 1)
                    {
                        Entity contact = new Entity("contact");
                        contact.Id = userInfo.Entities[0].Id;
                        contact["parentcustomerid"] = null;

                        crmService.Update(contact);
                        await ReplyAsync(userInfo.Entities[0].GetAttributeValue<string>("lastname") + " is no longer associated to a group.");
                    }
                    else if (userInfo.Entities.Count > 1)
                    {
                        await ReplyAsync("Duplicate guild member records were found for **" + raidMembers[i] + "*", false, null);
                    }
                }
            }
            [Command("-reset"), Summary("Remove member from group.")]
            public async Task ResetRaidGroup()
            {
                bool approved = false;
                foreach (SocketRole role in ((SocketGuildUser)Context.Message.Author).Roles)
                {
                    if (role.Id == 584754688423886858)
                    {
                        approved = true;
                    }
                }
                if (!approved)
                {
                    await Context.Channel.SendMessageAsync("Sorry but you do not have permissions to use this command.");
                    return;
                }
                EntityCollection raidMembersWithRaidGroup = GetMembersWithRaidGroup(crmService);
                for (int i = 0; i < raidMembersWithRaidGroup.Entities.Count; i++)
                {
                    Entity contact = new Entity("contact");
                    contact.Id = raidMembersWithRaidGroup.Entities[i].Id;
                    contact["parentcustomerid"] = null;

                    crmService.Update(contact);
                }
                
                await Context.Channel.SendMessageAsync("Members with a raid group = " + GetMembersWithRaidGroup(crmService).Entities.Count);

            }
            private static EntityCollection GetUserInformation(string userName, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("lastname", "contactid", "parentcustomerid");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("lastname", ConditionOperator.Equal, userName);
                query.Orders.Add(new OrderExpression("lastname", OrderType.Ascending));

                EntityCollection results = crmService.RetrieveMultiple(query);
                return results;
            }
            private static EntityCollection GetTotalMembers(Guid team, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("lastname", "contactid", "parentcustomerid");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, team);
                
                EntityCollection results = crmService.RetrieveMultiple(query);
                return results;
            }
            private static EntityCollection GetMembersWithRaidGroup(IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("contact");
                query.ColumnSet.AddColumns("lastname", "contactid", "parentcustomerid");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("parentcustomerid", ConditionOperator.NotNull);

                EntityCollection results = crmService.RetrieveMultiple(query);
                return results;
            }
        }
    }
}
