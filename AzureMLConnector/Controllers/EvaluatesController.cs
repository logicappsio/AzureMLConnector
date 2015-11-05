using AzureMLConnector.Models;
using AzureMLConnector.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using TRex.Metadata;

namespace AzureMLConnector.Controllers
{
    public class EvaluatesController : ApiController
    {
        [Metadata("Set Up Retraining", "Set up a one-time or scheduled retraining of your ML model.")]
        [HttpPost, Route("api/Retraining")]
        [ResponseType(typeof(ResponeObject))]
        //[SwaggerDefaultValue("Compare", "<", "<,<=,>,>=,=")]
        //[SwaggerDefaultValue("Model_URI", "@{body('besconnector').Results.output2.FullURL}")]
        //[SwaggerDefaultValue("Evaluate_Output_Path", "@{body('besconnector').Results.output1.FullURL}")]
        public async Task<HttpResponseMessage> Post(
            [Metadata("Retrained Model URL", "This should be the URL of the *.ilearner file which is the output of the call to the Retraining Endpoint")] string Model_URI,
            [Metadata("Scoring Web Service URL", "This is the new endpoint's Patch URL which you can get from Azure Portal's web service Dashboard. It is also returned when you call the AddEndpoint method to create the endpoint using the APIs")] string WebService_URL,
            [Metadata("Scoring Web Service Key", "This is the API Key of the new endpoint which you can get from Azure Portal's web service Dashboard")] string WebService_Key,
            [Metadata("Resource Name", "Saved Trained Model Name e.g. MyTrainedModel [trained model]")] string Resource_Name,
            [Metadata("Evaluate Model Output Path", "The URL of the output of the Evaluate model. You can get this value from the output of the BES Connector call")] string Evaluate_Output_Path = "",
            [Metadata("Evaluation Result Key", "The name of the parameter from the Evaluate Module result. Use the Visualize option of the module in the experiment to get the list of available keys to use here.")] string Evaluate_Key = "",
            [Metadata("Evaluation Condition", "Use to set the condition for the threshold for retraining.")] string Compare = "",
            [Metadata("Evaluation Value", "The threshold value of the Evaluation Result Key.")] double Evaluate_Condition = 0
            )
        {
            bool passEvaluate = CheckEvaluate(Evaluate_Output_Path, Evaluate_Key, Compare, Evaluate_Condition);
            ResponeObject Robj = new ResponeObject();
            if (passEvaluate)
            {
                HttpResponseMessage result = await OverwriteModel(Model_URI, WebService_URL, WebService_Key, Resource_Name);
                //return Request.CreateResponse(result.StatusCode, )

                Robj.HttpStatusCode = result.StatusCode;
                if (result.IsSuccessStatusCode)
                {
                    Robj.Description = "Successfully updated the model";
                    return Request.CreateResponse<ResponeObject>(HttpStatusCode.OK, Robj);
                }
                else
                {
                    Robj.Description = "UnSuccessful updated the model";
                    return Request.CreateResponse<ResponeObject>(HttpStatusCode.InternalServerError, Robj);
                }

            }

            Robj.HttpStatusCode = HttpStatusCode.NotAcceptable;
            Robj.Description = "The condition is not acceptable";
            return Request.CreateResponse<ResponeObject>(HttpStatusCode.NotAcceptable, Robj);
        }


