using ClothResorting.Models.FBAModels.BaseClass;
using ClothResorting.Models.FBAModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBACartonLocation : BaseFBAOrderDetail, IFBALocation
    {
        public float GrossWeightPerCtn { get; set; }

        public float CBMPerCtn { get; set; }

        public int CtnsPerPlt { get; set; }

        public string UPCNumber { get; set; }

        public int AvailableCtns { get; set; }

        public int PickingCtns { get; set; }

        public int ShippedCtns { get; set; }

        public string Location { get; set; }

        public FBAOrderDetail FBAOrderDetail { get; set; }

        public FBAPallet FBAPallet { get; set; }

        public ICollection<FBAPickDetail> FBAPickDetails { get; set; }

        public ICollection<FBAPickDetailCarton> FBAPickDetailCartons { get; set; }

        public void AssemblePltInfo(float grossWeightPerCtn, float cbmPerCtn, int ctnsPerPlt)
        {
            GrossWeightPerCtn = grossWeightPerCtn;
            CBMPerCtn = cbmPerCtn;
            CtnsPerPlt = ctnsPerPlt;
        }
    }
}