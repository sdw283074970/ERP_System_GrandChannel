using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class RegularCartonDetailDto
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string Container { get; set; }

        public string Style { get; set; }

        public string Customer { get; set; }

        public string CartonRange { get; set; }

        public string Dimension { get; set; }

        public double GrossWeight { get; set; }

        public double NetWeight { get; set; }

        public string Color { get; set; }

        public int Cartons { get; set; }

        public int ActualCtns { get; set; }

        public string SizeBundle { get; set; }

        public string PcsBundle { get; set; }

        public int PcsPerCarton { get; set; }

        public int Quantity { get; set; }

        public int ActualPcs { get; set; }

        public int ToBeAllocatedCtns { get; set; }

        public int ToBeAllocatedPcs { get; set; }

        public string Status { get; set; }

        public string OrderType { get; set; }

        public string Comment { get; set; }

        public string Receiver { get; set; }

        public string Adjustor { get; set; }

        public string Operator { get; set; }

        public string Vendor { get; set; }

        public string SKU { get; set; }

        public string ColorCode { get; set; }

        public DateTime InboundDate { get; set; }

    }
}