using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // To run in your VSTS org, update the following to match your environment...
            Uri accountUri = new Uri("https://MicrosoftDMM.VisuaLStudio.com");                
            String personalAccessToken = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";     // See https://www.visualstudio.com/docs/integrate/get-started/authentication/pats            
            int workItemId = 1633;
            var identities = new string[] { "d-mckinstry@bigfoot.com", "davemcki@microsoft.com" };

            // Create a connection to the account
            VssConnection connection = new VssConnection(accountUri, new VssBasicCredential(string.Empty, personalAccessToken));
            
            // Get an instance of the work item tracking client
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            for (int i = 1; i <= 10; i++)
            {
                var user = identities[i % identities.Length];
                Console.WriteLine("Updating assign to as {0} for update #{1}", user, i);

                Program.UpdateWorkItemAsync(workItemId, user, witClient, i);
                // The following line is bad.  But I didn't want to fight with "await"ing in a console app so I gave it time to update.
                // You should be able to do a better job either with a more experienced programmer and/or a more friendly async host.
                System.Threading.Thread.Sleep(1000);
            }
        }

        private static async Task UpdateWorkItemAsync(int workItemId, string user, WorkItemTrackingHttpClient witClient, int i)
        {
            try
            {
                var patchDocument = new JsonPatchDocument();
                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.AssignedTo",
                    Value = user
                });

                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.ChangedBy",
                    Value = user
                });

                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.History",
                    Value = "This comment was added during the sync #" + i.ToString()
                });

                await witClient.UpdateWorkItemAsync(patchDocument, workItemId, bypassRules: true);

                //var workItem = await witClient.GetWorkItemAsync(workItemId);
                //Console.WriteLine("Retrieved work item revision #{0}", workItem.Rev);
            }
            catch (AggregateException aex)
            {
                VssServiceException vssex = aex.InnerException as VssServiceException;
                if (vssex != null)
                {
                    Console.WriteLine(vssex.Message);
                }
            }
        }
    }
}