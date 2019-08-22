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
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Xrm.Sdk;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace The_House_Discord_Bot.Commands
{
    public class Signups : ModuleBase<SocketCommandContext>
    {
        public IOrganizationService crmService { get; set; }
        
        [Command("-raid")]
        public async Task CreateRaid(string raid, string date, string time, string timeZone, [Remainder] string description)
        {
            #region Validate that the User has permissions
            bool approved = false;
            foreach (SocketRole role in ((SocketGuildUser)Context.Message.Author).Roles)
            {
                if (role.Id == 584754688423886858 || role.Id == 604383945567764490)
                {
                    approved = true;
                }
            }
            if (!approved)
            {
                await Context.Channel.SendMessageAsync("Sorry but you do not have permissions to use this command.");
                return;
            }
            #endregion

            SocketUser author = Context.Message.Author;
            string guildNickname = Context.Guild.GetUser(author.Id).Nickname;
            string userNickname = author.Username;
            string userName = guildNickname == null ? userNickname : guildNickname;

            EntityCollection authorCrmGuid = MessageAuthorCrmUser(userName, crmService);

            Tuple<int, string, string, DateTime, int, double, string> activityType = ScheduledActivityInformation(raid, date, time, timeZone, 4, description, crmService);

            if (timeZone != "CDT"  && timeZone != "PDT")
            {
                await Context.Channel.SendMessageAsync("Sorry I did not understand the timezone, please use CDT or PDT.");
                return;
            }
            
            EntityCollection raidSchedule = RaidSchedule(activityType.Item2 + " - " + activityType.Item4 + " CST", activityType.Item4, crmService);
            
            if(raidSchedule.Entities.Count == 0)
            {
                EmbedBuilder raidScheduler = BuildSignupEmbed(author, activityType);
                RestUserMessage msg = await Context.Channel.SendMessageAsync("@here", false, raidScheduler.Build());

                string[] emojiArray = new string[] { "\U0001F44D", "\U0001F44E", "\U0001F937" };
                for (int i = 0; i < emojiArray.Length; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    await msg.AddReactionAsync(new Emoji(emojiArray.GetValue(i).ToString()));
                }

                try
                {
                    CreateRaidScheduleRecord(activityType, msg.GetJumpUrl(), authorCrmGuid, crmService);
                    await Context.Channel.DeleteMessageAsync(Context.Message.Id);
                }
                catch (Exception ex)
                {
                    await Context.Channel.DeleteMessageAsync(msg.Id);
                    await Context.Guild.Owner.SendMessageAsync(ex.ToString(), false, null );
                    await Context.Channel.SendMessageAsync("Something went wrong when creating this event.  Please make sure that your nickname is set to your main in game. " + Context.Guild.Owner.Mention, false,null);
                    throw;
                }
                
            }
            else
            {
                await Context.Channel.SendMessageAsync("This event already exists.");
                return;
            }

            
        }
        private static EntityCollection RaidSchedule(string eventName, DateTime date, IOrganizationService crmService)
        {
            QueryExpression query = new QueryExpression("wowc_raidschedule");
            query.ColumnSet.AddColumns("wowc_raidscheduleid");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_name", ConditionOperator.Equal, eventName);
            //query.Criteria.AddCondition("wowc_raidactivity", ConditionOperator.Equal, raidOptionSet);
            //query.Criteria.AddCondition("wowc_datetimetext", ConditionOperator.Equal, date.ToString());
            
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
        private static Tuple<int, string, string, DateTime, int, double, string> ScheduledActivityInformation(string raid, string date, string time, string timeZone, double hours, string description, IOrganizationService crmService)
        {
            Tuple<int, string, string, DateTime, int, double, string> results;

            string raidText = raid.ToLower();
            double estDuration = hours;
            int timeZoneOffSet = 0;
            
            switch (timeZone)
            {
                case "PDT":
                    timeZoneOffSet = +2;
                    break;
                case "CDT":
                    timeZoneOffSet = 0;
                    break;
                default:
                    break;
            }

            DateTime raidDate = Convert.ToDateTime(date);
            DateTime raidTime = Convert.ToDateTime(time);
            DateTime combinedDateTime = raidDate.AddHours(raidTime.AddHours(timeZoneOffSet).Hour).AddMinutes(raidTime.Minute);

            if(raidText == "mc" || raidText == "molten core")
            {
                results = Tuple.Create(1, "Molten Core", @"https://vignette.wikia.nocookie.net/wowwiki/images/2/20/Molten_Core_loading_screen.jpg", combinedDateTime, timeZoneOffSet, estDuration, description);
            }
            else if (raidText == "ony" || raidText == "onyxia")
            {
                results = Tuple.Create(6, "Onyxia", @"https://vignette.wikia.nocookie.net/wowwiki/images/4/46/Onyxia's_Lair_loading_screen.jpg", combinedDateTime, timeZoneOffSet, estDuration, description);
            }
            else if (raidText == "bwl" || raidText == "blackwing lair")
            {
                results = Tuple.Create(2, "Blackwing Lair", "https://vignette.wikia.nocookie.net/wowwiki/images/0/09/Blackwing_Lair_loading_screen.jpg", combinedDateTime, timeZoneOffSet, estDuration, description);
            }
            else if (raidText == "aq40")
            {
                results = Tuple.Create(3, "AQ 40", @"https://vignette.wikia.nocookie.net/wowwiki/images/6/6a/Temple_of_Ahn'Qiraj_loading_screen.jpg", combinedDateTime, timeZoneOffSet, estDuration, description);
            }
            else if (raidText == "naxx" || raidText == "naxxramas")
            {
                results = Tuple.Create(4, "Naxxramas", @"https://vignette.wikia.nocookie.net/wowwiki/images/1/1f/Naxxramas_loading_screen.jpg", combinedDateTime, timeZoneOffSet, estDuration, description);
            }
            else
            {
                results = Tuple.Create(5, raid, @"https://vignette.wikia.nocookie.net/wowwiki/images/8/89/HordeCrest.jpg", combinedDateTime, timeZoneOffSet, estDuration, description);
            }

            return results;
        }
        private static EntityCollection UsersAsActivityParty(IOrganizationService crmService)
        {
            QueryExpression query = new QueryExpression("systemuser");
            query.ColumnSet.AddColumns("systemuserid");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, "0");
            query.Criteria.AddCondition("accessmode", ConditionOperator.EndsWith, "0");
            
            EntityCollection results = crmService.RetrieveMultiple(query);

            EntityCollection activityPartyCollection = new EntityCollection();
            foreach (Entity systemUser in results.Entities)
            {
                Entity party = new Entity("activityparty");
                party["partyid"] = new EntityReference("systemuser", systemUser.Id);

                activityPartyCollection.Entities.Add(party);
            }

            return activityPartyCollection;
        }
        private static EmbedBuilder BuildSignupEmbed(SocketUser author, Tuple<int, string, string, DateTime, int, double, string> activityInformation)
        {
            EmbedBuilder raidScheduler = new EmbedBuilder();
            //raidScheduler.WithTitle("Get your raid !");
            raidScheduler.WithTitle("Click this link to see The House event calendar!")
                .WithAuthor(author)
            .WithUrl("https://thehouse.crm.dynamics.com/workplace/home_calendar.aspx")
            .WithDescription(activityInformation.Item7)
            .AddField("Raid Location:", activityInformation.Item2, true)
            .AddField("Date:", activityInformation.Item4.ToShortDateString(), true)
            .AddField("Time PDT:", activityInformation.Item4.AddHours(-2).ToShortTimeString(), true)
            .AddField("Time CDT:", activityInformation.Item4.ToShortTimeString(), true)
            .WithThumbnailUrl(activityInformation.Item3)
            .WithFooter("Please react to let us know if you can make it or not.")
            .WithCurrentTimestamp();

            return raidScheduler;
        }
        private static void CreateRaidScheduleRecord(Tuple<int, string, string, DateTime, int, double, string> activityInformation,string messageUrl, EntityCollection crmUser, IOrganizationService crmService)
        {
            Guid raidScheduleGuid = Guid.NewGuid();

            Entity raidScheduleRecord = new Entity("wowc_raidschedule");
            raidScheduleRecord["wowc_raidscheduleid"] = raidScheduleGuid;
            raidScheduleRecord["wowc_name"] = activityInformation.Item2 + " - " + activityInformation.Item4 + " CST";
            raidScheduleRecord["wowc_raidactivity"] = new OptionSetValue(activityInformation.Item1);
            raidScheduleRecord["wowc_dateandtime"] = activityInformation.Item4;
            raidScheduleRecord["wowc_datetimetext"] = activityInformation.Item4.ToString();
            raidScheduleRecord["wowc_description"] = activityInformation.Item7;
            raidScheduleRecord["wowc_discordchatlink"] = messageUrl;
            raidScheduleRecord["wowc_createdby"] = new EntityReference("contact", crmUser[0].Id);

            crmService.Create(raidScheduleRecord);

            Guid appointmentGuid = Guid.NewGuid();

            Entity appointment = new Entity("appointment");
            appointment.Id = appointmentGuid;
            appointment["subject"] = activityInformation.Item2 + " - " + activityInformation.Item4 + " CST";
            appointment["description"] = activityInformation.Item7;
            appointment["scheduledstart"] = activityInformation.Item4;
            appointment["scheduledend"] = activityInformation.Item4.AddHours(activityInformation.Item6);
            appointment["requiredattendees"] = UsersAsActivityParty(crmService);
            appointment["regardingobjectid"] = new EntityReference("wowc_raidschedule", raidScheduleGuid);
            crmService.Create(appointment);

            Entity updateRaidSchedule = new Entity("wowc_raidschedule");
            updateRaidSchedule.Id = raidScheduleGuid;
            updateRaidSchedule["wowc_relatedappointment"] = new EntityReference("appointment", appointmentGuid);

            crmService.Update(updateRaidSchedule);
        }
    }
}
