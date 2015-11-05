using AzureMLConnector.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace AzureMLConnector.Models
{
    public class BES_Output : BES_Obj
    {
        [Metadata("Storage Account Name (Output)", "Azure Storage Account Name")]
        public string Output_AccountName { get; set; }

        [Metadata("Storage Account Key (Output)", "Azure Storage Account Key")]
        public string Output_AccountKey { get; set; }

        [Metadata("Storage Container Name (Output)", "Azure Storage Container Name")]
        public string Output_Container { get; set; }

        [Metadata("Blob Name (Output)", "Azure Storage Blob Name. Include file extention")]
        public string Output_Blob { get; set; }

        public override AzureBlobDataReference GenerateInput()
        {
            return new AzureBlobDataReference();
        }

        public override Dictionary<string, AzureBlobDataReference> GenerateOutputs()
        {
            try
            {
                string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", Output_AccountName, Output_AccountKey);

                var Outputs = new Dictionary<string, AzureBlobDataReference>()
                    {
                        { 
                            "output1",
                            new AzureBlobDataReference()
                            {
                                ConnectionString = storageConnectionString,
                                RelativeLocation = string.Format("/{0}/{1}", Output_Container, Output_Blob)
                            }
                        },
                    };
                return Outputs;
            }
            catch (Exception) { return new Dictionary<string, AzureBlobDataReference>(); }
        }
        
    }
}