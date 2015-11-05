using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace AzureMLConnector.Models
{
    public class ResponeObject
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public string Description { get; set; }
    }
}