﻿using ClothResorting.Models.FBAModels;
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

        public string InvoiceType { get; set; }

        public string ChargingType { get; set; }

        public string Unit { get; set; }

        public double Quantity { get; set; }

        public double Rate { get; set; }

        public double OriginalAmount { get; set; }

        public double Amount { get; set; }  // final amount

        public string Operator { get; set; }

        public double Cost { get; set; }

        public bool CostConfirm { get; set; }

        public bool BonusStatus { get; set; }

        public bool CollectionStatus { get; set; }

        public bool PaymentStatus { get; set; }

        public float Discount { get; set; }

        public DateTime DateOfCost { get; set; }

        public string Memo { get; set; }

        public FBAMasterOrder FBAMasterOrder { get; set; }

        public FBAShipOrder FBAShipOrder { get; set; }

        public Invoice Invoice { get; set; }

        public InvoiceDetail()
        {
            DateOfCost = new DateTime(1900, 01, 01);
        }
    }
}