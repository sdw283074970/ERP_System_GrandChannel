using ClothResorting.Models.FBAModels.BaseClass;
using ClothResorting.Models.FBAModels.StaticModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAPallet : BaseFBAOrderDetail
    {
        public FBAPallet()
        {
            LocationStatus = FBAStatus.Original;
        }

        public string LocationStatus { get; set; }

        public string PalletSize { get; set; }

        public bool DoesAppliedLabel { get; set; }

        public bool HasSortingMarking { get; set; }

        public bool IsOverSizeOrOverwidth { get; set; }

        public int ActualPallets { get; set; }

        public int ComsumedPallets { get; set; }

        public string Memo { get; set; }

        public FBAMasterOrder FBAMasterOrder { get; set; }

        public ICollection<FBACartonLocation> FBACartonLocations { get; set; }

        public ICollection<FBAPalletLocation> FBAPalletLocations { get; set; }

        public void AssembleBoolValue(bool doesAppliedLabel, bool hasSortingMarking, bool isOversizeOrOverwidth)
        {
            DoesAppliedLabel = doesAppliedLabel;
            HasSortingMarking = hasSortingMarking;
            IsOverSizeOrOverwidth = isOversizeOrOverwidth;
        }
    }
}