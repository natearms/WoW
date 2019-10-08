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

            var embedHelp = new EmbedBuilder();
            var theHouseBotTrigger = botTrigger;
            embedHelp.WithTitle("The House Bot help file")
                .WithDescription("If you have any issues or have suggestions on bot features you'd like to see, please let " + Context.Guild.Owner.Mention + " know. \n\n Below are commands that I currently understand:\n\n ");

            await Context.Message.Author.SendMessageAsync(null, false, embedHelp.Build());

            //Poll Commands embed
            var embedPoll = new EmbedBuilder();
            embedPoll.WithTitle("Poll Commands");
            embedPoll.AddField("Create a simple poll with :thumbsup: :thumbsdown: :shrug:.", theHouseBotTrigger + "-poll -s <title>\n" + theHouseBotTrigger + "-poll -s This is a simple poll", false);
            embedPoll.AddField("Create a reaction poll with up to 26 options.", theHouseBotTrigger + "-poll -m [title] {Option1} {Option2} {Option3}\n" + theHouseBotTrigger + "-poll -m [This is a reaction poll] {First Option} {Second Option} {Third Option}", false);
            embedPoll.AddField("Create a straw poll with up to 30 options.", "\ntrue = multi select\nfalse = single select\n" + theHouseBotTrigger + "-poll -st <true|false> [title] {Option1} {Option2} {Option3}\n" + theHouseBotTrigger + "-poll -st true [This is a straw poll] {First Option} {Second Option} {Third Option}", false);

            await Context.Message.Author.SendMessageAsync(null, false, embedPoll.Build());

            //DKP Commands help descriptions
            var embedDKP = new EmbedBuilder();
            embedDKP.WithTitle("DKP commands");
            embedDKP.AddField("Return your PR, EP, and GP.", theHouseBotTrigger + "-dkp -s", false);
            embedDKP.AddField("Return PR, EP, and GP values for mentioned user.", theHouseBotTrigger + "-dkp -s <@usermention>\n" + theHouseBotTrigger + "-dkp -s @Raumedrius", false);
            embedDKP.AddField("Return PR, EP, and GP values for mentioned users.", theHouseBotTrigger + "-dkp -s <@usermention1> <@usermention2> <@usermention3>\n" + theHouseBotTrigger + "-dkp -s @Raumedrius @Chapeau @Flarix", false);
            embedDKP.AddField("Return top list of users and their DKP.", theHouseBotTrigger + "-dkp -top <number>\n" + theHouseBotTrigger + "-dkp -top 10", false);

            await Context.Message.Author.SendMessageAsync(null, false, embedDKP.Build());

            //Guild Bank Commands help descriptions
            var embedGuildBank = new EmbedBuilder();
            embedGuildBank.WithTitle("Guild Bank commands");
            embedGuildBank.AddField("Search Guild Bank records. Using \"%\" will return all records.", theHouseBotTrigger + "-gb -s <item name>\n" + theHouseBotTrigger + "-gb -s Black Lotus", false);
            embedGuildBank.AddField("Return audit history for a specific item.", theHouseBotTrigger + "-gb -a <item name>\n" + theHouseBotTrigger + "-gb -a Black Lotus", false);
            embedGuildBank.AddField("Search the guild bank for high need mats.", theHouseBotTrigger + "-gb -hn", false);

            await Context.Message.Author.SendMessageAsync(null, false, embedGuildBank.Build());
            
            // Profession Commands help descriptions
            var embedProfession = new EmbedBuilder();
            embedProfession.WithTitle("Profession commands");
            embedProfession.AddField("Add a recipe to your record in CRM.", theHouseBotTrigger + "-prof -a <item name>\n" + theHouseBotTrigger + "-prof -a Enchant Weapon - Crusader", false);
            embedProfession.AddField("Remove a recipe from your record in CRM.", theHouseBotTrigger + "-prof -r <item name>\n" + theHouseBotTrigger + "-prof -r Enchant Weapon - Crusader", false);
            embedProfession.AddField("Search for guild members that know this recpie.", theHouseBotTrigger + "-prof -s <item name>\n" + theHouseBotTrigger + "-prof -s Enchant Weapon - Crusader", false);
            embedProfession.AddField("Return recipes this guild member knows.", theHouseBotTrigger + "-prof -s <@usermention>\n" + theHouseBotTrigger + "-prof -s @Raumedrius", false);
            embedProfession.AddField("Set your profession skill levels.", theHouseBotTrigger + "-prof -set <primary profession> <primary level> <secondary profession> <secondary level>\n" + theHouseBotTrigger + "-prof -set Blacksmithing 300 Mining 300", false);
            embedProfession.AddField("Set your primary or secondary profession skill level.", theHouseBotTrigger + "-prof -set <primary|secondary> <profession> <level>\n" + theHouseBotTrigger + "-prof -set primary Alchemy 300", false);

            await Context.Message.Author.SendMessageAsync(null, false, embedProfession.Build());

            // Blacklist Commands help descriptions
            var embedBlacklist = new EmbedBuilder();
            embedBlacklist.WithTitle("Profession commands");
            embedBlacklist.AddField("Create a new blacklist record.", theHouseBotTrigger + "-bl -n <Character name> <Detailed description of what happened>\n" + theHouseBotTrigger + "-bl -n Raumedrius This person was a terrible tank, doesn't know how to hold threat, mark targets, and communicate with members of the party.", false);
            embedBlacklist.AddField("Search to see if someone was blacklisted.", theHouseBotTrigger + "-bl -s <Character name>\n" + theHouseBotTrigger + "-bl -s Raumedrius", false);
            
            await Context.Message.Author.SendMessageAsync(null, false, embedBlacklist.Build());

            // Event Commands help descriptions
            var embedSignups = new EmbedBuilder();
            embedSignups.WithTitle("Signup commands");
            embedSignups.AddField("Create a new event.", theHouseBotTrigger + "-event <eventname> <date> <time+am/pm> <timezone> <estimated hours> <description>\n" + theHouseBotTrigger + "-event MC 1/1/19 6:00pm PST 4 MC raiding, be there to get your loot!", false);
            
            await Context.Message.Author.SendMessageAsync(null, false, embedSignups.Build());

            /*
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
                "" + theHouseBotTrigger + "-prof -s <@usermention>```\n" +
                "Set your profession skill levels." +
                "" + theHouseBotTrigger + "-prof -set <primary profession> <primary level> <secondary profession> <secondary level>```\n" +
                "Set your primary or secondary profession skill level." +
                "" + theHouseBotTrigger + "-prof -set <primary|secondary> <profession> <level>```\n" +

                //Blacklist Commands help descriptions
                "__**<The House> Blacklist commands**__\n\n " +
                "Create a new blacklist record." +
                "" + theHouseBotTrigger + "-bl -n <Character name> <Detailed description of what happened>```\n" +
                "Search to see if someone was blacklisted." +
                "" + theHouseBotTrigger + "-bl -s <Character name>```\n" 
                );

            await Context.Message.Author.SendMessageAsync(null, false, embed.Build());
            */
            await ReplyAsync("Please see your DM's for help using this bot.");
        }
    }
}
