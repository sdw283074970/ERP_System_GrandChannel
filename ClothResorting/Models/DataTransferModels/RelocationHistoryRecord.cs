using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.DataTransferModels
{
    public class RelocationHistoryRecord
    {
        public DateTime RelocatedDate { get; set; }

        public string RelocatedPcs { get; set; }

        public string FromLocation { get; set; }

        public string ToLocation { get; set; }
    }
}