using ClothResorting.Models.FBAModels.BaseClass;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAPalletLocation : BaseFBAOrderDetail
    {
        public float GrossWeightPerPlt { get; set; }

        public float CBMPerPlt { get; set; }

        public int CtnsPerPlt { get; set; }

        public int OriginalPlts { get; set; }

        public int AvailablePlts { get; set; }

        public int PickingPlts { get; set; }

        public int ShippedPlts { get; set; }

        public string Location { get; set; }

        public FBAPallet FBAPallet { get; set; }

        public FBAPalletLocation()
        {
            Location = FBAStatus.Unassigned;
        }

        public void AssemblePltDetails(float grossWeightPerPlt, float cbmPerPlt, int ctnsPerPlt)
        {
            GrossWeightPerPlt = grossWeightPerPlt;
            CBMPerPlt = cbmPerPlt;
            CtnsPerPlt = ctnsPerPlt;
        }
    }
}