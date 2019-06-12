using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace The_House_Bot.Core.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("help"), Alias("helpme, plzhalp"), Summary("Help command")]
        public async Task Nate()
        {
            await Context.Channel.SendMessageAsync("Here's your help");
        }

        [Command("embed"), Summary("Embed box")]
        public async Task Embed([Remainder]string Input = "None")
        {
            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithAuthor("The House Bot", Context.User.GetAvatarUrl());
            Embed.WithColor(40, 200, 150);
            Embed.WithFooter("The footer of the embed",Context.Guild.Owner.GetAvatarUrl());
            Embed.WithDescription("This is a dummy description, with a cool link.\n [This is my favorite website](https:\\www.google.com");
            Embed.AddField("User input:", Input);

            await Context.Channel.SendMessageAsync("", false, Embed.Build());
        }
    }
}
