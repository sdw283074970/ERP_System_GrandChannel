using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class BulkPOSummaryJsonObj
    {
        public int PreId { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string OrderType { get; set; }

        public int POLine { get; set; }

        public string Customer { get; set; }

        public string Vendor { get; set; }

    }
}