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
         

        [Command("dkp"), Alias("pr, ep, gp"), Summary("Users DKP breakdown by PR/EP/GP")]
        public async Task PrEpGp()
        {
            CrmServiceClient crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString);
            IOrganizationService crmService = crmConn.OrganizationServiceProxy;
            Console.WriteLine(crmConn.IsReady);

            string guildNickname = Context.Guild.GetUser(Context.Message.Author.Id).Nickname;
            string userNickname = Context.Message.Author.Username;

            string userName = guildNickname == null ? userNickname : guildNickname;
            var userNameMention = Context.Message.Author.Mention;
            var currentUser = Context.Client.CurrentUser;

            EntityCollection userInfo = GetUserEpGp(userName, crmService);

            EmbedBuilder prBuilder = new EmbedBuilder();

            prBuilder.WithDescription("**Here is your DKP breakdown** " + userNameMention +
                                 Environment.NewLine+ Environment.NewLine + "**PR: **" + userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalpr").ToString("0.##") +
                                 Environment.NewLine + "**EP: **" + userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalep").ToString("0.##") +
                                 Environment.NewLine + "**GP: **" + userInfo.Entities[0].GetAttributeValue<Decimal>("wowc_totalgp").ToString("0.##")+ Environment.NewLine
                                 )
                
                .WithCurrentTimestamp();
            await ReplyAsync(null, false, prBuilder.Build());
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
