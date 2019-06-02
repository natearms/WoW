using System;
using System.Activities;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;

public sealed class CalculateRollupStatistics : CodeActivity
{
    ///
    /// Executes the workflow activity.
    ///

    /// The execution context.

    [RequiredArgument]
    [Input("Contact")]
    [ReferenceTarget("contact")]
    public InArgument<EntityReference> iContact { get; set; }

    protected override void Execute(CodeActivityContext executionContext)
    {
        // Create the context
        IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();

        if (context == null)
        {
            throw new InvalidPluginExecutionException("Failed to retrieve workflow context.");
        }

        IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);


        try
        {
            EntityReference contactReference = iContact.Get(executionContext);
            List<string> rollupAttributes = new List<string>() { "wowc_totalep", "wowc_totalgp"};

            foreach (string attribute in rollupAttributes)
            {
                CalculateRollupFieldRequest reqUpdateRollup = new CalculateRollupFieldRequest
                {
                    FieldName = attribute,
                    Target = contactReference
                };
                service.Execute(reqUpdateRollup);
            }
        }
        catch (FaultException<OrganizationServiceFault> e)
        {
            
            throw;
        }
    }

}