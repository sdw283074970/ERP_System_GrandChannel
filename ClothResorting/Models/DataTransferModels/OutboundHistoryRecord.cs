using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.DataTransferModels
{
    public class OutboundHistoryRecord
    {
        public DateTime OutboundDate { get; set; }

        public string OutboundPcs { get; set; }

        public string FromLocation { get; set; }

        public string OrderPurchaseOrder { get; set; }
    }
}