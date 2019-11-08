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

        public string Barcode { get; set; }

        public string Symbology { get; set; }

        public DateTime InboundDate { get; set; }

        public int ctnsPerPlt { get; set; }

        public int AvailableCtns { get; set; }

        public int PickingCtns { get; set; }

        public int ShippedCtns { get; set; }

        public int HoldCtns { get; set; }

        public string Location { get; set; }

        public int SelectedCtns { get; set; }
    }
}