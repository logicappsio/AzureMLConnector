using AzureMLConnector.Controllers;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace AzureMLConnector.Models
{    
    public class BES_Input : BES_Obj
    {      
        [Metadata("Storage Account Name (Input)", "Azure Storage Account Name")]
        public string Input_AccountName { get; set; }

        [Metadata("Storage Account Key (Input)", "Azure Storage Account Key")]
        public string Input_AccountKey { get; set; }

        [Metadata("Storage Container Name (Input)", "Azure Storage Container Name")]
        public string Input_Container { get; set; }

        [Metadata("Blob Name (Input)", "Azure Storage Blob Name")]
        public string Input_Blob { get; set; }

        
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
            return new Dictionary<string, AzureBlobDataReference>();
        }
    }
}