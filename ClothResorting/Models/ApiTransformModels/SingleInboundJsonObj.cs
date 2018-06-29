using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class SingleInboundJsonObj : BasicFourAttrsJsonObj
    {
        public int Id { get; set; }

        public int Ctns { get; set; }

        public int Quantity { get; set; }

        public string Location { get; set; }
    }
}