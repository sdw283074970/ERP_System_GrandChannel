using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class BasicFourAttrsJsonObj
    {
        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }
    }
}