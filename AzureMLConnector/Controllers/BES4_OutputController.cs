using AzureMLConnector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using TRex.Metadata;

namespace AzureMLConnector.Controllers
{
    public class BES4_OutputController : ApiController
    {

        [Metadata("Batch Job With only Output", "Experiment has no web service input module, but has a web service output module (e.g. uses a Reader module")]
        [Swashbuckle.Swagger.Annotations.SwaggerResponse(HttpStatusCode.OK, "Finish", typeof(BatchScoreStatus))]
        [Swashbuckle.Swagger.Annotations.SwaggerResponseRemoveDefaults]
        [HttpPost, Route("api/OnlyOutput")]
        public async Task<HttpResponseMessage> Post(
              [Metadata("API POST URL", "Web Service Request URI")] string API_URL,
              [Metadata("API Key", "Web Service API Key")] string API_Key,
              
              [Metadata("Storage Account Name (Output)", "Azure Storage Account Name. Leave blank if same with Input")] string Output_AccountName,
              [Metadata("Storage Account Key (Output)", "Azure Storage Account Key. Leave blank if same with Input")] string Output_AccountKey,
              [Metadata("Storage Container Name (Output)", "Azure Storage Container Name. Leave blank if same with Input")] string Output_Container,
              [Metadata("Blob Name (Output)", "Azure Storage Blob Name. Include file extention. Leaving blank will set it default name")] string Output_Blob,
              [Metadata(FriendlyName = "Global Parameters Keys", Description = "Comma separated list of parameters", Visibility = VisibilityType.Advanced)] string GlobalKeys = "",
              [Metadata(FriendlyName = "Global Parameters Values", Description = "Comma separated list of values", Visibility = VisibilityType.Advanced)] string GlobalValues = ""

             )
        {
            BES_Output Obj = new BES_Output
            {
                API_URL = API_URL,
                API_Key = API_Key,
                
                Output_AccountName = Output_AccountName,
                Output_AccountKey = Output_AccountKey,
                Output_Container = Output_Container,
                Output_Blob = Output_Blob,
                GlobalKeys = GlobalKeys,
                GlobalValues = GlobalValues
            };
            BatchScoreStatus result = await BatchExecutionService.InvokeBatchExecutionService(Obj);
            HttpResponseMessage response = Request.CreateResponse<BatchScoreStatus>(HttpStatusCode.Accepted, result);
            response.Headers.Location = new Uri(string.Format("https://" + Request.RequestUri.Authority + "/api/CheckStatus?url={0}&api={1}", WebUtility.UrlEncode(result.JobLocation), WebUtility.UrlEncode(API_Key)));
            response.Headers.Add("Retry-after", "30");
            return response;
        }

    }
}
