using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class PreReceiveOrder
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public DateTime? CreatDate { get; set; }

        public int? TotalCartons { get; set; }

        public double? TotalGrossWeight { get; set; }

        public double? TotalNetWeight { get; set; }

        public double? TotalVol { get; set; }

        public int? ActualReceivedCtns { get; set; }

        public string ContainerNumber { get; set; }

        public int? TotalPcs { get; set; }

        public int? ActualReceivedPcs { get; set; }

        public string Status { get; set; }

        public string Operator { get; set; }

        public string WorkOrderType { get; set; }

        public ICollection<PurchaseOrderSummary> PurchaseOrderSummaries { get; set; }

        public ICollection<POSummary> POSummaries { get; set; }

        public ICollection<FCRegularLocationDetail> FCRegularLocationDetails { get; set; }
    }
}