using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class CartonBreakDownDto
    {
        public int Id { get; set; }

        public string PurchaseNumber { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public int? CartonNumberRangeFrom { get; set; }

        public int? CartonNumberRangeTo { get; set; }

        public string RunCode { get; set; }

        public string Size { get; set; }

        public int? ForecastPcs { get; set; }

        public int? ActualPcs { get; set; }

        public int? AvailablePcs { get; set; }

        public string Location { get; set; }
    }
}