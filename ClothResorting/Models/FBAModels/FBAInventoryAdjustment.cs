using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAInventoryAdjustment
    {
        public int Id { get; set; }

        public string AdjustmentType {get;set;}

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public string Change { get; set; }

        public string Description { get; set; }

        public ICollection<FBAPalletLocation> FBAPalletLocations { get; set; }

        public ICollection<FBACartonLocation> FBACartonLocations { get; set; }
    }
}