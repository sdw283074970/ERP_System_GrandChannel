using ClothResorting.Models.FBAModels.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBACartonLocation : BaseFBAOrderDetail
    {
        public int AvaliableCtns { get; set; }

        public int PickingCtns { get; set; }

        public int ShippedCtns { get; set; }

        public string Location { get; set; }

        public FBAOrderDetail FBAOrderDetail { get; set; }

        public FBAPallet FBAPallet { get; set; }
    }
}