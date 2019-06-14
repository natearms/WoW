using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

    public class IUserArray : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            string[] stringArray = input.Split(' ');
            IUser iuserArray = new IUser(stringArray);

            for (int i = 0; i < stringArray.Length; i++)
            {
                iuserArray[i] = typeof(IUser).stringArray[i];
                iuserArray.
            }
            
            if (iuserArray.GetType()==typeof(IUser))
                return Task.FromResult(TypeReaderResult.FromSuccess(iuserArray));

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a boolean."));
        }
    }