        private bool CheckEvaluate(string Evaluate_Output_Path, string Evaluate_Key, string compare, double Evaluate_Condition)
        {
            bool defaultResult = true;
            try
            {
                if (string.IsNullOrEmpty(Evaluate_Output_Path)) return defaultResult;
                string evaluateResult = ReadBlobFile(Evaluate_Output_Path);

                string[] lines = evaluateResult.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                // Check if result has content
                if (lines.Length == 0) return defaultResult;

                if (lines.Length == 1) return CompareEvaluate(Convert.ToDouble(lines[0]), compare, Evaluate_Condition);//Convert.ToDouble(lines[0]) < Evaluate_Condition;

                // Seperate output by comma
                string[] keys = lines[0].Split(',');
                string[] values = lines[1].Split(',');

                // Check match number of column and value
                // if not match, return false

                if (keys.Length != values.Length)
                    return false;

                // Find Evaluate Key. If found, compare value
                // if and only if value lower than Evaluate Condition, set result is False
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i].ToLower() == Evaluate_Key.ToLower())
                    {
                        return CompareEvaluate(Convert.ToDouble(values[i]), compare, Evaluate_Condition);
                        //if (Convert.ToDouble(values[i]) < Evaluate_Condition)
                        //    return false;
                        //else return true;
                    }
                }
                return false;
            }
            catch (Exception) { return defaultResult; };
        }

        private bool CompareEvaluate(double Value, string compare, double condition)
        {
            switch (compare)
            {
                case "<":
                    return Value < condition;
                case "<=":
                    return Value <= condition;
                case ">":
                    return Value > condition;
                case ">=":
                    return Value >= condition;
                case "=":
                    return Value == condition;
            }
            return true;
        }

        private string ReadBlobFile(string fullURL)
        {
            var webRequest = WebRequest.Create(fullURL);

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                var strContent = reader.ReadToEnd();
                return strContent;
            }
        }

        private async Task<HttpResponseMessage> OverwriteModel(string iLearnerPath, string endpointUrl, string apiKey, string resourceName)
        {
            AzureBlobDataReference InputLocation = new AzureBlobDataReference
            {
                FullURL = iLearnerPath
            };
            InputLocation.ParseFullURL();

            var resourceLocations = new
            {
                Resources = new[]
                {
                    new
                    {
                        Name = resourceName,
                        Location = InputLocation
                        //Location = new AzureBlobDataReference()
                        //{
                        //    BaseLocation = "https://esintussouthsus.blob.core.windows.net/",
                        //    RelativeLocation = "your endpoint relative location", //from the output, for example: “experimentoutput/8946abfd-79d6-4438-89a9-3e5d109183/8946abfd-79d6-4438-89a9-3e5d109183.ilearner”
                        //    SasBlobToken = "your endpoint SAS blob token" //from the output, for example: “?sv=2013-08-15&sr=c&sig=37lTTfngRwxCcf94%3D&st=2015-01-30T22%3A53%3A06Z&se=2015-01-31T22%3A58%3A06Z&sp=rl”
                        //}
                    }
                }
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpointUrl))
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(resourceLocations), System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.SendAsync(request);

                    //if (!response.IsSuccessStatusCode)
                    //{
                    //    //await WriteFailedResponse(response);
                    //}

                    return response;
                }
            }
        }

        private Task WriteFailedResponse(HttpResponseMessage response)
        {
            throw new NotImplementedException();
        }

        public class AzureBlobDataReference
        {
            // Storage connection string used for regular blobs. It has the following format:
            // DefaultEndpointsProtocol=https;AccountName=ACCOUNT_NAME;AccountKey=ACCOUNT_KEY
            // It's not used for shared access signature blobs.
            public string ConnectionString { get; set; }

            // Relative uri for the blob, used for regular blobs as well as shared access 
            // signature blobs.
            public string RelativeLocation { get; set; }

            // Base url, only used for shared access signature blobs.
            public string BaseLocation { get; set; }

            // Shared access signature, only used for shared access signature blobs.
            public string SasBlobToken { get; set; }

            // Show full link of output blob
            public string FullURL { get; set; }

            /// <summary>
            /// Create full link for Blob output 
            /// </summary>
            public void SetFullURL()
            {
                FullURL = BaseLocation + RelativeLocation + SasBlobToken;
            }

            public void ParseFullURL()
            {
                if (FullURL.Length < 9) return;
                int id1 = FullURL.IndexOf('/', 8);
                int id2 = FullURL.IndexOf('?');

                if (id1 == -1 || id2 == -1)
                    return;
                BaseLocation = FullURL.Substring(0, id1 + 1);
                RelativeLocation = FullURL.Substring(id1 + 1, id2 - id1 - 1);
                SasBlobToken = FullURL.Substring(id2);

            }
        }
    }
}
