using System;
using System.Collections.Generic;

namespace ClothResorting.Dtos
{
    public class PurchaseOrderSummaryDto
    {
        public string Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string OrderType { get; set; }

        public string PurchaseOrder_StyleNumber { get; set; }

        public DateTime? Date { get; set; }

        public double? NetWeight { get; set; }

        public double? GrossWeight { get; set; }

        public double? CFT { get; set; }

        public int? PackedCartons { get; set; }

        public int? NumberOfSizeRatio { get; set; }

        public int? NumberOfDemension { get; set; }

        public int? ActualReceived { get; set; }

        public int? Available { get; set; }

        public int? InventoryCtn { get; set; }

        public int? TotalPcs { get; set; }

        public int? ActualReceivedPcs { get; set; }

        public int? AvailablePcs { get; set; }

        public int? InventoryPcs { get; set; }

        public IList<MeasurementDto> TotalMeasurements { get; set; }

        public ICollection<CartonDetailDto> SilkIconCartonDetails { get; set; }
    }
}