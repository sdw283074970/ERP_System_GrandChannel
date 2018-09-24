using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class SpeciesInventoryDto
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
    }
}