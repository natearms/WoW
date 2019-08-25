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
        public static void SignUpRecord(IGuildUser user, string messageUrl, IOrganizationService crmService, SocketReaction reaction, bool deleted, IUser guildOwner)
        {
            try
            {
                string guildNickname = user.Nickname;
                string userNickname = user.Username;
                string userName = guildNickname == null ? userNickname : guildNickname;

                if (guildNickname == null)
                {
                    user.SendMessageAsync("Please make sure you set your server nickname to your in game character name otherwise several functions of the discord bot will not work properly.", false, null);
                    UserExtensions.SendMessageAsync(guildOwner, userNickname + " tried to singup for an event but has not set their nickname in the Discord server.", false, null);
                    return;
                }

                EntityCollection raidSignup = ExistingRaidSchedule(messageUrl, crmService);
                EntityCollection crmUser = ReactedCrmUser(userName, crmService);

                if (crmUser.Entities.Count == 0)
                {
                    
                    user.SendMessageAsync("It looks like you haven't been setup in CRM yet, I've messaged Raumedrius to create a user for you in CRM.", false, null);
                    UserExtensions.SendMessageAsync(guildOwner, guildNickname + " does not exist in CRM.", false, null);
                    return;
                }
                EntityCollection existingAttendance = CheckExistingAttendance(crmUser.Entities[0].GetAttributeValue<Guid>("contactid"), raidSignup.Entities[0].GetAttributeValue<Guid>("wowc_raidscheduleid"), crmService);

                string emoteName = reaction.Emote.ToString();
                int attendingOptionSet = 0;
                switch (emoteName)
                {
                    case "👍":
                        attendingOptionSet = 257260000;
                        break;
                    case "👎":
                        attendingOptionSet = 257260001;
                        break;
                    default:
                        attendingOptionSet = 257260002;
                        break;
                }
                if (raidSignup.Entities.Count == 1 && crmUser.Entities.Count == 1 && existingAttendance.Entities.Count == 1 && deleted)
                {
                    crmService.Delete("wowc_raidsignup", existingAttendance.Entities[0].GetAttributeValue<Guid>("wowc_raidsignupid"));
                }
                else if (raidSignup.Entities.Count == 1 && crmUser.Entities.Count == 1 && existingAttendance.Entities.Count == 0)
                {
                    Guid raidSignupGuid = Guid.NewGuid();
                    Entity raidSignupEntity = new Entity("wowc_raidsignup");
                    raidSignupEntity.Id = raidSignupGuid;
                    raidSignupEntity["wowc_name"] = raidSignup.Entities[0].GetAttributeValue<string>("wowc_name");
                    raidSignupEntity["wowc_raider"] = new EntityReference("contact", crmUser.Entities[0].GetAttributeValue<Guid>("contactid"));
                    raidSignupEntity["wowc_raidschedule"] = new EntityReference("wowc_raidschedule", raidSignup.Entities[0].GetAttributeValue<Guid>("wowc_raidscheduleid"));
                    raidSignupEntity["wowc_attending"] = new OptionSetValue(attendingOptionSet);

                    crmService.Create(raidSignupEntity);

                }
                else if (raidSignup.Entities.Count == 1 && crmUser.Entities.Count == 1 && existingAttendance.Entities.Count == 1)
                {
                    Entity raidSignupEntity = new Entity("wowc_raidsignup");
                    raidSignupEntity.Id = existingAttendance.Entities[0].GetAttributeValue<Guid>("wowc_raidsignupid");
                    raidSignupEntity["wowc_attending"] = new OptionSetValue(attendingOptionSet);
                    crmService.Update(raidSignupEntity);

                }
            }
            catch (Exception ex)
            {
                UserExtensions.SendMessageAsync(guildOwner, "Error when attempting to create singup in CRM: \n" + ex, false, null);
                Console.WriteLine(ex);
                throw;
            }
            

        }

        private static EntityCollection CheckExistingAttendance(Guid raider, Guid signup, IOrganizationService crmService)
        {
            QueryExpression query = new QueryExpression("wowc_raidsignup");
            query.ColumnSet.AddColumns("wowc_raidsignupid");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_raider", ConditionOperator.Equal, raider);
            query.Criteria.AddCondition("wowc_raidschedule", ConditionOperator.Equal, signup);
            
            EntityCollection results = crmService.RetrieveMultiple(query);

            return results;
        }
        private static EntityCollection ReactedCrmUser(string username, IOrganizationService crmService)
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
            query.ColumnSet.AddColumns("wowc_raidscheduleid", "wowc_name");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_discordchatlink", ConditionOperator.Equal, discordChatLink);

            EntityCollection results = crmService.RetrieveMultiple(query);

            return results;
        }

    }
}
