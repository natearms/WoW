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

            #region SetTimezoneOffset
            int pstOffSet = 0;
            int cstOffset = 0;

            switch (timeZone)
            {
                case "PST":
                    pstOffSet = 0;
                    cstOffset = 2;
                    break;
                case "CST":
                    pstOffSet = -2;
                    cstOffset = 0;
                    break;
                default:
                    break;
            }

            if (pstOffSet == 0 && cstOffset == 0)
            {
                await Context.Channel.SendMessageAsync("Sorry I did not understand the timezone, please use CST or PST.");
                return;
            }
            #endregion

            string[] emojiArray = new string[] { "\U0001F44D", "\U0001F44E" };

            DateTime raidDate = Convert.ToDateTime(date);
            DateTime raidTime = Convert.ToDateTime(time);
            DateTime combinedDateTime = raidDate.AddHours(raidTime.AddHours(cstOffset).Hour).AddMinutes(raidTime.Minute);
            
            Tuple<int, string, string> activityType = ScheduledActivityType(raid, crmService);
            EntityCollection raidSchedule = RaidSchedule(activityType.Item1, combinedDateTime, crmService);

            if(raidSchedule.Entities.Count == 0)
            {
                Guid appointmentGuid = Guid.NewGuid();
                Guid raidScheduleGuid = Guid.NewGuid();

                Entity raidScheduleRecord = new Entity("wowc_raidschedule");
                raidScheduleRecord["wowc_raidscheduleid"] = raidScheduleGuid;
                raidScheduleRecord["wowc_name"] = activityType.Item2 + " - " + combinedDateTime + " CST";
                raidScheduleRecord["wowc_raidactivity"] = new OptionSetValue(activityType.Item1);
                raidScheduleRecord["wowc_dateandtime"] = combinedDateTime;
                raidScheduleRecord["wowc_datetimetext"] = combinedDateTime.ToString();
                raidScheduleRecord["wowc_description"] = description;
                crmService.Create(raidScheduleRecord);

                Entity appointment = new Entity("appointment");
                appointment.Id = appointmentGuid;
                appointment["subject"] = activityType.Item2 + " - " + combinedDateTime + " CST";
                appointment["description"] = description;
                appointment["scheduledstart"] = combinedDateTime;
                appointment["scheduledend"] = combinedDateTime.AddHours(4);
                appointment["requiredattendees"] = UsersAsActivityParty(crmService);
                appointment["regardingobjectid"] = new EntityReference("wowc_raidschedule", raidScheduleGuid);
                crmService.Create(appointment);

                Entity updateRaidSchedule = new Entity("wowc_raidschedule");
                updateRaidSchedule.Id = raidScheduleGuid;
                updateRaidSchedule["wowc_relatedappointment"] = new EntityReference("appointment", appointmentGuid);

                crmService.Update(updateRaidSchedule);
            }
            else
            {
                await Context.Channel.SendMessageAsync("This raid event already exists.");
                return;
            }

            EmbedBuilder raidScheduler = new EmbedBuilder();
            //raidScheduler.WithTitle("Get your raid !");
            raidScheduler.WithTitle("Click this link to see The House event calendar!")
            .WithUrl("https://thehouse.crm.dynamics.com/workplace/home_calendar.aspx")
            .WithDescription(description)
            .AddField("Raid Location:", activityType.Item2, true)
            .AddField("Date:", raidDate.ToShortDateString(), true)
            .AddField("Time PST:", raidTime.AddHours(pstOffSet).ToShortTimeString(), true)
            .AddField("Time CST:", raidTime.AddHours(cstOffset).ToShortTimeString(), true)
            .WithThumbnailUrl(activityType.Item3)
            .WithFooter("Please react to let us know if you can make it or not.")
            .WithCurrentTimestamp();

            await Context.Channel.DeleteMessageAsync(Context.Message.Id);
            RestUserMessage msg = await Context.Channel.SendMessageAsync(null, false, raidScheduler.Build());
            
            for (int i = 0; i < emojiArray.Length; i++)
            {
                System.Threading.Thread.Sleep(1000);
                await msg.AddReactionAsync(new Emoji(emojiArray.GetValue(i).ToString()));
            }
        }
        private static EntityCollection RaidSchedule(int raidOptionSet, DateTime date, IOrganizationService crmService)
        {
            QueryExpression query = new QueryExpression("wowc_raidschedule");
            query.ColumnSet.AddColumns("wowc_raidscheduleid");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_raidactivity", ConditionOperator.Equal, raidOptionSet);
            query.Criteria.AddCondition("wowc_datetimetext", ConditionOperator.Equal, date.ToString());
            
            EntityCollection results = crmService.RetrieveMultiple(query);

            return results;
        }
        private static Tuple<int, string, string> ScheduledActivityType(string activityType, IOrganizationService crmService)
        {
            Tuple<int, string, string> results;
            switch (activityType.ToLower())
            {
                case "mc":
                    results = Tuple.Create(1,"Molten Core", @"https://vignette.wikia.nocookie.net/wowwiki/images/2/20/Molten_Core_loading_screen.jpg");
                    break;
                case "ony":
                    results = Tuple.Create(6, "Onyxia", @"https://vignette.wikia.nocookie.net/wowwiki/images/4/46/Onyxia's_Lair_loading_screen.jpg");
                    break;
                case "bwl":
                    results = Tuple.Create(2, "Blackwing Lair", "https://vignette.wikia.nocookie.net/wowwiki/images/0/09/Blackwing_Lair_loading_screen.jpg");
                    break;
                case "aq40":
                    results = Tuple.Create(3, "AQ 40", @"https://vignette.wikia.nocookie.net/wowwiki/images/6/6a/Temple_of_Ahn'Qiraj_loading_screen.jpg");
                    break;
                case "naxx":
                    results = Tuple.Create(4, "Naxxramas", @"https://vignette.wikia.nocookie.net/wowwiki/images/1/1f/Naxxramas_loading_screen.jpg");
                    break;
                default:
                    results = Tuple.Create(5, "Other", @"https://vignette.wikia.nocookie.net/wowwiki/images/8/89/HordeCrest.jpg");
                    break;
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
    }
}
