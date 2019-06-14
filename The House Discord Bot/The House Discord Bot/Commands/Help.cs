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
        [Command("help"), Alias("helpme, plzhalp"), Summary("Help command")]
        public async Task HelpDetails()
        {
            var embed = new EmbedBuilder();
            var theHouseBotTrigger = "```thb! ";
            embed.WithTitle("The House Bot help file")
                .WithDescription("Below are commands that I currently understand:\n " +
                                 "__**The House DKP commands**__\n " +
                                 ""+theHouseBotTrigger+"dkp```\n" +
                                 "This command will attempt to parse the DKP system using your current nickname on the server and return your PR, EP, and GP values."+

                                 "\n\n__**Poll commands**__\n " +
                                 "" + theHouseBotTrigger + "poll: [title] {Option1} {Option2} {Option3}```\n" +
                                 "This will genearte a poll with reactions and up to 26 options."

                                 );
                

            await ReplyAsync("This bot is currently under construction.  If you have any suggestions on bot features you'd like to see, please let "+ Context.Guild.Owner.Mention +" know.");
            await Context.Message.Author.SendMessageAsync(null,false,embed.Build());
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
