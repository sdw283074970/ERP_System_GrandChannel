using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class InventoryResult
    {
        public string Location { get; set; }

        public string PurchaseOrderNumber { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public int? TargetPcs { get; set; }

        public int? NumberOfCartons { get; set; }
    }
}