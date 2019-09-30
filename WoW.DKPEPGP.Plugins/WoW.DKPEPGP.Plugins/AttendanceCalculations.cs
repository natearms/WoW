using System;
using System.Activities;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace WoW.DKPEPGP.Plugins
{
    public sealed class AttendanceCalculations : CodeActivity
    {
        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                EntityCollection raidMembers = GetRaidMembers(service);

                foreach (var members in raidMembers.Entities)
                {
                    DateTime trialEnd = members.GetAttributeValue<DateTime>("wowc_firstraidattended");
                    tracingService.Trace("First Raid Attended " + trialEnd);

                    Guid memberGuid = members.GetAttributeValue<Guid>("contactid");
                    tracingService.Trace("Member Guid " + memberGuid);

                    int lastThirtyAttendance = MemberAttendance(DateTime.Now.AddDays(-30), memberGuid, service);
                    int lastThirtyRaids = AttendanceRecords(DateTime.Now.AddDays(-30), service);

                    int lastSixtyAttendance = MemberAttendance(DateTime.Now.AddDays(-60), memberGuid, service);
                    int lastSixtyRaids = AttendanceRecords(DateTime.Now.AddDays(-60), service);

                    int totalMemberAttendance = MemberAttendance(trialEnd, memberGuid, service);
                    int totalMemberTrialEndRaids = AttendanceRecords(trialEnd, service);

                    tracingService.Trace("Counts: Member 30 = {0} - Raid 30 = {1} - Member 60 = {2} - Raid 60 = {3} - Total Member Attendance = {4} - Total Raids From First Raid Attended = {5} ", lastThirtyAttendance, lastThirtyRaids, lastSixtyAttendance, lastSixtyRaids, totalMemberAttendance, totalMemberTrialEndRaids);

                    Double thirtyDays = 0;
                    Double sixtyDays = 0;
                    Double totalAttendance = 0;

                    if (lastThirtyAttendance > 0)
                    {
                        thirtyDays = ((Double)lastThirtyAttendance / (Double)lastThirtyRaids)*100;
                    }
                    if (lastSixtyAttendance > 0)
                    {
                        sixtyDays = ((Double)lastSixtyAttendance / (Double)lastSixtyRaids)*100;
                    }
                    if (totalMemberAttendance > 0)
                    {
                        totalAttendance = ((Double)totalMemberAttendance / (Double)totalMemberTrialEndRaids)*100;
                    }

                    tracingService.Trace("Thirty% = {0} : Sixty% = {1} : Total% - {2}", thirtyDays, sixtyDays, totalAttendance);

                    Entity entity = new Entity("contact");

                    entity.Id = memberGuid;
                    entity["wowc_30daysattendance"] = (Double)thirtyDays;
                    entity["wowc_60daysattendance"] = (Double)sixtyDays;
                    entity["wowc_attendanceoverall"] = (Double)totalAttendance;
                    service.Update(entity);

                }
            }
            catch (FaultException<OrganizationServiceFault> e)
            {

                throw;
            }
        }
        private static EntityCollection GetRaidMembers(IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("contact");
            query.ColumnSet.AddColumns("contactid", "wowc_firstraidattended");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, "Active");
            query.Criteria.AddCondition("wowc_firstraidattended", ConditionOperator.NotNull);
            query.Criteria.AddCondition("wowc_firstraidattended", ConditionOperator.LessEqual, DateTime.Now);

            EntityCollection results = service.RetrieveMultiple(query);
            return results;
        }
        
        private static int AttendanceRecords(DateTime trialEndDate, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("wowc_attendance");
            query.ColumnSet.AddColumns("wowc_attendanceid");
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("wowc_attendancedate", ConditionOperator.GreaterEqual, trialEndDate);

            int results = service.RetrieveMultiple(query).Entities.Count;
            return results;
        }
        private static int MemberAttendance(DateTime trialEndDate, Guid contactGuid, IOrganizationService service)
        {
            string entity1 = "wowc_attendance";
            string entity2 = "contact";
            string relationshipEntityName = "wowc_wowc_attendance_contact";
            QueryExpression query = new QueryExpression(entity1);
            //query.ColumnSet = new ColumnSet("wowc_attendanceid");

            LinkEntity linkEntity1 = new LinkEntity(entity1, relationshipEntityName, "wowc_attendanceid", "wowc_attendanceid", JoinOperator.Inner);
            LinkEntity linkEntity2 = new LinkEntity(relationshipEntityName, entity2, "contactid", "contactid", JoinOperator.Inner);
            linkEntity2.LinkCriteria.AddCondition("contactid", ConditionOperator.Equal, contactGuid);
            linkEntity1.LinkEntities.Add(linkEntity2);
            query.LinkEntities.Add(linkEntity1);
            query.Criteria.AddCondition("wowc_attendancedate", ConditionOperator.GreaterEqual, trialEndDate);

            int results = service.RetrieveMultiple(query).Entities.Count;

            return results;
        }

    }
}
