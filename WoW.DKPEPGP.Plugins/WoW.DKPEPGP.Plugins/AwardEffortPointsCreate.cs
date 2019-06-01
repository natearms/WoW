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
    public class AwardEffortPointsCreate : IPlugin
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

                try
                {
                    
                    //Award Effort Points for Raid Team members
                    CreateEffortPoints(GetRaidMembers(((EntityReference)entity.Attributes["wowc_raidteam"]).Id, service), service, entity);

                    //Award Effort Points for Standby Team members
                    if (entity.Contains("wowc_standbyteam"))
                    {
                        tracingService.Trace("Get Standby Members");
                        CreateEffortPoints(GetRaidMembers(((EntityReference)entity.Attributes["wowc_standbyteam"]).Id, service), service, entity);
                    }

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
        private static EntityCollection GetRaidMembers(Guid raidTeam, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("contact");
            query.ColumnSet.AddColumns("parentcustomerid", "contactid", "fullname");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, raidTeam);

            EntityCollection results = service.RetrieveMultiple(query);
            return results;
            
        }

        private static void CreateEffortPoints(EntityCollection raidTeam, IOrganizationService service, Entity entity )
        {
            foreach (var a in raidTeam.Entities)
            {

                Entity effortPoint = new Entity("letter");
                
                effortPoint["subject"] = entity.Attributes["wowc_item"] + "-" + a.GetAttributeValue<string>("fullname");
                effortPoint["wowc_raidmember"] = new EntityReference("contact", a.GetAttributeValue<Guid>("contactid"));
                effortPoint["wowc_item"] = entity.Attributes["wowc_item"];
                effortPoint["wowc_efforttype"] = entity.Attributes["wowc_efforttype"];
                effortPoint["wowc_category"] = entity.Attributes["wowc_category"];
                effortPoint["wowc_eprate"] = entity.Attributes["wowc_eprate"];
                effortPoint["wowc_epcount"] = entity.Attributes["wowc_epcount"];
                effortPoint["wowc_ep"] = entity.Attributes["wowc_ep"];

                service.Create(effortPoint);
            }
        }
    }
}