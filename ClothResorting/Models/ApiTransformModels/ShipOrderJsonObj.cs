﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class ShipOrderJsonObj
    {
        public string OrderPurchaseOrder { get; set; }

        public string Customer { get; set; }

        public string Address_1 { get; set; }

        public string Address_2 { get; set; }

        public string PickTicketsRange { get; set; }
    }
}