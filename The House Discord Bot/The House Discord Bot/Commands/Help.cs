using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;

namespace The_House_Discord_Bot.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        public string botTrigger { get; set; }

        [Command("-help"), Alias("helpme, plzhalp"), Summary("Help command")]
        public async Task HelpDetails()
        {
            
            var embed = new EmbedBuilder();
            var theHouseBotTrigger = "```"+botTrigger+"";
            embed.WithTitle("The House Bot help file")
                .WithDescription("If you have any issues or have suggestions on bot features you'd like to see, please let " + Context.Guild.Owner.Mention + " know. \n\n Below are commands that I currently understand:\n\n " +
                //Poll Commands help descriptions
                "\n__**Poll commands**__\n\n " +
                "Create a simple poll with :thumbsup: :thumbsdown: :shrug:." +
                "" + theHouseBotTrigger + "-poll -s <title>```\n" +
                "Create a reaction poll with up to 26 options." +
                "" + theHouseBotTrigger + "-poll -m [title] {Option1} {Option2} {Option3}```\n" +
                "Create a straw poll with up to 30 options. \n  " +
                                 "true = multi select\n" +
                                 "false = single select\n" +
                "" + theHouseBotTrigger + "-poll -st <true or false> [title] {Option1} {Option2} {Option3}```\n"+

                //DKP Commands help descriptions
                "__**<The House> DKP commands**__\n\n " +
                "Return your PR, EP, and GP." +
                "" +theHouseBotTrigger+"-dkp -s```\n" +
                "Return PR, EP, and GP values for mentioned user." +
                "" + theHouseBotTrigger + "-dkp -s <@usermention>```\n" +
                "Return PR, EP, and GP values for mentioned users. You can mention up to 10 users at a time." +
                "" + theHouseBotTrigger + "-dkp -s <@usermention1> <@usermention2> <@usermention3>```\n" +
                "Return top list of users and their DKP." +
                "" + theHouseBotTrigger + "-dkp -top <number>```\n" +

                //Guild Bank Commands help descriptions
                "__**<The House> Guild Bank commands**__\n\n " +
                "Search Guild Bank records. Using \"%\" will return all records." +
                "" + theHouseBotTrigger + "-gb -s <item name>```\n" +
                "Return audit history for a specific item." +
                "" + theHouseBotTrigger + "-gb -a <item name>```\n" +
                "Search the guild bank for high need mats." +
                "" + theHouseBotTrigger + "-gb -hn```\n" +
                

                //Profession Commands help descriptions
                "__**<The House> Profession management commands**__\n\n " +
                "Add a recipe to your record in CRM." +
                "" + theHouseBotTrigger + "-prof -a <item name>```\n" +
                "Remove a recipe from your record in CRM." +
                "" + theHouseBotTrigger + "-prof -r <item name>```\n" +
                "Search for guild members that know this recpie." +
                "" + theHouseBotTrigger + "-prof -s <item name>```\n" +
                "Return recipes this guild member knows." +
                "" + theHouseBotTrigger + "-prof -s <@usermention>```\n"
                );

            await Context.Message.Author.SendMessageAsync(null, false, embed.Build());
            await ReplyAsync("Please see your DM's for help using this bot.");
        }
    }
}
