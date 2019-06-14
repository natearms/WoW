using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;
using Char = System.Char;
using System.Timers;

namespace The_House_Discord_Bot.Commands
{
    public class Polls : ModuleBase<SocketCommandContext>
    {
        [Group("poll:"), Alias("polls:"), Summary("Poll generator")]
        public class PollGroup : ModuleBase<SocketCommandContext>
        {
            [Command(""), Summary("Poll builder")]
            public async Task pollBuilder([Remainder] string messageContent)
            {
                string[] stringArray2 = messageContent.Split(' ');
                string[] stringArray = Regex.Split(messageContent, @"\[(.*?)\]|\{(.*?)\}");

                stringArray = stringArray.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                string pollHeader = "";
                string pollDetails = string.Empty;
                char c1 = 'a';
                
                for (var i = 0; i < stringArray.Length; i++)
                {
                    if (i == 0)
                    {
                        pollHeader += "**" + stringArray.GetValue(i).ToString() + "**\n\n";
                    }
                    else
                    {
                        pollDetails += ":regional_indicator_" + c1 + ": " + stringArray.GetValue(i).ToString() + "\n\n";
                        c1++;
                    }
                    
                }

                var embed = new EmbedBuilder();
                embed.WithTitle(pollHeader);
                embed.WithDescription(pollDetails);

                RestUserMessage msg = await Context.Channel.SendMessageAsync("", false, embed.Build());
                CreateReactions(msg, stringArray.Length-1);   
            }
        }

        private static void CreateReactions(RestUserMessage msg, int reactions)
        {
            string[] emojiArray = new string[]
            {
                "\U0001f1e6", "\U0001f1e7", "\U0001f1e8", "\U0001f1e9", "\U0001f1ea", "\U0001f1eb", "\U0001f1ec", "\U0001f1ed", "\U0001f1ee"
                , "\U0001f1ef", "\U0001f1f0", "\U0001f1f1", "\U0001f1f2", "\U0001f1f3", "\U0001f1f4", "\U0001f1f5", "\U0001f1f6"
                , "\U0001f1f7", "\U0001f1f8", "\U0001f1f9", "\U0001f1fa", "\U0001f1fb", "\U0001f1fc", "\U0001f1fd", "\U0001f1fe","\U0001f1ff"
            };

            for (int i = 0; i < reactions; i++)
            {
                System.Threading.Thread.Sleep(1000);
                msg.AddReactionAsync(new Emoji(emojiArray.GetValue(i).ToString()));
            }
        }
    }
}
