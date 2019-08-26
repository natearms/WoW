using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Services.Description;
using System.Net.Http;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using The_House_Discord_Bot.DiscordFunctions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json.Linq;
using Microsoft.Xrm.Sdk.Query;
using ServiceCollection = Microsoft.Extensions.DependencyInjection.ServiceCollection;

namespace The_House_Discord_Bot
{
    class TheHouseBot
    {   
        private DiscordSocketClient _client;
        private CommandService _command;
        private IServiceProvider _servicePriveProvider;
        private IOrganizationService _crmConn;
        private int recurrenceFlag = 0;
        private int barrensChatActivity = 0;


        //Live Bot Trigger
        private string botTrigger = "thb! ";
        private string botToken = "NTg4NDgyNTAzOTcxNTY5Njkw.XQFxlQ.kOu5eynSGWL05-LJAL9XrbVAu8Y";
        private ulong postingChannel = 584775200642564141;
        private int recurrenceInterval = 18;
        private int chatPostInterval = 18;


        ////Test Bot Trigger
        //private string botTrigger = "thbt! ";
        //private string botToken = "NjEwODgwNTYwMTA0OTk2ODg0.XVLtjQ.jkFf9GkyyOiffLpwXPtdtEkUKIQ";
        //private ulong postingChannel = 588449417481158662;
        //private int recurrenceInterval = -1;
        //private int chatPostInterval = 0;



