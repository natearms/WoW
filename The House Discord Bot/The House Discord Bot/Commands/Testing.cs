using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace The_House_Discord_Bot.Commands
{
    public class Testing : ModuleBase<SocketCommandContext>
    {
        [Group("test:"), Alias("testing:"), Summary("Testing Class")]
        public class PollGroup : ModuleBase<SocketCommandContext>
        {
            [Command("compute1"), Summary("Poll builder")]
            public async Task pollBuilder(int int1, int int2)
            {
                int total = int1 + int2;
                await Context.Channel.SendMessageAsync(total.ToString());
            }

            [Command("compute2"), Summary("Poll builder")]
            public async Task pollBuilder(int int1, int int2, int int3)
            {
                int total = int1 + int2 + int3;
                await Context.Channel.SendMessageAsync(total.ToString());
            }

        }
    }
}
