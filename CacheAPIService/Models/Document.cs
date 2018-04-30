using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CacheAPIService.Models
{
    /**
     * Domain class for documents.
     */
    public class Document
    {
        public int ID { get; set; }
        public String Message { get; set; }
    }
}