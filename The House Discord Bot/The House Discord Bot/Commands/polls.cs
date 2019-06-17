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
using System.Timers;
using RestSharp;
using RestSharp.Deserializers;

namespace The_House_Discord_Bot.Commands
{
    public class Polls : ModuleBase<SocketCommandContext>
    {
        [Group("poll:"), Alias("polls:"), Summary("Poll generator")]
        public class PollGroup : ModuleBase<SocketCommandContext>
        {
            /*
            [Command(""), Summary("Poll builder")]
            public async Task SingleQuestionPoll([Remainder] string messageContent)
            {
                string pollHeader = "**" + messageContent + "**\n\n";
                string pollDetails = ":thumbsup: :thumbsdown: :shrug:";
                string[] emojiArray = new string[] { "\U0001F44D", "\U0001F44E", "\U0001F937" };

                var embed = new EmbedBuilder();
                embed.WithTitle(pollHeader);

                RestUserMessage msg = await Context.Channel.SendMessageAsync("", false, embed.Build());

                for (int i = 0; i < emojiArray.Length; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    await msg.AddReactionAsync(new Emoji(emojiArray.GetValue(i).ToString()));
                }
                
            }
            */

            [Command("straw"), Summary("Poll builder")]
            public async Task StrawPoll([Remainder] string messageContent)
            {

                string[] stringArray = Regex.Split(messageContent, @"\[(.*?)\]|\{(.*?)\}");

                stringArray = stringArray.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                string pollHeader = "";
                string pollDetails = string.Empty;
                char c1 = 'a';

                for (var i = 0; i < stringArray.Length; i++)
                {
                    if (i == 0)
                    {
                        pollHeader += stringArray.GetValue(i).ToString();
                    }
                    else
                    {
                        pollDetails += "\"" + stringArray.GetValue(i).ToString() + "\",";
                        c1++;
                    }

                }

                var client = new RestClient("https://www.strawpoll.me/api/v2");
                
                var request = new RestRequest("polls", Method.POST);

                request.AddParameter("application/json", "{\"title\": \""+pollHeader+"\",\"options\": ["+pollDetails+"],\"multi\": true}", ParameterType.RequestBody);
        
                // execute the request
                IRestResponse response = client.Execute(request);
                var content = response.Content; // raw content as string

                string responseId = Regex.Match(content, @"\:(.*?)\,").ToString().Trim(':',',');
                /*
                var embed = new EmbedBuilder();
                embed.WithTitle(pollHeader);
                embed.WithDescription(pollDetails);
                */
                RestUserMessage msg = await Context.Channel.SendMessageAsync("https://www.strawpoll.me/"+responseId, false, null);
            }

            [Command("multi"), Summary("Poll builder")]
            public async Task MultiOptionPoll([Remainder] string messageContent)
            {
                
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
}
