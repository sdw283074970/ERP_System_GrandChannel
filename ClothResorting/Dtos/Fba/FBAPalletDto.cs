using ClothResorting.Models.FBAModels.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class FBAPalletDto : BaseFBAOrderDetail
    {
        public string PalletSize { get; set; }

        public bool DoesAppliedLabel { get; set; }

        public bool HasSortingMarking { get; set; }

        public bool IsOverSizeOrOverwidth { get; set; }

        public int ActualPallets { get; set; }

        public int ComsumedPallets { get; set; }

        public IEnumerable<FBACartonLocationDto> FBACartonLocations { get; set; }
    }
}