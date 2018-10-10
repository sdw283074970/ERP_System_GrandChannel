using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class InvoiceDetail
    {
        public int Id { get; set; }

        public string Activity { get; set; }

        public string ChargingType { get; set; }

        public string Unit { get; set; }

        public string Rate { get; set; }

        public string Amount { get; set; }

        public Invoice Invoice { get; set; }
    }
}