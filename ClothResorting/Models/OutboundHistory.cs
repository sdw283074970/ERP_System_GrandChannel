using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class OutboundHistory
    {
        public int Id { get; set; }

        public int OutboundPcs { get; set; }

        public DateTime OutboundDate { get; set; }

        public string FromLocation { get; set; }

        public string OrderPurchaseOrder { get; set; }

        public SpeciesInventory SpeciesInventory { get; set; }
    }
}