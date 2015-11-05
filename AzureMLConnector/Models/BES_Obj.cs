using AzureMLConnector.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace AzureMLConnector.Models
{
    public abstract class BES_Obj
    {
        [Metadata("API POST URL", "Web Service Request URI")]
        public string API_URL { get; set; }

        [Metadata("API Key", "Web Service API Key")]
        public string API_Key { get; set; }

        [Metadata(FriendlyName = "Global Parameters Keys", Description = "Comma separated list of parameters", Visibility = VisibilityType.Advanced)]
        public string GlobalKeys { get; set; }

        [Metadata(FriendlyName = "Global Parameters Values", Description = "Comma separated list of values", Visibility = VisibilityType.Advanced)]
        public string GlobalValues { get; set; }

        public string GetAPIURL()
        {
            return API_URL;
        }

        public string GetApiVersion()
        {
            if (API_URL.IndexOf("api-version") == -1)
                return "";
            return API_URL.Substring(API_URL.IndexOf("api-version"));
        }

        public string GetAPIKey()
        {
            return API_Key;
        }
        abstract public AzureBlobDataReference GenerateInput();
        abstract public Dictionary<string, AzureBlobDataReference> GenerateOutputs();

        public Dictionary<string, string> GenerateGlobalParameters()
        {
            Dictionary<string, string> _globalParam = new Dictionary<string, string>();
            if(string.IsNullOrEmpty(GlobalKeys) || string.IsNullOrEmpty(GlobalValues))
                return _globalParam;
            
            string[] keys = GlobalKeys.Split(',').Select(x => x.Trim()).ToArray();
            string[] values = GlobalValues.Split(',').Select(x => x.Trim()).ToArray();

            if (keys.Length == values.Length)
                for (int i = 0; i < values.Length; i++ )
                    _globalParam.Add(keys[i], values[i]);
            return _globalParam;
        }
    }
}