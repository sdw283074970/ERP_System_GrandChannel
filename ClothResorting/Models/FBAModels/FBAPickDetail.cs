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

        public string Barcode { get; set; }

        public string Symbology { get; set; }

        public int PickableCtns { get; set; }

        public string Location { get; set; }

        public int ActualPlts { get; set; }

        public DateTime InboundDate { get; set; }

        public string OrderType { get; set; }

        public int PltsFromInventory { get; set; }

        public int NewPlts { get; set; }

        public FBAShipOrder FBAShipOrder { get; set; }

        public FBAPalletLocation FBAPalletLocation { get; set; }

        public FBACartonLocation FBACartonLocation { get; set; }

        public ICollection<FBAPickDetailCarton> FBAPickDetailCartons { get; set; }

        public FBAPickDetail()
        {
            InboundDate = new DateTime(1900, 1, 1, 0, 0, 0, 0);
        }
    }
}