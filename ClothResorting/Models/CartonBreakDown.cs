using ClothResorting.Models.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class CartonBreakDown : ICartonBreakDown
    {
        public int Id { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public int? CartonNumberRangeFrom { get; set; }

        public int? CartonNumberRangeTo { get; set; }

        public string RunCode { get; set; }

        public string Size { get; set; }

        public int? PcsPerCartons { get; set; }

        public int? ForecastPcs { get; set; }

        public int? ActualPcs { get; set; }

        public int? AvailablePcs { get; set; }

        public string Location { get; set; }
        
        public PackingList PackingList { get; set; }

        public CartonDetail CartonDetail { get; set; }

        public ICollection<CartonBreakdownOutbound> CartonBreakdownOutbounds { get; set; }
    }
}