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

        public string Vendor { get; set; }

        public string OrderType { get; set; }
        
        public string DepartmentCode { get; set; }
    }
}