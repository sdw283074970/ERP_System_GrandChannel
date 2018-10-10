using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class UpperVendor
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string DepartmentCode { get; set; }

        public ICollection<PreReceiveOrder> WorkOrders { get; set; }

        public ICollection<ChargingItem> ChargingItems { get; set; }

        public ICollection<Invoice> Invoices { get; set; }
    }
}