using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using System.Web.Http;
using TRex.Metadata;

namespace AzureMLConnector.Controllers
{
    public class CheckStatusController : ApiController
    {
        [Metadata(FriendlyName = "Check Status", Description = "Check Status of Job", Visibility = VisibilityType.Internal)]
        //[Metadata("Check Status", "Experiment has web service input and output modules")]
        [Swashbuckle.Swagger.Annotations.SwaggerResponse(HttpStatusCode.Accepted, "Accepted", typeof(BatchScoreStatus))]
        [HttpGet, Route("api/CheckStatus")]
        //public async Task<BatchScoreStatus> Put([FromBody]
        //                                [Metadata("Input Blob Info", "Input Blob and Global Parameters Keys")]
        //                                    BES_Full Obj)
        public async Task<HttpResponseMessage> GET(string url, string api)
        {
             var result = await InvokeBatchExecutionService(url, api);
             return result;
        }

        private async Task<HttpResponseMessage> InvokeBatchExecutionService(string url, string api)
        {
            var response = new HttpResponseMessage();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", api);
                    //Task<string> response = null;
                    //response = client.GetStringAsync(url);

                    BatchScoreStatus status = new BatchScoreStatus();
                    response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        response.StatusCode = HttpStatusCode.NotFound;
                        return response;
                    }

                    status = await response.Content.ReadAsAsync<BatchScoreStatus>();


                    if (status.StatusCode == BatchScoreStatusCode.NotStarted || status.StatusCode == BatchScoreStatusCode.Running)
                    {
                        response.StatusCode = HttpStatusCode.Accepted;
                        response.Headers.Location = new Uri(Request.RequestUri.AbsoluteUri);    // Set location same with last time.
                        response.Headers.Add("Retry-after", "30");
                    }

                    else if (status.StatusCode == BatchScoreStatusCode.Failed || status.StatusCode == BatchScoreStatusCode.Cancelled)
                    {
                        response.StatusCode = HttpStatusCode.InternalServerError;
                    }

                    
                    else if (status.StatusCode == BatchScoreStatusCode.Finished)
                    {
                        status.SetAdditionInformation();
                        response = Request.CreateResponse<BatchScoreStatus>(HttpStatusCode.OK, status);
                    }                    

                    return response;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ReasonPhrase = ex.Message;
                return response;
            }
        }
        public class res_output
        {
            public string Name;
            public string ConnectionString;
            public string RelativeLocation;
            public string BaseLocation;
            public string SasBlobToken;
        }

        public class ResponseBES
        {
            public string Status;
            public List<res_output> lOutput = new List<res_output>();
            public string Details;
        }
        static ResponseBES ReadResponse(string json)
        {
            var objects = JObject.Parse(json);

            ResponseBES r = new ResponseBES();
            r.Status = objects.SelectToken("StatusCode").ToString();

            if (!string.IsNullOrEmpty(objects["Details"].ToString()))
                r.Details = objects["Details"].ToString();

            if (string.IsNullOrEmpty(objects["Results"].ToString()))
                return r;

            var outputList = JObject.Parse(objects["Results"].ToString());

            foreach (var _output in outputList)
            {
                res_output rop = new res_output();
                rop.Name = _output.Key;
                rop.ConnectionString = _output.Value["ConnectionString"] != null ? _output.Value["ConnectionString"].ToString() : null;
                rop.RelativeLocation = _output.Value["RelativeLocation"] != null ? _output.Value["RelativeLocation"].ToString() : null;
                rop.BaseLocation = _output.Value["BaseLocation"] != null ? _output.Value["BaseLocation"].ToString() : null;
                rop.SasBlobToken = _output.Value["SasBlobToken"] != null ? _output.Value["SasBlobToken"].ToString() : null;

                r.lOutput.Add(rop);
            }
            return r;
        }
    }
}
