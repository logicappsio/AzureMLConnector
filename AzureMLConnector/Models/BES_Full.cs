using AzureMLConnector.Controllers;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace AzureMLConnector.Models
{
    public class BES_Full : BES_Obj
    {
        [Metadata("Storage Account Name (Input)", "Azure Storage Account Name")]
        public string Input_AccountName { get; set; }

        [Metadata("Storage Account Key (Input)", "Azure Storage Account Key")]
        public string Input_AccountKey { get; set; }

        [Metadata("Storage Container Name (Input)", "Azure Storage Container Name")]
        public string Input_Container { get; set; }

        [Metadata("Blob Name (Input)", "Azure Storage Blob Name")]
        public string Input_Blob { get; set; }
               
        [Metadata("Storage Account Name (Output)", "Azure Storage Account Name. Leave blank if same with Input")]
        public string Output_AccountName { get; set; }

        [Metadata("Storage Account Key (Output)", "Azure Storage Account Key. Leave blank if same with Input")]
        public string Output_AccountKey { get; set; }

        [Metadata("Storage Container Name (Output)", "Azure Storage Container Name. Leave blank if same with Input")]
        public string Output_Container { get; set; }

        [Metadata("Blob Name (Output)", "Azure Storage Blob Name. Include file extention")]
        public string Output_Blob { get; set; }

        public override AzureBlobDataReference GenerateInput()
        {
            try
            {
                string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", Input_AccountName, Input_AccountKey);
                var blobClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(Input_Container);
                container.CreateIfNotExists();
                var blob = container.GetBlockBlobReference(Input_Blob);

                var Input = new AzureBlobDataReference()
                {
                    ConnectionString = storageConnectionString,
                    RelativeLocation = blob.Uri.LocalPath
                };
                return Input;
            }
            catch (Exception) { return new AzureBlobDataReference(); }
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