using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class PickTiketsRangeJsonObj
    {
        public string OrderPurchaseOrder { get; set; }

        public string Customer { get; set; }

        public string Address { get; set; }

        public string PickTicketsRange { get; set; }
    }
}