using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class PickingRecord
    {
        public int Id { get; set; }

        public string Container { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string CustomerCode { get; set; }

        public string SizeBundle { get; set; }

        public string PcsBundle { get; set; }

        public int Cartons { get; set; }

        public int Quantity { get; set; }

        public int PcsPerCaron { get; set; }

        public string Location { get; set; }

        public DateTime? PickingDate { get; set; }

        public PickingList PickingList { get; set; }
    }
}