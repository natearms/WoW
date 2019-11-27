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
    public class EPGP : ModuleBase<SocketCommandContext>
    {
        [Group("-ep"), Summary("Effort Point Query Commands")]
        public class EPModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }
            [Command("-donations")]
            public async Task ReturnEPDonations()
            {
                await ReplyAsync(null, false, epFormatEmbed(GetEPDonations(crmService), crmService).Build());
            }
            private EntityCollection GetEPDonations(IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("wowc_loot");
                query.ColumnSet.AddColumns("wowc_name", "wowc_epvalue");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
                query.Criteria.AddCondition("wowc_marketrate", ConditionOperator.GreaterThan, 0);
                query.Criteria.AddCondition("wowc_epvalue", ConditionOperator.GreaterThan, 0);
                query.Orders.Add(new OrderExpression("wowc_name", OrderType.Ascending));

                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
            }
            private EmbedBuilder epFormatEmbed(EntityCollection epRecords, IOrganizationService crmService)
            {
                EmbedBuilder epBuilder = new EmbedBuilder();
                int nameLength = 0;                

                for (int i = 0; i < epRecords.Entities.Count; i++)
                {
                    if(epRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length > nameLength)
                    {
                        nameLength = epRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length;
                    }
                }

                string commentString = "```" + "Name".PadRight(nameLength) + "EP Value".PadLeft(10);

                for (int i = 0; i < epRecords.Entities.Count; i++)
                {
                    string recordName = epRecords.Entities[i].GetAttributeValue<string>("wowc_name");
                    string epValue = epRecords.Entities[i].GetAttributeValue<Decimal>("wowc_epvalue").ToString("N3");

                    commentString += "\n" + recordName.PadRight(nameLength, '.') + epValue.PadLeft(10,'.');
                }
                commentString += "```";

                epBuilder.WithDescription(commentString);

                return epBuilder;
            }
        }
        [Group("-gp"), Summary("GP Value of Item")]
        public class GPModule : ModuleBase<SocketCommandContext>
        {
            public IOrganizationService crmService { get; set; }
            [Command("-s"), Summary("Return GP Values on items")]
            public async Task ReturnGPValues([Remainder]string itemSearch)
            {
                await ReplyAsync(null, false, GpFormatEmbed(GetGPValues(itemSearch,crmService),crmService).Build());
            }
            private EntityCollection GetGPValues(string itemName, IOrganizationService crmService)
            {
                QueryExpression query = new QueryExpression("wowc_loot");
                query.ColumnSet.AddColumns("wowc_name", "wowc_defaultgp","wowc_huntergpvalue");
                query.Criteria = new FilterExpression();
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
                query.Criteria.AddCondition("wowc_name", ConditionOperator.Like, "%" + itemName + "%");
                query.Orders.Add(new OrderExpression("wowc_name", OrderType.Ascending));

                EntityCollection results = crmService.RetrieveMultiple(query);

                return results;
            }
            private EmbedBuilder GpFormatEmbed(EntityCollection gpRecords, IOrganizationService crmService)
            {
                EmbedBuilder gpBuilder = new EmbedBuilder();

                int nameLength = 0;
                bool containsHunterValues = false;

                for (int i = 0; i < gpRecords.Entities.Count; i++)
                {
                    if (gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length > nameLength)
                    {
                        nameLength = gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length >= 30 ? 30 : gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length;

                        if (gpRecords.Entities[i].GetAttributeValue<Decimal>("wowc_huntergpvalue") > 0)
                            containsHunterValues = true;
                    }
                }

                string commentString = "";
                if (containsHunterValues)
                {
                    commentString += "```" + "Name".PadRight(nameLength+2) + "GP Value".PadLeft(10) + "Hunter GP".PadLeft(12);
                }
                else
                {
                    commentString += "```" + "Name".PadRight(nameLength+2) + "GP Value".PadLeft(10);
                }
                

                for (int i = 0; i < gpRecords.Entities.Count; i++)
                {
                    if (containsHunterValues)
                    {
                        string recordName = gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length >= 30 ? gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Substring(0,30) : gpRecords.Entities[i].GetAttributeValue<string>("wowc_name");
                        string gpValue = gpRecords.Entities[i].GetAttributeValue<Decimal>("wowc_defaultgp").ToString("N3");
                        string hunterGpValue = gpRecords.Entities[i].GetAttributeValue<Decimal>("wowc_huntergpvalue").ToString("N3");

                        commentString += "\n" + recordName.PadRight(nameLength+2, '.') + gpValue.PadLeft(10,'.') + hunterGpValue.PadLeft(12,'.');
                    }
                    else
                    {
                        string recordName = gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Length >= 30 ? gpRecords.Entities[i].GetAttributeValue<string>("wowc_name").Substring(0, 30) : gpRecords.Entities[i].GetAttributeValue<string>("wowc_name");
                        string gpValue = gpRecords.Entities[i].GetAttributeValue<Decimal>("wowc_defaultgp").ToString("N3");

                        commentString += "\n" + recordName.PadRight(nameLength+2, '.') + gpValue.PadLeft(10,'.');
                    }
                    
                }
                commentString += "```";

                gpBuilder.WithDescription(commentString);

                return gpBuilder;
            }
        }
    }
     
}
