using Jil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.QBOModels
{
    public class ItemResponseBody
    {
        public ItemQueryResponse QueryResponse { get; set; }

        public string Time { get; set; }
    }

    public class ItemQueryResponse
    {
        public ICollection<Item> Item { get; set; }

        public int StartPosition { get; set; }

        public int MaxResults { get; set; }
    }

    public class Item
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Active { get; set; }

        public string FullyQualifiedName { get; set; }

        public bool Taxable { get; set; }

        public double UnitPrice { get; set; }

        public string Type { get; set; }

        public IncomeAccountRef IncomeAccountRef { get; set; }

        public double PurchaseCost { get; set; }

        public bool TrackQtyOnHand { get; set; }

        public string Domain { get; set; }

        public bool Sparse { get; set; }

        public string Id { get; set; }

        public MetaData MetaData { get; set; }
    }

    public class MetaData
    {
        public string CreateTime { get; set; }

        public string LastUpdatedTime { get; set; }
    }

    public class IncomeAccountRef
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}