using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace WoW.DKPEPGP.Plugins
{
    public class GearPointCalculator : IPlugin
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
                Entity image = context.PreEntityImages["PreImage"];

                Decimal ilvlCheck = 0;
                if (entity.Attributes.Contains("wowc_ilvl"))
                ilvlCheck = (Decimal)entity.Attributes["wowc_ilvl"];
                else
                ilvlCheck = (Decimal)image.Attributes["wowc_ilvl"];

                if (ilvlCheck > 0 )
                {
                    try
                    {
                       
                        Decimal ilvl = 0;
                        int rarityValue = 0;
                        Decimal slotModifier = 0;
                        Decimal four = 4;
                        Decimal twoFive = 2.5m;

                        if (entity.Attributes.Contains("wowc_ilvl"))
                            ilvl = (Decimal)entity.Attributes["wowc_ilvl"];
                        else
                            ilvl = (Decimal)image.Attributes["wowc_ilvl"];

                        if (entity.Attributes.Contains("wowc_rarityvalue"))
                            rarityValue = (int)entity.Attributes["wowc_rarityvalue"];
                        else
                            rarityValue = (int)image.Attributes["wowc_rarityvalue"];

                        if (entity.Attributes.Contains("wowc_slotmodifier"))
                            slotModifier = (Decimal)entity.Attributes["wowc_slotmodifier"];
                        else
                            slotModifier = (Decimal)image.Attributes["wowc_slotmodifier"];
                        
                        Decimal total = ((ilvl / four) * (rarityValue * slotModifier)) / twoFive;

                        entity.Attributes.Add("wowc_gp",total);
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
