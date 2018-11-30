using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class SpeciesInventory
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public int OrgPcs { get; set; }

        public int AdjPcs { get; set; }

        public int AvailablePcs { get; set; }

        public int PickingPcs { get; set; }

        public int ShippedPcs { get; set; }

        public string Vendor { get; set; }

        public PurchaseOrderInventory PurchaseOrderInventory { get; set; }

        public ICollection<AdjustmentRecord> AdjustmentRecords { get; set; }

        public ICollection<ReplenishmentLocationDetail> ReplenishmentLocationDetail { get; set; }

        public ICollection<OutboundHistory> OutboundHistory { get; set; }
    }
}