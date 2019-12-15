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

namespace The_House_Discord_Bot.Utilities
{
    public class ResultsFormatter
    {
        public static string FormatResultsIntoTable(EntityCollection queryResults, string triggeredBy, string[] columnHeaders, string[] queryColumns)
        {
            string formattedString = "";
            string headerFormatted = "";
            string bodyFormatted = "";
            int[] columnWidths = columnHeaders.Select(x => x.Length).ToArray();

            //Determine field lengths
            foreach (var entity in queryResults.Entities)
            {
                for (int i = 0; i < queryColumns.Length; i++)
                {
                    if (entity.Attributes[queryColumns[i]].GetType() == typeof(Decimal))
                    {
                        if (entity.GetAttributeValue<Decimal>(queryColumns[i]).ToString("N3").Length > columnWidths[i])
                        {
                            columnWidths[i] = entity.GetAttributeValue<Decimal>(queryColumns[i]).ToString("N3").Length > 45 ? 45 : entity.GetAttributeValue<Decimal>(queryColumns[i]).ToString("N3").Length;
                        }
                    }
                    else
                    {
                        if (entity.Attributes[queryColumns[i]].ToString().Length > columnWidths[i])
                        {
                            columnWidths[i] = entity.Attributes[queryColumns[i]].ToString().Length > 45 ? 45 : entity.Attributes[queryColumns[i]].ToString().ToLower() == triggeredBy.ToLower() ? ("*" + entity.Attributes[queryColumns[i]].ToString()).Length : entity.Attributes[queryColumns[i]].ToString().Length;

                        }
                    }
                }
            }

            //Format header
            for (int i = 0; i < columnHeaders.Length; i++)
            {
                if(i == 0)
                {
                    headerFormatted = columnHeaders[i].PadRight(columnWidths[i]);
                }
                else
                {
                    headerFormatted += columnHeaders[i].PadLeft(columnWidths[i] + 2);
                }
            }

            //Format Body
            foreach (var entity in queryResults.Entities)
            {
                for (int i = 0; i < columnHeaders.Length; i++)
                {
                    if (i == 0)
                    {
                        if (entity.Attributes[queryColumns[i]].GetType() == typeof(Decimal))
                        {
                            bodyFormatted += entity.GetAttributeValue<Decimal>(queryColumns[i]).ToString("N3").PadRight(columnWidths[i], '.');
                        }
                        else
                        {
                            bodyFormatted += entity.Attributes[queryColumns[i]].ToString().ToLower() == triggeredBy.ToLower() ? ("*" + entity.Attributes[queryColumns[i]].ToString()).PadRight(columnWidths[i], '.') : entity.Attributes[queryColumns[i]].ToString().PadRight(columnWidths[i], '.');
                        }
                    }
                    else
                    {
                        if(entity.Attributes[queryColumns[i]].GetType() == typeof(Decimal))
                        {
                            bodyFormatted += entity.GetAttributeValue<Decimal>(queryColumns[i]).ToString("N3").PadLeft(columnWidths[i] + 2, '.');
                        }
                        else
                        {
                            bodyFormatted += entity.Attributes[queryColumns[i]].ToString().PadLeft(columnWidths[i] + 2, '.');
                        }
                    }
                }
                bodyFormatted += "\n";
            }

            formattedString = "```" + headerFormatted + "\n" + bodyFormatted + "```";

            return formattedString;
        }
    }
}