        static void Main(string[] args)
            => new TheHouseBot().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {   
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug
            });
            _command = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Debug
                
            });
            
           _crmConn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString).OrganizationServiceProxy;
           _servicePriveProvider = new ServiceCollection()
               .AddSingleton(_client)
               .AddSingleton(_command)
               .AddSingleton(_crmConn)
               .AddSingleton(botTrigger)
               .BuildServiceProvider();
            
            _client.MessageReceived += Client_MessageReceived;
            _client.Ready += Client_Ready;
            _client.Log += Client_Log;
            _client.ReactionAdded += Client_ReactionAdded;
            _client.ReactionRemoved += Client_ReactionRemoved;

            await _command.AddModulesAsync(Assembly.GetEntryAssembly(), _servicePriveProvider);
            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task Client_Log(LogMessage Message)
        {
            Console.WriteLine($"{DateTime.Now} at {Message.Source}] {Message.Message}");
            if (Message.Message != "Received HeartbeatAck")
            {
                recurrenceFlag++;
                
                if(recurrenceFlag > recurrenceInterval && barrensChatActivity > chatPostInterval)
                {
                    recurrenceFlag = 0;
                    barrensChatActivity = 0;

                    int randomNumber = new Random().Next(1,4);
                    //randomNumber = 2;
                    
                    if (randomNumber == 1)
                    {
                        HttpClient client = new HttpClient();
                        string returnString = await client.GetStringAsync("https://api.chucknorris.io/jokes/random");
                        JObject o = JObject.Parse(returnString);
                        string joke = (string)o["value"];

                        await ((ISocketMessageChannel)_client.GetChannel(postingChannel)).SendMessageAsync(joke);
                    }
                    else if (randomNumber == 2)
                    {
                        HttpClient client = new HttpClient();
                        string returnString = await client.GetStringAsync("https://sv443.net/jokeapi/category/Miscellaneous?blacklistFlags=nsfw,political,religious");
                        JObject o = JObject.Parse(returnString);
                        string jokeType = (string)o["type"];
                        if(jokeType == "twopart")
                        {
                            string theSetup = (string)o["setup"];
                            string theDelivery = (string)o["delivery"];
                            await ((ISocketMessageChannel)_client.GetChannel(postingChannel)).SendMessageAsync(theSetup);
                            Thread.Sleep(5000);
                            await ((ISocketMessageChannel)_client.GetChannel(postingChannel)).SendMessageAsync(theDelivery);
                        }
                        else if (jokeType == "single")
                        {
                            string joke = (string)o["joke"];
                            await ((ISocketMessageChannel)_client.GetChannel(postingChannel)).SendMessageAsync(joke);
                        }
                    }
                    else if (randomNumber == 3)
                    {
                        HttpClient client = new HttpClient();
                        string returnString = await client.GetStringAsync("https://uselessfacts.jsph.pl/random.json?language=en");
                        JObject o = JObject.Parse(returnString);
                        string uselessFact = (string)o["text"];
                        await ((ISocketMessageChannel)_client.GetChannel(postingChannel)).SendMessageAsync("Fact: " + uselessFact);
                    }
                    else if (randomNumber == 4)
                    {
                        HttpClient client = new HttpClient();
                        string returnString = await client.GetStringAsync("https://catfact.ninja/fact");
                        JObject o = JObject.Parse(returnString);
                        string catFact = (string)o["fact"];
                        await ((ISocketMessageChannel)_client.GetChannel(postingChannel)).SendMessageAsync("Cat facts: " + catFact);
                    }
                }
            }
        }

        private async Task Client_Ready()
        {
            await _client.SetGameAsync(botTrigger + "-help", "", ActivityType.Listening);
        }

        private async Task Client_MessageReceived(SocketMessage MessageParam)
        {
            var Message = MessageParam as SocketUserMessage;
            var Context = new SocketCommandContext(_client, Message);
            int ArgPos = 0;
            
            if(Message.Channel.Id == postingChannel)
            {
                if(Context.Message.Author.IsBot) return;

                barrensChatActivity++;
            }

            if (Context.User.IsBot) return;
            if (Context.Message == null || Context.Message.Content == "") return;
            if (!(Message.HasStringPrefix(botTrigger, ref ArgPos) || Message.HasMentionPrefix(_client.CurrentUser, ref ArgPos))) return;

            var Result = await _command.ExecuteAsync(Context, ArgPos, _servicePriveProvider);
                        
            if (!Result.IsSuccess)
            {
                Console.WriteLine($"{DateTime.Now} at Commands] Something went wrong with executing a command. Text: {Context.Message.Content} | Error: {Result.ErrorReason}");
                await Context.Channel.SendMessageAsync("Sorry but I did not recognize this command, please type **" + botTrigger + "-help** for a list of available commands that I understand.", false, null);
            }
        }
        private async Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            IGuildUser user = (IGuildUser)arg3.User.Value;
            IGuildUser guildOwner = await user.Guild.GetOwnerAsync();
            string messageUrl = "https://discordapp.com/channels/" + ((IGuildChannel)arg3.Channel).GuildId.ToString() + "/" + arg3.Channel.Id.ToString() + "/" + arg3.MessageId.ToString();

            if (user.IsBot)
                return;

            //Grant roles to users that choose their class and role when reacting to the "Choose your role" post under member-information
            if (arg1.Id == 610883091241631787)
            {
                IRole reactionRole = Reactions.RoleInformation(arg3);
                if (reactionRole != null)
                {
                    await (user).RemoveRoleAsync(reactionRole);
                }
            }
            if (arg3.Channel.Id == 614097420728401955 || arg3.Channel.Id == 614097533983129613 || arg3.Channel.Id == 614097574877724702 || arg3.Channel.Id == 588449417481158662)
            {
                Reactions.SignUpRecord(user, messageUrl, _crmConn, arg3, true, guildOwner);
            }
                
        }

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            IGuildUser user = (IGuildUser)arg3.User.Value;
            IGuildUser guildOwner = await user.Guild.GetOwnerAsync();
            string messageUrl = "https://discordapp.com/channels/"+ ((IGuildChannel)arg3.Channel).GuildId.ToString() + "/"+ arg3.Channel.Id.ToString() + "/" + arg3.MessageId.ToString();
            
            if (user.IsBot)
                return;

            //Grant roles to users that choose their class and role when reacting to the "Choose your role" post under member-information
            if (arg1.Id == 610883091241631787)
            {
                IRole reactionRole = Reactions.RoleInformation(arg3);
                if (reactionRole != null)
                {
                    await (user).AddRoleAsync(reactionRole);
                }
            }

            if(arg3.Channel.Id == 614097420728401955 || arg3.Channel.Id == 614097533983129613 || arg3.Channel.Id == 614097574877724702 || arg3.Channel.Id == 588449417481158662)
            {
                Reactions.SignUpRecord(user, messageUrl, _crmConn, arg3, false, guildOwner);
            }
        }
    }
}
