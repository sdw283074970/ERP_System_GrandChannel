using ClothResorting.Models.FBAModels.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class FBACartonLocationDto : BaseFBAOrderDetail
    {
        public float GrossWeightPerCtn { get; set; }

        public float CBMPerCtn { get; set; }

        public int ctnsPerPlt { get; set; }

        public int AvaliableCtns { get; set; }

        public int PickingCtns { get; set; }

        public int ShippedCtns { get; set; }

        public string Location { get; set; }
    }
}