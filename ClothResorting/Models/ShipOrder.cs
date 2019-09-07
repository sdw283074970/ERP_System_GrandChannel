using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class ShipOrder
    {
        public int Id { get; set; }

        public string OrderPurchaseOrder { get; set; }

        public string Customer { get; set; }

        public string Address { get; set; }

        public string PickTicketsRange { get; set; }

        public string Status { get; set; }

        public string CreateDate { get; set; }

        public string PickDate { get; set; }

        public string PickingMan { get; set; }

        public string Operator { get; set; }

        public string ShippingMan { get; set; }

        public string Vendor { get; set; }

        public string OrderType { get; set; }

        public string DepartmentCode { get; set; }

        public DateTime ShipDate { get; set; }

        public ICollection<PickDetail> PickDetails { get; set; }

        public ICollection<PullSheetDiagnostic> PullSheetDiagnostics { get; set; }

        public ShipOrder()
        {
            ShipDate = new DateTime(1900, 1, 1);
        }
    }
}