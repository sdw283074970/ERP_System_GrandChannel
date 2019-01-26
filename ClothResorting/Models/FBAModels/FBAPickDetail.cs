using ClothResorting.Models.FBAModels.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAPickDetail : BaseFBAOrderDetail
    {
        public string Size { get; set; }

        public int CtnsPerPlt { get; set; }

        public string Location { get; set; }

        public int ActualPlts { get; set; }

        public string OrderType { get; set; }

        public FBAShipOrder FBAShipOrder { get; set; }

        public FBAPalletLocation FBAPalletLocation { get; set; }

        public FBACartonLocation FBACartonLocation { get; set; }

        public ICollection<FBAPickDetailCarton> FBAPickDetailCartons { get; set; }
    }
}