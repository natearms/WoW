using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace The_House_Discord_Bot.DiscordFunctions
{
    public class MiscellaneousFunctions
    {
        public async Task ApiPost(DiscordSocketClient _client, ulong postingChannel)
        {
            int randomNumber = new Random().Next(1, 6);
            //randomNumber = 5;

            if (randomNumber == 1)
            {
                HttpClient client = new HttpClient();
                string returnString = await client.GetStringAsync("https://api.chucknorris.io/jokes/random");
                JObject o = JObject.Parse(returnString);
                string joke = (string)o["value"];

                await((ISocketMessageChannel)_client.GetChannel(postingChannel)).SendMessageAsync(joke);
            }
            else if (randomNumber == 2)
            {
                HttpClient client = new HttpClient();
                string returnString = await client.GetStringAsync("https://uselessfacts.jsph.pl/random.json?language=en");
                JObject o = JObject.Parse(returnString);
                string uselessFact = (string)o["text"];
                await((ISocketMessageChannel)_client.GetChannel(postingChannel)).SendMessageAsync("Fact: " + uselessFact);
            }
            else if (randomNumber == 3)
            {
                HttpClient client = new HttpClient();
                string returnString = await client.GetStringAsync("https://catfact.ninja/fact");
                JObject o = JObject.Parse(returnString);
                string catFact = (string)o["fact"];
                await((ISocketMessageChannel)_client.GetChannel(postingChannel)).SendMessageAsync("Cat facts: " + catFact);
            }
            else if (randomNumber == 4 || randomNumber == 5 || randomNumber == 6)
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                string returnString = await client.GetStringAsync("https://icanhazdadjoke.com/");
                JObject o = JObject.Parse(returnString);
                string dadJoke = (string)o["joke"];
                await ((ISocketMessageChannel)_client.GetChannel(postingChannel)).SendMessageAsync(dadJoke);
            }
        }
    }
}
