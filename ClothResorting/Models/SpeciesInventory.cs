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

        public int Quantity { get; set; }

        public PurchaseOrderInventory PurchaseOrderInventory { get; set; }
    }
}