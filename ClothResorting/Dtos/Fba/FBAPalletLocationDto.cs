using ClothResorting.Models.FBAModels.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class FBAPalletLocationDto : BaseFBAOrderDetail
    {
        public string PalletSize { get; set; }

        public float GrossWeightPerPlt { get; set; }

        public DateTime InboundDate { get; set; }

        public float CBMPerPlt { get; set; }

        public int CtnsPerPlt { get; set; }

        public int ActualPlts { get; set; }

        public int AvailablePlts { get; set; }

        public int PickingPlts { get; set; }

        public int ShippedPlts { get; set; }

        public string Location { get; set; }

        public int OriginalTotalCtns { get; set; }

        public int CurrentAvailableCtns { get; set; }

        public int SelectedPlts { get; set; }

        public int NewPlts { get; set; }

        public IEnumerable<FBACartonLocationDto> FBACartonLocations { get; set; }
    }
}