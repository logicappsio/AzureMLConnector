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
    public class BES2_NoneController : ApiController
    {
        [Metadata("Batch Job No Input and Output", "Experiment does not have web service input or output module (e.g. uses a Reader and Writer module")]
        [Swashbuckle.Swagger.Annotations.SwaggerResponseRemoveDefaults]
        [HttpPost, Route("api/None")]
        public async Task<HttpResponseMessage> Post(
             [Metadata("API POST URL", "Web Service Request URI")] string _API_URL,
             [Metadata("API Key", "Web Service API Key")] string _API_Key,             
             [Metadata(FriendlyName = "Global Parameters Keys", Description = "Comma separated list of parameters", Visibility = VisibilityType.Advanced)] string _GlobalKeys = "",
             [Metadata(FriendlyName = "Global Parameters Values", Description = "Comma separated list of values", Visibility = VisibilityType.Advanced)] string _GlobalValues = ""

            )
        {
            BES_None Obj = new BES_None
            {
                API_URL = _API_URL,
                API_Key = _API_Key,                
                GlobalKeys = _GlobalKeys,
                GlobalValues = _GlobalValues
            };

            BatchScoreStatus result = await BatchExecutionService.InvokeBatchExecutionService(Obj);
            HttpResponseMessage response = Request.CreateResponse<BatchScoreStatus>(HttpStatusCode.Accepted, result);
            response.Headers.Location = new Uri(string.Format("https://" + Request.RequestUri.Authority + "/api/CheckStatus?url={0}&api={1}", WebUtility.UrlEncode(result.JobLocation), WebUtility.UrlEncode(_API_Key)));
            response.Headers.Add("Retry-after", "30");
            return response;
        }
    }
}
