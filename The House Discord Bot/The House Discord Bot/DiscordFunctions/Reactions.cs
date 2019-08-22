using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace The_House_Discord_Bot.DiscordFunctions
{
    public class Reactions
    {
        public static IRole RoleInformation(SocketReaction reaction)
        {
            IRole role;
            string roleName = reaction.Emote.Name;

            switch (roleName)
            {
                case "druid":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492505678970894);
                    break;
                case "hunter":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492340335443971);
                    break;
                case "mage":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492649703112724);
                    break;
                case "priest":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492724604993539);
                    break;
                case "rogue":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492846390804492);
                    break;
                case "shaman":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492930108850197);
                    break;
                case "warlock":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609493012392837199);
                    break;
                case "warrior":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609493091031842816);
                    break;
                case "tank":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(610918933347696641);
                    break;
                case "healer":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(610919681607467217);
                    break;
                case "dps":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(610919870732959774);
                    break;

                default:
                    role = null;
                    break;

            }

        return role;
        }
        public static void CreateSignupRecord(IGuildUser user, string messageUrl, IOrganizationService crmService)
        {
            string guildNickname = user.Nickname;
            string userNickname = user.Nickname;
            string userName = guildNickname == null ? userNickname : guildNickname;
            
            if (ExistingRaidSchedule(messageUrl, crmService).Entities.Count == 1)
            {
                //left off here, finish this shizzz
                //need to continue implimenting the function to create the singup
                //record in CRM when someone reacts
            }

        }

        private static EntityCollection CheckExistingAttendance(Guid raider, Guid signup, IOrganizationService crmService)
        {
            QueryExpression query = new QueryExpression("wowc_raidsignup");
            query.ColumnSet.AddColumns("wowc_raidsignupid");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_raider", ConditionOperator.Equal, raider);
            query.Criteria.AddCondition("wowc_raidsignupid", ConditionOperator.Equal, signup);
            
            EntityCollection results = crmService.RetrieveMultiple(query);

            return results;
        }
        private static EntityCollection MessageAuthorCrmUser(string username, IOrganizationService crmService)
        {
            QueryExpression query = new QueryExpression("contact");
            query.ColumnSet.AddColumns("contactid");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("lastname", ConditionOperator.Equal, username);

            EntityCollection results = crmService.RetrieveMultiple(query);

            return results;
        }
        private static EntityCollection ExistingRaidSchedule(string discordChatLink, IOrganizationService crmService)
        {
            QueryExpression query = new QueryExpression("wowc_raidschedule");
            query.ColumnSet.AddColumns("wowc_raidscheduleid");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_discordchatlink", ConditionOperator.Equal, discordChatLink);

            EntityCollection results = crmService.RetrieveMultiple(query);

            return results;
        }

    }
}
