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
    public sealed class CalculateMembersDaysActive : CodeActivity
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
                tracingService.Trace("Start of calculating guild members total days.");

                EntityCollection RaidMembers;
                {
                    tracingService.Trace("Getting Raid Members informaiton");
                    QueryExpression query = new QueryExpression("contact");
                    query.ColumnSet.AddColumns("contactid", "wowc_trialend");
                    query.Criteria = new FilterExpression();
                    query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");

                    RaidMembers = service.RetrieveMultiple(query);
                }
                
                foreach (var a in RaidMembers.Entities)
                {
                    tracingService.Trace("Setting users days in guild.");
                    

                    if(a.GetAttributeValue<DateTime>("wowc_trialend") != DateTime.MinValue)
                    {
                        Entity raidMember = new Entity("contact");
                        raidMember.Id = a.Id;
                        raidMember["wowc_daysasamember"] = (int)(DateTime.Now - a.GetAttributeValue<DateTime>("wowc_trialend")).TotalDays;
                        service.Update(raidMember);
                        tracingService.Trace("Set users days in guild.");
                    }
                    tracingService.Trace("User doesn't have an end trial date");
                }
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                throw;
            }
        }
    }
}
