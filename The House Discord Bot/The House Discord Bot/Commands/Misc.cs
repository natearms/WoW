using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Configuration;
using System.IdentityModel.Metadata;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace The_House_Discord_Bot.Commands
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        public IOrganizationService crmService { get; set; }

        [Command("-tsm"), Summary("Trade Skill Master string for current donations.")]
        public async Task TSMStringBUilder()
        {
            EntityCollection epDonations = GetEPDonations(crmService);
            string tsmStringBuilder = "`";

            for (int i = 0; i < epDonations.Entities.Count; i++)
            {
                tsmStringBuilder += epDonations.Entities[i].GetAttributeValue<string>("wowc_name") + " /exact; ";
            }
            tsmStringBuilder += "`";
            await ReplyAsync("Below is the TSM shopping list string for guild requested materials that reward EP. \n\n" + tsmStringBuilder,false,null);

        }
        [Command("-drink"), Summary("Randomly assign people to drink")]
        public async Task RaidMemberDrink()
        {
            int drinkCount = new Random().Next(4, 7);
            string drinkResults = "";
            int activeRaidMembers = 0;
            List<int> randomNumbers = new List<int>();

            EntityCollection fetchResults = crmService.RetrieveMultiple(
                    new FetchExpression(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='contact'>
                                                <attribute name='lastname' />
                                                <attribute name='contactid' />
                                                <order attribute='lastname' descending='false' />
                                                <filter type='and'>
                                                  <condition attribute='parentcustomerid' operator='eq' uiname='Active Raid Group' uitype='account' value='{BA455092-778A-E911-A81A-000D3A3B53C4}' />
                                                </filter>
                                              </entity>
                                            </fetch>"));

            activeRaidMembers = fetchResults.Entities.Count;
            
            if(activeRaidMembers == 0)
            {
                await ReplyAsync("Doesn't look like there's an active raid.", false, null);
                return;
            }

            if(drinkCount > 10)
            {
                await ReplyAsync("Whoa whoa whoa, lets take it easy there, how about you pick somewhere between 1-10 people to drink.  We do need to get this done at some point.", false, null);
            }
            else if (drinkCount <= 0)
            {
                await ReplyAsync("Enter more than 0! Make em drink!", false, null);
            }
            else if(drinkCount == 5 || drinkCount == 10)
            {
                if(drinkCount == 5)
                {
                    if (activeRaidMembers > 35)
                    {
                        await ReplyAsync("Group " + new Random().Next(1, 7) + " drink!", false, null);
                    }
                    else
                    {
                        await ReplyAsync("Group " + new Random().Next(1, 8) + " drink!", false, null);
                    }
                }
                else
                {
                    int groupA = new Random().Next(1, 8);
                    int groupB = 0;

                    do
                    {
                        groupB = new Random().Next(1, 8);
                    } while (groupA == groupB);

                    await ReplyAsync($"Groups {groupA} and {groupB} drink!", false, null);
                }
            }
            else
            {
                while (randomNumbers.Count < drinkCount)
                {
                    bool unique = true;

                    int randomNum = new Random().Next(activeRaidMembers);
                    foreach (var item in randomNumbers)
                    {
                        if (item == randomNum)
                        {
                            unique = false;
                            break;
                        }
                    }
                    if (unique)
                    {
                        randomNumbers.Add(randomNum);
                    }
                }

                int[] sortedRandomNumbers = randomNumbers.OrderBy(x => x).ToArray();

                for (int i = 0; i < sortedRandomNumbers.Length; i++)
                {
                    if (i == 0)
                    {
                        drinkResults += fetchResults.Entities[sortedRandomNumbers[i]].GetAttributeValue<string>("lastname");
                    }
                    else
                    {
                        drinkResults += ", " + fetchResults.Entities[sortedRandomNumbers[i]].GetAttributeValue<string>("lastname");
                    }
                }

                drinkResults += " time to drink!";

                await ReplyAsync(drinkResults, false, null);
            }

        }
        private static EntityCollection GetEPDonations(IOrganizationService crmService)
        {
            
            QueryExpression query = new QueryExpression("wowc_loot");
            query.ColumnSet.AddColumns("wowc_name");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_slot", ConditionOperator.In, 257260007, 257260008, 257260009);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
            query.Criteria.AddCondition("wowc_epvalue", ConditionOperator.GreaterThan, 0);

            EntityCollection results = crmService.RetrieveMultiple(query);
            return results;
        }
    }
}
