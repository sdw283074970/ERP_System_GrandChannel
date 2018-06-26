using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class PurchaseOrderInventory
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string OrderType { get; set; }

        public string Vender { get; set; }

        public int InvPcs { get; set; }

        public int InvCtns { get; set; }

        public ICollection<LocationDetail> LocationDetails { get; set; }

        public ICollection<SpeciesInventory> SpeciesInventories { get; set; }
    }
}