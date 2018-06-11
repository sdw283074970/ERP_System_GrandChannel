using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class SearchResult
    {
        public string ContainerNumber { get; set; }

        public string PurchaseOrderNumber { get; set; }

        public string Vender { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public int? CartonNumberRangeFrom { get; set; }

        public int? CartonNumberRangeTo { get; set; }

        public string RunCode { get; set; }

        public string Size { get; set; }

        public int? ReceivedPcs { get; set; }

        public int? AvailablePcs { get; set; }

        public string Location { get; set; }
    }
}