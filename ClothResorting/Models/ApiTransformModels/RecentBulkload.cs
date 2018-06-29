using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class RecentBulkload : BasicFourAttrsJsonObj
    {
        public int NumberOfCartons { get; set; }

        public int Pcs { get; set; }

        public string Location { get; set; }

        public DateTime InboundDate { get; set; }
    }
}