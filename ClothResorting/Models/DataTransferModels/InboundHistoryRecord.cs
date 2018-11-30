using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.DataTransferModels
{
    public class InboundHistoryRecord
    {
        public string FileName { get; set; }

        public string InboundPcs { get; set; }

        public int ResidualPcs { get; set; }

        public string Location { get; set; }
    }
}