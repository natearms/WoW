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


        ////Live Bot Trigger
        //private string botTrigger = "thb! ";
        //private string botToken = "NTg4NDgyNTAzOTcxNTY5Njkw.XQFxlQ.kOu5eynSGWL05-LJAL9XrbVAu8Y";
        //private ulong postingChannel = 584775200642564141;
        //private int recurrenceInterval = 60;
        //private int chatPostInterval = 25;


        //Test Bot Trigger
        private string botTrigger = "thbt! ";
        private string botToken = "NjEwODgwNTYwMTA0OTk2ODg0.XVLtjQ.jkFf9GkyyOiffLpwXPtdtEkUKIQ";
        private ulong postingChannel = 588449417481158662;
        private int recurrenceInterval = -1;
        private int chatPostInterval = 400;



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
            _client.UserJoined += Client_AnnouncedJoinedUser;
            _client.GuildMemberUpdated += Client_GuildMemberUpdated;

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

                    await new MiscellaneousFunctions().ApiPost(_client, postingChannel);
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

            bool approved = false;
            foreach (SocketRole role in ((SocketGuildUser)Context.Message.Author).Roles)
            {
                if (role.Id == 584755014648725524)
                {
                    approved = true;
                }
            }
            if (!approved)
            {
                await Context.Channel.SendMessageAsync("Sorry, but you need to be a member of The House in order to run commands.");
                return;
            }

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
        private async Task Client_AnnouncedJoinedUser(SocketGuildUser arg)
        {
            await arg.SendMessageAsync("Welcome to **<The House>** Discord server! The first and largest guild on Arcanite Reaper!  If you're interested in details on becoming a raider, please talk to **Raumedrius**."
                    + "\n\n" + "Please take a moment to review and follow our server rules."
                    + "\n" + "https://discordapp.com/channels/578967161322733578/584757220403707920/621077337349292067");
        }
        private async Task Client_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            bool memberRoleExisted = false;
            bool memberRoleAssigned = false;

            foreach (SocketRole role in arg1.Roles)
            {
                if(role.Id == 584755014648725524)
                {
                    memberRoleExisted = true;
                }
            }
            foreach (SocketRole role in arg2.Roles)
            {
                if (role.Id == 584755014648725524)
                {
                    memberRoleAssigned = true;
                }
            }

            if (!memberRoleExisted && memberRoleAssigned)
            {
                EmbedBuilder newMemberMessage = new EmbedBuilder();

                newMemberMessage.WithTitle("Welcome to <The House>!");
                newMemberMessage.WithDescription("It looks like you've had the member role assigned, welcome to the family!");
                newMemberMessage.AddField("Guild Information", "Please take a moment to review the posts under **" +arg1.Guild.GetCategoryChannel(608483000736284673).Name +"** > **#"+ arg1.Guild.GetChannel(584757445340037120).Name + "** for more information about us. \n" + "https://discordapp.com/channels/578967161322733578/584757445340037120/584758046555635732", false);
                newMemberMessage.AddField("Set your Discord Nickname!", "We utilize a custom built guild management system that requires you set your nickname to your in game character to take advantage of.  If this is not done, you may miss out on being able to use key features of our Discord bot such as not being able to sign up for events, maintain your profession information, run EP/GP commands, and more.", false);
                newMemberMessage.AddField("Set your Discord class color!", "Wondering why everyone has different colors for their names?  Well you too can set your class color by going to this link and reacting to one of the existing reactions to set this! \nhttps://discordapp.com/channels/578967161322733578/584757445340037120/610883091241631787", false);
                newMemberMessage.AddField("Any questions?", "Please feel free to reach out to ***Raumedrius*** with any further questions you might have!", false);
                
                await arg1.SendMessageAsync(null,false,newMemberMessage.Build());
            }
        }
    }
}
