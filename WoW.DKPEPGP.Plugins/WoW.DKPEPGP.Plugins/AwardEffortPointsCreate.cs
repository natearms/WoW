using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;

namespace WoW.DKPEPGP.Plugins
{
    class AwardEffortPointsCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity entity = context.PostEntityImages["PostImage"];

                try
                {
                    ///enter execution code here
                    
                    service.Update(entity);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
        private static void GetRaidMembers()
        {
            QueryExpression query = new QueryExpression("wowc_loot");
            query.ColumnSet.AddColumns("wowc_itemid", "wowc_lootid");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_itemid", ConditionOperator.Equal, itemStats[0]);

            EntityCollection results = service.RetrieveMultiple(query);

            int totalrecords = results.Entities.Count;
        }
    }
}