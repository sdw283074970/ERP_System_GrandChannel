using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.QBOModels
{
    public class CustomerResponseBody
    {
        public CustomerQueryResponse QueryResponse { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }
    }

    public class CustomerQueryResponse
    {
        public ICollection<Customer> Customer { get; set; }

        [JsonProperty("startPosition")]
        public int StartPosition { get; set; }

        [JsonProperty("maxResults")]
        public int MaxResults { get; set; }
    }

    public class Customer
    {
        public string Id { get; set; }
    }
}