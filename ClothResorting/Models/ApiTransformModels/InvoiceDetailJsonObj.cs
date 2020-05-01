using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class InvoiceDetailJsonObj
    {
        public int invoiceId { get; set; }

        public string Activity { get; set; }

        public float Discount { get; set; }

        public string ChargingType { get; set; }

        public string Unit { get; set; }

        public double Rate { get; set; }

        public double Amount { get; set; }  // final amount

        public double OriginalAmount { get; set; }

        public double Cost { get; set; }

        public DateTime DateOfCost { get; set; }

        public double Quantity { get; set; }

        public string Memo { get; set; }
    }
}