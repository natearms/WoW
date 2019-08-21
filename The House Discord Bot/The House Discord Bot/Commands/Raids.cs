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
    public class Raids : ModuleBase<SocketCommandContext>
    {
        public IOrganizationService crmService { get; set; }

        [Command("-raid")]
        public async Task CreateRaid(string raid, string date, string time, string timeZone)
        {
            bool approved = false ;
            foreach (SocketRole role in ((SocketGuildUser)Context.Message.Author).Roles)
            {
                if(role.Id == 584754688423886858 || role.Id == 604383945567764490)
                {
                    approved = true;
                }
            }
            if (!approved)
            {
                await Context.Channel.SendMessageAsync("Sorry but you do not have permissions to use this command.");
                return;
            }

            int pstOffSet = 0;
            int cstOffset = 0;
            string[] emojiArray = new string[] { "\U0001F44D", "\U0001F44E"};
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

            if(pstOffSet == 0 && cstOffset == 0)
            {
                await Context.Channel.SendMessageAsync("Sorry I did not understand the timezone, please use CST or PST.");
                return;
            }

            DateTime raidDate = Convert.ToDateTime(date);
            DateTime raidTime = Convert.ToDateTime(time);
            
            EmbedBuilder raidScheduler = new EmbedBuilder();
            //raidScheduler.WithTitle("Get your raid !");
            raidScheduler.AddField("Raid", "Molten Core", true);
            raidScheduler.AddField("Date", raidDate.ToShortDateString(), true);
            raidScheduler.AddField("Time PST", raidTime.AddHours(pstOffSet).ToShortTimeString(), true);
            raidScheduler.AddField("Time CST", raidTime.AddHours(cstOffset).ToShortTimeString(), true);
            raidScheduler.WithThumbnailUrl("https://gamepedia.cursecdn.com/wowpedia/e/e2/Ragnaros_the_Firelord.png?version=7c856349d3ddbe433d0c25fefde336e3");
            //raidScheduler.WithDescription("Please react below if you are able to attend this raid.");

            await Context.Channel.DeleteMessageAsync(Context.Message.Id);
            RestUserMessage msg = await Context.Channel.SendMessageAsync(null, false, raidScheduler.Build());
            
            for (int i = 0; i < emojiArray.Length; i++)
            {
                System.Threading.Thread.Sleep(1000);
                await msg.AddReactionAsync(new Emoji(emojiArray.GetValue(i).ToString()));
            }
        }
    }
}
