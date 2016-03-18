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
    public class BES3_InputController : ApiController
    {
        [Metadata("Batch Job With only Input", "Experiment has a web service input module, but no web service output module (e.g. uses a Writer module")]
        [Swashbuckle.Swagger.Annotations.SwaggerResponseRemoveDefaults]
        [HttpPost, Route("api/OnlyInput")]
        public async Task<HttpResponseMessage> Post(
             [Metadata("API POST URL", "Web Service Request URI")] string API_URL,
             [Metadata("API Key", "Web Service API Key")] string API_Key,
             [Metadata("Storage Account Name (Input)", "Azure Storage Account Name")] string Input_AccountName,
             [Metadata("Storage Account Key (Input)", "Azure Storage Account Key")] string Input_AccountKey,
             [Metadata("Storage Container Name (Input)", "Azure Storage Container Name")] string Input_Container,
             [Metadata("Blob Name (Input)", "Azure Storage Blob Name")] string Input_Blob,
             
             [Metadata(FriendlyName = "Global Parameters Keys", Description = "Comma separated list of parameters", Visibility = VisibilityType.Advanced)] string GlobalKeys = "",
             [Metadata(FriendlyName = "Global Parameters Values", Description = "Comma separated list of values", Visibility = VisibilityType.Advanced)] string GlobalValues = ""

            )
        {
            BES_Input Obj = new BES_Input
            {
                API_URL = API_URL,
                API_Key = API_Key,
                Input_AccountName = Input_AccountName,
                Input_AccountKey = Input_AccountKey,
                Input_Container = Input_Container,
                Input_Blob = Input_Blob,
                
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
