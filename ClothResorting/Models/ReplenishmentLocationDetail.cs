using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class ReplenishmentLocationDetail
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public int Cartons { get; set; }

        public int AvailableCtns { get; set; }

        public int PickingCtns { get; set; }

        public int ShippedCtns { get; set; }

        public int Quantity { get; set; }

        public int AvailablePcs { get; set; }

        public int PickingPcs { get; set; }

        public int ShippedPcs { get; set; }

        public string Location { get; set; }

        public DateTime InboundDate { get; set; }

        public PurchaseOrderInventory PurchaseOrderInventory { get; set; }

        public GeneralLocationSummary GeneralLocationSummary { get; set; }
    }
}