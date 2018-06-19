using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class PurchaseOrderDetail
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public string ForecastPcs { get; set; }

        public int ActualPcs { get; set; }

        public int AvailablePcs { get; set; }

        public PackingList PackingList { get; set; }

        public ICollection<LocationDetail> LocationDetails { get; set; }
    }
}