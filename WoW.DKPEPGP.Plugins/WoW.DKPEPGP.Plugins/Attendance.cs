using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace WoW.DKPEPGP.Plugins
{
    public class Attendance : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if(context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                tracingService.Trace("Check to see if the Award Effort Point record was for a Boss Kill or Boss Attempt.");

                if(entity.GetAttributeValue<OptionSetValue>("wowc_efforttype").Value != 257260000 && entity.GetAttributeValue<OptionSetValue>("wowc_efforttype").Value != 257260001)
                {
                    tracingService.Trace("Effort type was not Boss Kill or Boss Attempt.");
                    return;
                }

                tracingService.Trace("Award Effort Point was for a Boss Kill or Boss Attempt");
                try
                {
                    tracingService.Trace("Search for an existing Attendance record.");
                    EntityCollection attendanceRecordCollection =  GetAttendanceRecord((DateTime)entity.Attributes["createdon"], ((EntityReference)entity.Attributes["wowc_item"]).Id, service);
                    Entity attendanceRecord = new Entity("wowc_attendance");

                    if(attendanceRecordCollection.Entities.Count == 0)
                    {
                        tracingService.Trace("No attendance records found, creating record.");
                        Guid attendanceRecordGuid = Guid.NewGuid();
                        attendanceRecord["wowc_attendanceid"] = attendanceRecordGuid;
                        attendanceRecord["wowc_boss"] = entity.Attributes["wowc_item"];
                        attendanceRecord["wowc_relatedawardeffortpointrecord"] = new EntityReference("wowc_awardeffortpoint", entity.Id);
                        attendanceRecord["wowc_attendancedate"] = ((DateTime)entity.Attributes["createdon"]).Date;
                        attendanceRecord["wowc_name"] = ((EntityReference)entity.Attributes["wowc_item"]).Name + " - " + ((DateTime)entity.Attributes["createdon"]).ToShortDateString();

                        service.Create(attendanceRecord);
                        attendanceRecord.Id = attendanceRecordGuid;
                        
                    }

                    if(attendanceRecordCollection.Entities.Count == 1)
                    {
                        tracingService.Trace("Attendance record was found.");
                        attendanceRecord = attendanceRecordCollection.Entities[0];
                    }

                    if(attendanceRecordCollection.Entities.Count > 1)
                    {
                        tracingService.Trace("There were multiple attendance records for this boss and date.");
                        return;
                    }

                    tracingService.Trace("Creating raid member relationships with the Attendance record.");
                    var relationship = new Relationship("wowc_wowc_attendance_contact");

                    if(entity.Contains("wowc_raidteam"))
                    {
                        EntityCollection contactCollection = GetRaidMembers(((EntityReference)entity.Attributes["wowc_raidteam"]).Id, service);
                       
                        for (int i = 0; i < contactCollection.Entities.Count; i++)
                        {
                            if (AttendanceRecordExistsValidation(relationship.SchemaName, attendanceRecord.Id, contactCollection.Entities[i].Id, service))
                            {
                                var contactRef = new EntityReference("contact", contactCollection.Entities[i].Id);
                                var contactRefCollection = new EntityReferenceCollection();
                                contactRefCollection.Add(contactRef);

                                service.Associate(attendanceRecord.LogicalName, attendanceRecord.Id, relationship, contactRefCollection);
                            }
                        }

                    }
                    if (entity.Contains("wowc_standbyteam"))
                    {
                        EntityCollection contactCollection = GetRaidMembers(((EntityReference)entity.Attributes["wowc_standbyteam"]).Id, service);

                        for (int i = 0; i < contactCollection.Entities.Count; i++)
                        {
                            if (AttendanceRecordExistsValidation(relationship.SchemaName, attendanceRecord.Id, contactCollection.Entities[i].Id, service))
                            {
                                var contactRef = new EntityReference("contact", contactCollection.Entities[i].Id);
                                var contactRefCollection = new EntityReferenceCollection();
                                contactRefCollection.Add(contactRef);

                                service.Associate(attendanceRecord.LogicalName, attendanceRecord.Id, relationship, contactRefCollection);
                            }
                        }
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
        private static EntityCollection GetAttendanceRecord(DateTime date, Guid boss, IOrganizationService service)
        {
            var dateFormatted = date.Date;
            QueryExpression query = new QueryExpression("wowc_attendance");
            query.ColumnSet.AddColumns("wowc_attendanceid", "wowc_boss", "wowc_attendancedate");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_boss", ConditionOperator.Equal, boss);
            query.Criteria.AddCondition("wowc_attendancedate", ConditionOperator.Equal, dateFormatted);

            EntityCollection results = service.RetrieveMultiple(query);
            return results;
        }
        private static bool AttendanceRecordExistsValidation(string relationshipEntityName, Guid attendanceGuid, Guid contactGuid, IOrganizationService service)
        {
            string entity1 = "wowc_attendance";
            string entity2 = "contact";
            QueryExpression query = new QueryExpression(entity1);
            //query.ColumnSet = new ColumnSet("wowc_attendanceid");

            LinkEntity linkEntity1 = new LinkEntity(entity1, relationshipEntityName, "wowc_attendanceid", "wowc_attendanceid", JoinOperator.Inner);
            LinkEntity linkEntity2 = new LinkEntity(relationshipEntityName, entity2, "contactid", "contactid", JoinOperator.Inner);
            linkEntity1.LinkCriteria.AddCondition("wowc_attendanceid", ConditionOperator.Equal, attendanceGuid);
            linkEntity2.LinkCriteria.AddCondition("contactid", ConditionOperator.Equal, contactGuid);
            linkEntity1.LinkEntities.Add(linkEntity2);
            query.LinkEntities.Add(linkEntity1);
            
            EntityCollection queryResults = service.RetrieveMultiple(query);

            bool results = queryResults.Entities.Count == 0 ? true : false;

            return results;
        }
    }
}
