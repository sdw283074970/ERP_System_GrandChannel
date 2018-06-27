using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class AdjustmentRecord
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public string Adjustment { get; set; }

        public DateTime AdjustDate { get; set; }

        public SpeciesInventory SpeciesInventory { get; set; }
    }
}