using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace WoW.DKPEPGP.Plugins
{
    public class WeeklyDonationEPGain : IPlugin
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
                Entity entity = (Entity)context.InputParameters["Target"];
                
                if(entity.GetAttributeValue<OptionSetValue>("wowc_efforttype").Value == 257260003)
                {
                    try
                    {
                        EntityCollection lastWeeksDonations = GetLastSevenDaysDonations(entity.GetAttributeValue<EntityReference>("wowc_raidmember").Id, service);
                        Decimal totalEpGain = 0;

                        for (int i = 0; i < lastWeeksDonations.Entities.Count; i++)
                        {
                            totalEpGain += lastWeeksDonations.Entities[i].GetAttributeValue<Decimal>("wowc_ep");
                        }

                        Entity contact = new Entity("contact");
                        contact.Id = entity.GetAttributeValue<EntityReference>("wowc_raidmember").Id;
                        contact["wowc_last7daysofepdonations"] = (Decimal)totalEpGain;
                        service.Update(contact);
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
        }
        private static EntityCollection GetLastSevenDaysDonations(Guid contactId, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("wowc_effortpoint");
            query.ColumnSet.AddColumns("wowc_raidmember", "wowc_ep");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_raidmember", ConditionOperator.Equal, contactId);
            query.Criteria.AddCondition("wowc_efforttype", ConditionOperator.Equal, 257260003);
            query.Criteria.AddCondition("wowc_ep", ConditionOperator.NotEqual, 0);
            query.Criteria.AddCondition("createdon", ConditionOperator.GreaterEqual, DateTime.Now.AddDays(-7));

            EntityCollection results = service.RetrieveMultiple(query);
            return results;

        }
    }
}