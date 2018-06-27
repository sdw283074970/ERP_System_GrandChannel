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

        public int InvPcs { get; set; }

        public PurchaseOrderInventory PurchaseOrderInventory { get; set; }
    }
}