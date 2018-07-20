using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class PickingList
    {
        public int Id { get; set; }

        public string OrderPurchaseOrder { get; set; }

        public DateTime? CreateDate { get; set; }

        public string PickTicketsRange { get; set; }

        public string Status { get; set; }

        public PreReceiveOrder PreReceiveOrder { get; set; }

        public ICollection<PickingRecord> PickingRecords { get; set; }
    }
}