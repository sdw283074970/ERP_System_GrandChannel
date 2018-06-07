using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class SilkIconCartonDetail : CartonDetail
    {
        public string PurchaseOrderNumber { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public int? CartonNumberRangeFrom { get; set; }

        public int? CartonNumberRangeTo { get; set; }

        public int? SumOfCarton { get; set; }

        public int? ActualReceived { get; set; }

        public int? Available { get; set; }

        public string RunCode { get; set; }

        public string Dimension { get; set; }

        public double? GrossWeightPerCartons { get; set; }

        public double? NetWeightPerCartons { get; set; }

        public int? PcsPerCartons { get; set; }

        public int? TotalPcs { get; set; }

        public int? ActualReceivedPcs { get; set; }

        public int? AvailablePcs { get; set; }

        public string Location { get; set; }

        public IList<SizeRatio> SizeRatios { get; set; }

        public ICollection<CartonBreakDown> CartonBreakdowns { get; set; }

        public SilkIconPackingList SilkIconPackingList { get; set; }
    }
}