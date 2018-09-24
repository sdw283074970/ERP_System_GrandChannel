using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class PurchaseOrderInventoryDto
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string OrderType { get; set; }

        public string Vender { get; set; }

        public int AvailablePcs { get; set; }

        public int AvailableCtns { get; set; }

        public int PickingPcs { get; set; }

        public int ShippedPcs { get; set; }
    }
}