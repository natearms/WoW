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

                        Decimal total = 0;
                        Decimal ilvl = 0;
                        int rarityValue = 0;
                        Decimal slotModifier = 0;
                        int wowc_farm = 1;
                        int wowc_offSpec = 1;
                        
                        ilvl = entity.Attributes.Contains("wowc_ilvl") ? (Decimal)entity.Attributes["wowc_ilvl"] : (Decimal)image.Attributes["wowc_ilvl"];
                        rarityValue = entity.Attributes.Contains("wowc_rarityvalue") ? (int)entity.Attributes["wowc_rarityvalue"] : (int)image.Attributes["wowc_rarityvalue"];
                        slotModifier = entity.Attributes.Contains("wowc_slotmodifier") ? (Decimal)entity.Attributes["wowc_slotmodifier"] : (Decimal)image.Attributes["wowc_slotmodifier"];

                        tracingService.Trace("Getting Farm Option Set");
                        wowc_farm = entity.Attributes.Contains("wowc_farm") ? (entity.GetAttributeValue<bool>("wowc_farm") ? 2 : 1) : (image.GetAttributeValue<bool>("wowc_farm") ? 2 : 1);
                        tracingService.Trace("Getting Off Spec Option Set");
                        wowc_offSpec = entity.Attributes.Contains("wowc_offspec") ? (entity.GetAttributeValue<bool>("wowc_offspec") ? 2 : 1) : (image.GetAttributeValue<bool>("wowc_offspec") ? 2 : 1);

                        if(wowc_offSpec == 2)
                        {
                            total = 0;
                        }
                        else
                        {
                            total = (Decimal)Math.Round((4 * Math.Pow(2, ((double)ilvl / 28 + (rarityValue - 4))) * (double)slotModifier / (double)wowc_offSpec) / (double)wowc_farm);
                        }                        

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
