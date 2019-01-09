using ClothResorting.Models.FBAModels.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAPallet : BaseFBAOrderDetail
    {
        public string PltSize { get; set; }

        public int OriginalPallets { get; set; }

        public int AvailablePalltes { get; set; }

        public int PickingPallets { get; set; }

        public int ShippedPallets { get; set; }

        //public FBAMasterOrder FBAMasterOrder { get; set; }

        public ICollection<FBACartonLocation> FBACartonLocations { get; set; }
    }
}