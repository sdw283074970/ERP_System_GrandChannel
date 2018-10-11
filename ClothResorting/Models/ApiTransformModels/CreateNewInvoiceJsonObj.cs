using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class CreateNewInvoiceJsonObj
    {
        public string Vendor { get; set; }

        public string DepartmentCode { get; set; }

        public string InvoiceNumber { get; set; }

        public string PurchaseOrder { get; set; }

        public string InvoiceDate { get; set; }

        public string DueDate { get; set; }

        public string Enclosed { get; set; }

        public string  ShipDate { get; set; }

        public string ShipVia { get; set; }

        public string BillTo { get; set; }

        public string ShipTo { get; set; }

        public string Currency { get; set; }

        public string Terms { get; set; }
    }
}