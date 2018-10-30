using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.QBOModels
{
    public class InvoiceCreateRequestBody
    {
        public ICollection<Line> Line { get; set; }

        public CustomerRef CustomerRef { get; set; }
    }

    public class Line
    {
        public double Amount { get; set; }

        public string DetailType { get; set; }

        public SalesItemLineDetail SalesItemLineDetail { get; set; }
    }

    public class SalesItemLineDetail
    {
        public ItemRef ItemRef { get; set; }

        public double UnitPrice { get; set; }

        public double Qty { get; set; }
    }

    public class ItemRef
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class CustomerRef
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}