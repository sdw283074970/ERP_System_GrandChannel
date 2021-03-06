﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; }

        public string InvoiceType { get; set; }

        public double TotalDue { get; set; }

        public string BillTo { get; set; }

        public string Enclosed { get; set; }

        public string ShipTo { get; set; }

        public string ShipVia { get; set; }

        public string Currency { get; set; }

        public string PurchaseOrder { get; set; }

        public string InvoiceDate { get; set; }

        public string DueDate { get; set; }

        public string ShipDate { get; set; }

        public string Container { get; set; }

        public string Status { get; set; }

        public string RequestId { get; set; }

        public string CreatedBy { get; set; }

        public string UploadedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UploadedDate { get; set; }

        public UpperVendor UpperVendor { get; set; }

        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }

        public Invoice()
        {
            CreatedDate = new DateTime(1900, 01, 01);
            UploadedDate = new DateTime(1900, 01, 01);
        }
    }
}