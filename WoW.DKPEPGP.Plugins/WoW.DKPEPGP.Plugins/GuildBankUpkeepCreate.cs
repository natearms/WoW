using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace WoW.DKPEPGP.Plugins
{
    public class GuildBankUpkeepCreate : IPlugin
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
                
                if (entity.GetAttributeValue<OptionSetValue>("wowc_efforttype").Value.ToString() == "257260003" )
                {
                    try
                    {
                        tracingService.Trace("Search for Guild Bank Record");
                        
                        EntityCollection guildBankRecordCollection = GetGuildBankRecord(((EntityReference)entity.Attributes["wowc_item"]).Id, service);

                        if (guildBankRecordCollection.Entities.Count == 0)
                        {
                            Guid guildBankRecordGuid = Guid.NewGuid();

                            tracingService.Trace("Guild Bank Record not found");
                            Entity guildBankRecord = new Entity("wowc_guildbankrecord");
                            
                            guildBankRecord["wowc_name"] = ((EntityReference)entity.Attributes["wowc_item"]).Name;
                            guildBankRecord["wowc_item"] = entity.Attributes["wowc_item"];
                            guildBankRecord["wowc_inventory"] = (int)entity.GetAttributeValue<Decimal>("wowc_epcount");
                            guildBankRecord["wowc_guildbankrecordid"] = guildBankRecordGuid;

                            tracingService.Trace("Creating Guild Bank Record");
                            service.Create(guildBankRecord);

                            entity["wowc_guildbankrecord"] = new EntityReference("wowc_guildbankrecord", guildBankRecordGuid);
                            entity["wowc_created"] = true;
                            service.Update(entity);
                        }
                        else if (guildBankRecordCollection.Entities.Count == 1)
                        {
                            tracingService.Trace("Guild Bank Record found");
                            Entity guildBankRecord = new Entity("wowc_guildbankrecord");

                            guildBankRecord.Id = guildBankRecordCollection[0].GetAttributeValue<Guid>("wowc_guildbankrecordid");
                            guildBankRecord["wowc_inventory"] = (int)entity.GetAttributeValue<Decimal>("wowc_epcount") + guildBankRecordCollection.Entities[0].GetAttributeValue<int>("wowc_inventory");

                            tracingService.Trace("Updating GUild Bank Record");
                            service.Update(guildBankRecord);

                            entity["wowc_guildbankrecord"] = new EntityReference("wowc_guildbankrecord", guildBankRecordCollection.Entities[0].GetAttributeValue<Guid>("wowc_guildbankrecordid"));
                            entity["wowc_created"] = true;
                            service.Update(entity);
                        }
                        else
                        {

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
        }
        private static EntityCollection GetGuildBankRecord(Guid itemId, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("wowc_guildbankrecord");
            query.ColumnSet.AddColumns("wowc_guildbankrecordid", "wowc_item", "wowc_inventory");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_item", ConditionOperator.Equal, itemId);

            EntityCollection results = service.RetrieveMultiple(query);
            return results;

        }
    }
}
