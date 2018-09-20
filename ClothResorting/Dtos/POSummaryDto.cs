using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class POSummaryDto
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public int PoLine { get; set; }

        public string Customer { get; set; }

        public int Quantity { get; set; }

        public int ActualPcs { get; set; }

        public int Cartons { get; set; }

        public int ActualCtns { get; set; }

        public double CBM { get; set; }

        public double GrossWeight { get; set; }

        public double NetWeight { get; set; }

        public double NNetWeight { get; set; }

        public string Container { get; set; }

        public string OrderType { get; set; }

        public string Operator { get; set; }

    }
}