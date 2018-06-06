using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class PurchaseOrderReceivedCartons
    {
        public string PurchaseOrder { get; set; }
        public int ReceivedCartons { get; set; }
    }
}