using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class PreReceiveOrdersDto
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public string ContainerNumber { get; set; }

        public DateTime? CreatDate { get; set; }

        public int? TotalCartons { get; set; }

        public double? TotalGrossWeight { get; set; }

        public double? TotalNetWeight { get; set; }

        public double? TotalVol { get; set; }

        public int? ActualReceived { get; set; }

        public int? Available { get; set; }

        public string Status { get; set; }

        public int? TotalPcs { get; set; }

        public int? ActualReceivedPcs { get; set; }

        public int? AvailablePcs { get; set; }

        public int InvPcs { get; set; }

        public ICollection<PurchaseOrderSummaryDto> SilkIconPackingLists { get; set; }
    }
}