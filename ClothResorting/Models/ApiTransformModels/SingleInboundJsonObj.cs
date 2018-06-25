using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class SingleInboundJsonObj
    {
        public int Id { get; set; }

        public int PreId { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public int Ctns { get; set; }

        public int Quantity { get; set; }

        public string Location { get; set; }
    }
}