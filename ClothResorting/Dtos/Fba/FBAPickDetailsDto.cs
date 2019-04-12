using ClothResorting.Models.FBAModels.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class FBAPickDetailsDto : BaseFBAOrderDetail
    {
        public string Size { get; set; }

        public int CtnsPerPlt { get; set; }

        public string Location { get; set; }

        public int ActualPlts { get; set; }

        public int PickableCtns { get; set; }

        public string OrderType { get; set; }

        public int PltsFromInventory { get; set; }

        public int NewPlts { get; set; }
    }
}