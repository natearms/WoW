using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace WoW.DKPEPGP.Plugins
{
    public class GearPointCalculatorCreate : IPlugin
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

                if (entity.GetAttributeValue<decimal>("wowc_ilvl") > 0)
                {
                    try
                    {

                        Decimal ilvl = (Decimal)entity.Attributes["wowc_ilvl"];
                        int rarityValue = (int)entity.Attributes["wowc_rarityvalue"];
                        Decimal slotModifier = (Decimal)entity.Attributes["wowc_slotmodifier"];
                        Decimal four = 4;
                        Decimal twoFive = 2.5m;

                        Decimal total = ((ilvl / four) * (rarityValue * slotModifier)) / twoFive;

                        entity["wowc_gp"] = total;
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
        }
    }
}
