using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class RegularSpeciesInventory
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string SizeBundle { get; set; }

        public int PcsBundle { get; set; }

        public int InvCartons { get; set; }

        public int PcsPerCarton { get; set; }

        public int InvPcs { get; set; }

        public PurchaseOrderInventory PurchaseOrderInventory { get; set; }
    }
}