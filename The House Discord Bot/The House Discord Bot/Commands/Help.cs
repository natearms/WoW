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
                "This will generate a simple poll with :thumbsup: :thumbsdown: :shrug:." +
                "" + theHouseBotTrigger + "poll: -single <title>```\n" +
                "This will generate a poll with reactions with up to 26 options." +
                "" + theHouseBotTrigger + "poll: -multi [title] {Option1} {Option2} {Option3}```\n" +
                "This will generate a straw poll with up to 30 options. \n  " +
                                 "true = multi select\n" +
                                 "false = single select\n" +
                "" + theHouseBotTrigger + "poll: -straw <true or false> [title] {Option1} {Option2} {Option3}```\n"+

                //DKP Commands help descriptions
                "__**<The House> DKP commands**__\n\n " +
                "Parse <The House> DKP system to return your PR, EP, and GP values." +
                "" +theHouseBotTrigger+"-dkp```\n" +
                "Parse <The House> DKP system using the mentioned user to return their PR, EP, and GP values." +
                "" + theHouseBotTrigger + "-dkp <@usermention>```\n" +
                "Parse <The House> DKP system using the mentioned users to return their PR, EP, and GP values. You can mention up to 10 users at a time." +
                "" + theHouseBotTrigger + "-dkp <@usermention1> <@usermention2> <@usermention3>```\n" +

                //Guild Bank Commands help descriptions
                "__**<The House> Guild Bank commands**__\n\n " +
                "Parse <The House> Guild Bank system for a specific item. The search will always search with wildcard behavior. Using \"%\" will return all records." +
                "" + theHouseBotTrigger + "-gb -search <item name>```\n" +
                "Parse <The House> Guild Bank system for a high need mats." +
                "" + theHouseBotTrigger + "-gb -highneed```\n"
                );

            await Context.Message.Author.SendMessageAsync(null, false, embed.Build());
            await ReplyAsync("Please see your DM's for help using this bot.");
        }

        [Command("embed")]
        public async Task SendRichEmbedAsync()
        {
            var embed = new EmbedBuilder();
            // Or with methods
            embed.AddField("Field title",
                "Field value. I also support [hyperlink markdown](https://example.com)!")
                .WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = "I am a footer.")
                .WithColor(Color.Blue)
                .WithTitle("I overwrote \"Hello world!\"")
                .WithDescription("I am a description.")
                .WithUrl("https://example.com")
                .WithCurrentTimestamp();
            await ReplyAsync("",false, embed.Build(), RequestOptions.Default);
        }
    }
}
