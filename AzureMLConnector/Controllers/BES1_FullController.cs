using AzureMLConnector.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using TRex.Metadata;

namespace AzureMLConnector.Controllers
{
    public class BES1_FullController : ApiController
    {

        [Metadata("Batch Job With Input and Output", "Experiment has web service input and output modules")]
        [Swashbuckle.Swagger.Annotations.SwaggerResponse(HttpStatusCode.OK, "Finish", typeof(BatchScoreStatus))]
        [Swashbuckle.Swagger.Annotations.SwaggerResponseRemoveDefaults]
        [HttpPost, Route("api/Full")]
        
        public async Task<HttpResponseMessage> Post(
             [Metadata("API POST URL", "Web Service Request URI")] string API_URL,
             [Metadata("API Key", "Web Service API Key")] string API_KEY,
             [Metadata("Storage Account Name (Input)", "Azure Storage Account Name")] string Input_AccountName,
             [Metadata("Storage Account Key (Input)", "Azure Storage Account Key")] string Input_AccountKey,
             [Metadata("Storage Container Name (Input)", "Azure Storage Container Name")] string Input_Container,
             [Metadata("Blob Name (Input)", "Azure Storage Blob Name")] string Input_Blob,
             [Metadata("Storage Account Name (Output)", "Azure Storage Account Name. Leave blank if same with Input")] string Output_AccountName = "",
             [Metadata("Storage Account Key (Output)", "Azure Storage Account Key. Leave blank if same with Input")] string Output_AccountKey = "",
             [Metadata("Storage Container Name (Output)", "Azure Storage Container Name. Leave blank if same with Input")] string Output_Container = "",
             [Metadata("Blob Name (Output)", "Azure Storage Blob Name. Include file extention. Leaving blank will set it default name")] string Output_Blob = "",
             [Metadata(FriendlyName = "Global Parameters Keys", Description = "Comma separated list of parameters", Visibility = VisibilityType.Advanced)] string GlobalKeys = "",
             [Metadata(FriendlyName = "Global Parameters Values", Description = "Comma separated list of values", Visibility = VisibilityType.Advanced)] string GlobalValues = ""
            
            )
        {
            BES_Full Obj = new BES_Full
            {
                API_URL = API_URL,
                API_Key = API_KEY,
                Input_AccountName = Input_AccountName,
                Input_AccountKey = Input_AccountKey,
                Input_Container = Input_Container,
                Input_Blob = Input_Blob,
                Output_AccountName = Output_AccountName,
                Output_AccountKey = Output_AccountKey,
                Output_Container = Output_Container,
                Output_Blob = Output_Blob,
                GlobalKeys = GlobalKeys,
                GlobalValues = GlobalValues
            };

            if (string.IsNullOrEmpty(Obj.Output_AccountName))
                Obj.Output_AccountName = Obj.Input_AccountName;
            if (string.IsNullOrEmpty(Obj.Output_AccountKey))
                Obj.Output_AccountKey = Obj.Input_AccountKey;
            if (string.IsNullOrEmpty(Obj.Output_Container))
                Obj.Output_Container = Obj.Input_Container;
            if (string.IsNullOrEmpty(Obj.Output_Blob))
                Obj.Output_Blob = "output_" + Obj.Input_Blob;

            BatchScoreStatus result = await BatchExecutionService.InvokeBatchExecutionService(Obj);

            HttpResponseMessage response = Request.CreateResponse<BatchScoreStatus>(HttpStatusCode.Accepted, result);
            response.Headers.Location = new Uri(string.Format("https://" + Request.RequestUri.Authority + "/api/CheckStatus?url={0}&api={1}", WebUtility.UrlEncode(result.JobLocation), WebUtility.UrlEncode(API_KEY)));
            response.Headers.Add("Retry-after", "30");

            return response;
        }
    }
}
