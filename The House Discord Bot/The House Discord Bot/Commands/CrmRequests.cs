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
         

        [Command("help2"), Alias("helpme, plzhalp"), Summary("Help command")]
        public async Task Nate()
        {
            CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString);
            IOrganizationService crmService = crmConn.OrganizationServiceProxy;
            Console.WriteLine(crmConn.IsReady);

            string userName = Context.Guild.GetUser(Context.User.Id).Nickname;
            EntityCollection userInfo = GetUserEpGp(userName, crmService);

            await ReplyAsync(userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalpr").ToString());
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
    }
}
