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
    public class Misc : ModuleBase<SocketCommandContext>
    {
        public IOrganizationService crmService { get; set; }

        [Command("-tsm"), Summary("Trade Skill Master string for current donations.")]
        public async Task TSMStringBUilder()
        {
            EntityCollection epDonations = GetEPDonations(crmService);
            string tsmStringBuilder = "`";

            for (int i = 0; i < epDonations.Entities.Count; i++)
            {
                tsmStringBuilder += epDonations.Entities[i].GetAttributeValue<string>("wowc_name") + " /exact; ";
            }
            tsmStringBuilder += "`";
            await ReplyAsync("Below is the TSM shopping list string for guild requested materials that reward EP. \n\n" + tsmStringBuilder,false,null);

        }
        private static EntityCollection GetEPDonations(IOrganizationService crmService)
        {
            
            QueryExpression query = new QueryExpression("wowc_loot");
            query.ColumnSet.AddColumns("wowc_name");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_slot", ConditionOperator.In, 257260007, 257260008, 257260009);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
            query.Criteria.AddCondition("wowc_epvalue", ConditionOperator.GreaterThan, 0);

            EntityCollection results = crmService.RetrieveMultiple(query);
            return results;
        }
    }
}
