using System;
using System.Activities;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace WoW.DKPEPGP.Plugins
{
    public sealed class CalculateEPandGPDecay : CodeActivity
    {
        //[Input("Test")]
        //public InArgument<string> Test { get; set; }
        protected override void Execute(CodeActivityContext executionContext)
        {
            //Create the tracing service
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            //Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);


            try
            {
                tracingService.Trace("Create EP Decay");
                
                EntityCollection RaidMembers;
                {
                    tracingService.Trace("Getting Raid Members informaiton");
                    QueryExpression query = new QueryExpression("contact");
                    query.ColumnSet.AddColumns("contactid", "wowc_totalep", "wowc_totalgp", "fullname","wowc_class");
                    query.Criteria = new FilterExpression();
                    query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");

                    RaidMembers = service.RetrieveMultiple(query);
                }

                EntityCollection DecayInformation;
                {
                    tracingService.Trace("Getting decay item informaiton");
                    QueryExpression query = new QueryExpression("wowc_loot");
                    query.ColumnSet.AddColumns("wowc_lootid", "wowc_name", "wowc_ilvl", "wowc_rarity", "wowc_slot", "wowc_category");
                    query.Criteria = new FilterExpression();
                    query.Criteria.AddCondition("wowc_name", ConditionOperator.Equal, "Decay");

                    DecayInformation = service.RetrieveMultiple(query);
                }

                //CreateEffortPoints(GetRaidMembers(service, tracingService), GetDecayInfo(service, tracingService), service,tracingService);
                Entity decayItemLoot = DecayInformation.Entities[0];
                
                foreach (var a in RaidMembers.Entities)
                {
                    tracingService.Trace("Creating EP Decay record");
                    Entity effortPoint = new Entity("wowc_effortpoint");
                    Entity gearPoint = new Entity("wowc_gearpoint");

                    if(a.GetAttributeValue<Decimal>("wowc_totalep") > 0)
                    {
                        effortPoint["subject"] = decayItemLoot.Attributes["wowc_name"] + " - " + a.GetAttributeValue<string>("fullname") + " - " + DateTime.Today.ToShortDateString();
                        effortPoint["wowc_raidmember"] = new EntityReference("contact", a.GetAttributeValue<Guid>("contactid"));
                        effortPoint["wowc_item"] = new EntityReference("wowc_loot", decayItemLoot.GetAttributeValue<Guid>("wowc_lootid"));
                        effortPoint["wowc_efforttype"] = new OptionSetValue(Int32.Parse("257260006"));
                        effortPoint["wowc_category"] = decayItemLoot.Attributes["wowc_category"];
                        effortPoint["wowc_eprate"] = new Decimal(1);
                        effortPoint["wowc_epcount"] = new Decimal(1);
                        effortPoint["wowc_ep"] = (a.GetAttributeValue<Decimal>("wowc_totalep") * new Decimal(.10)) * -1;

                        service.Create(effortPoint);
                    }
                    
                    if(a.GetAttributeValue<Decimal>("wowc_totalgp") > 0)
                    {
                        gearPoint["subject"] = decayItemLoot.Attributes["wowc_name"] + " - " + a.GetAttributeValue<string>("fullname") + " - " + DateTime.Today.ToShortDateString();
                        gearPoint["wowc_raidmember"] = new EntityReference("contact", a.GetAttributeValue<Guid>("contactid"));
                        gearPoint["wowc_item"] = new EntityReference("wowc_loot", decayItemLoot.GetAttributeValue<Guid>("wowc_lootid"));
                        gearPoint["wowc_category"] = decayItemLoot.Attributes["wowc_category"];
                        gearPoint["wowc_class"] = a.Attributes["wowc_class"];
                        gearPoint["wowc_slot"] = decayItemLoot.Attributes["wowc_slot"];
                        gearPoint["wowc_rarity"] = decayItemLoot.Attributes["wowc_rarity"];
                        gearPoint["wowc_ilvl"] = new Decimal(0);
                        gearPoint["wowc_rarityvalue"] = 0;
                        gearPoint["wowc_slotmodifier"] = new Decimal(0);
                        gearPoint["wowc_gp"] = (a.GetAttributeValue<Decimal>("wowc_totalgp") * new Decimal(.10)) * -1;

                        service.Create(gearPoint);
                    }
                    
                }
                

            }
            catch (FaultException<OrganizationServiceFault> e)
            {

                throw;
            }

        }
        
    }
}
