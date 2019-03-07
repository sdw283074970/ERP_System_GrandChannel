using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class InvoiceDetailDto
    {
        public int Id { get; set; }

        public string Activity { get; set; }

        public string InvoiceType { get; set; }

        public string ChargingType { get; set; }

        public string Unit { get; set; }

        public double Quantity { get; set; }

        public double Rate { get; set; }

        public double Amount { get; set; }

        public DateTime DateOfCost { get; set; }

        public string Memo { get; set; }
    }
}