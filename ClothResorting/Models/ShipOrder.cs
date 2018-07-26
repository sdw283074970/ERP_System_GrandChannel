using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class ShipOrder
    {
        public int Id { get; set; }

        public string OrderPurchaseOrder { get; set; }

        public string Customer { get; set; }

        public string Address_1 { get; set; }

        public string Address_2 { get; set; }

        public string ShipDate { get; set; }

        public string PickTicketsRange { get; set; }

        public string Status { get; set; }

        public ICollection<PickingRecord> PickingRecords { get; set; }
    }
}