using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothResorting.Models
{
	public class SilkIconPackingList : PackingList
	{
		public string PurchaseOrderNumber { get; set; }

		public string StyleNumber { get; set; }

		public int? ActualReceived {get; set; }

		public int? Available { get; set; }

		public string PurchaseOrder_StyleNumber { get; set; }

		public DateTime? ReceivedDate { get; set; }

		public double? NetWeight { get; set; }

		public double? GrossWeight { get; set; }

		public double? CFT { get; set; }

		public int? PackedCartons { get; set; }

		public int? NumberOfSizeRatio { get; set; }

		public int? NumberOfDemension { get; set; }

		public int? TotalPcs { get; set; }

		public int? ActualReceivedPcs { get; set; }

		public int? AvailablePcs { get; set; }

		public IList<CartonBreakDown> CartonBreakDowns { get; set; }

		public IList<Measurement> TotalMeasurements { get; set; }

		public ICollection<SilkIconCartonDetail> SilkIconCartonDetails { get; set; }

		public SilkIconPreReceiveOrder SilkIconPreReceiveOrder { get; set; }
	}
}
