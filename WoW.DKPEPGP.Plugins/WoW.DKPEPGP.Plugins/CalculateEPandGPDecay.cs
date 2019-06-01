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
        [Input("Test")]
        public InArgument<string> Test { get; set; }
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
                    query.ColumnSet.AddColumns("contactid", "wowc_totalep", "wowc_totalgp", "fullname");
                    query.Criteria = new FilterExpression();
                    query.Criteria.AddCondition("contactid", ConditionOperator.Equal, "{0B352331-7982-E911-A827-000D3A1D5A0B}");

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
                    Entity effortPoint = new Entity("letter");

                    effortPoint["subject"] = decayItemLoot.Attributes["wowc_name"] + " " + DateTime.Today.Date + "-" + a.GetAttributeValue<string>("fullname");
                    effortPoint["wowc_raidmember"] = new EntityReference("contact", a.GetAttributeValue<Guid>("contactid"));
                    effortPoint["wowc_item"] = new EntityReference("wowc_loot",decayItemLoot.GetAttributeValue<Guid>("wowc_lootid"));
                    effortPoint["wowc_efforttype"] = new OptionSetValue(Int32.Parse("257260006"));
                    effortPoint["wowc_category"] = decayItemLoot.Attributes["wowc_category"];
                    effortPoint["wowc_eprate"] = new Decimal(1);
                    effortPoint["wowc_epcount"] = new Decimal(1);
                    effortPoint["wowc_ep"] = (a.GetAttributeValue<decimal>("wowc_totalep") * new Decimal(.10)) * -1;

                    service.Create(effortPoint);
                }
                

            }
            catch (FaultException<OrganizationServiceFault> e)
            {

                throw;
            }

        }
        /*
        private static EntityCollection GetRaidMembers(IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("Getting Raid Members informaiton");
            QueryExpression query = new QueryExpression("contact");
            query.ColumnSet.AddColumns("contactid", "wowc_totalep", "wowc_totalgp", "fullname");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("contactid", ConditionOperator.Equal, "{0B352331-7982-E911-A827-000D3A1D5A0B}");

            EntityCollection results = service.RetrieveMultiple(query);
            return results;

        }

        private static EntityCollection GetDecayInfo(IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("Getting decay item informaiton");
            QueryExpression query = new QueryExpression("wowc_loot");
            query.ColumnSet.AddColumns("wowc_itemid", "wowc_name", "wowc_ilvl", "wowc_itemid", "wowc_rarity", "wowc_slot", "wowc_category");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_name", ConditionOperator.Equal, "Decay");

            EntityCollection results = service.RetrieveMultiple(query);
            return results;
        }
        private static void CreateEffortPoints(EntityCollection contactList, EntityCollection decayItem, IOrganizationService service, ITracingService tracingService)
        {
            Entity decayItemLoot = decayItem.Entities[0];
            
            foreach (var a in contactList.Entities)
            {
                tracingService.Trace("Creating EP Decay record");
                Entity effortPoint = new Entity("letter");

                effortPoint["subject"] = decayItemLoot.Attributes["wowc_name"] + "-" + a.GetAttributeValue<string>("fullname");
                effortPoint["wowc_raidmember"] = new EntityReference("contact", a.GetAttributeValue<Guid>("contactid"));
                effortPoint["wowc_item"] = decayItemLoot.Attributes["wowc_itemid"];
                effortPoint["wowc_efforttype"] = new OptionSetValue(Int32.Parse("257260006"));
                effortPoint["wowc_category"] = decayItemLoot.Attributes["wowc_category"];
                effortPoint["wowc_eprate"] = new Decimal(1);
                effortPoint["wowc_epcount"] = new Decimal(1);
                effortPoint["wowc_ep"] = a.GetAttributeValue<decimal>("wowc_totalep") * 10;

                service.Create(effortPoint);
            }
        }
        */
    }
}
