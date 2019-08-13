using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace The_House_Discord_Bot.DiscordFunctions
{
    public class UserRoles
    {
        public static IRole RoleInformation(SocketReaction reaction)
        {
            IRole role;
            string roleName = reaction.Emote.Name;

            switch (roleName)
            {
                case "druid":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492505678970894);
                    break;
                case "hunter":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492340335443971);
                    break;
                case "mage":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492649703112724);
                    break;
                case "priest":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492724604993539);
                    break;
                case "rogue":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492846390804492);
                    break;
                case "shaman":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609492930108850197);
                    break;
                case "warlock":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609493012392837199);
                    break;
                case "warrior":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(609493091031842816);
                    break;
                case "tank":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(610918933347696641);
                    break;
                case "healer":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(610919681607467217);
                    break;
                case "dps":
                    role = ((IGuildChannel)reaction.Channel).Guild.GetRole(610919870732959774);
                    break;

                default:
                    role = null;
                    break;

            }

        return role;
        }

    }
}
