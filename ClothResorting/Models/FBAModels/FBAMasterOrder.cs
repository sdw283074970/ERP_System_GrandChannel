using ClothResorting.Models.FBAModels.BaseClass;
using ClothResorting.Models.FBAModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAMasterOrder : BaseFBAMasterOrder
    {
        public int Id { get; set; }

        public string GrandNumber { get; set; }

        public string TotalCBM { get; set; }

        public string TotalPlts { get; set; }

        public string TotalCtns { get; set; }

        public DateTime InboundDate { get; set; }

        string Status { get; set; }

        public UpperVendor Customer { get; set; }
    }
}