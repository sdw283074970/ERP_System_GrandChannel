﻿using ClothResorting.Models.FBAModels.BaseClass;
using ClothResorting.Models.FBAModels.Interfaces;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAMasterOrder : BaseFBAMasterOrder
    {
        public int Id { get; set; }

        public string GrandNumber { get; set; }

        public float TotalCBM { get; set; }

        public int TotalCtns { get; set; }

        public float ActualCBM { get; set; }

        public int ActualCtns { get; set; }

        public int ActualPlts { get; set; }

        public string InboundType { get; set; }

        public string InvoiceStatus { get; set; }

        public DateTime InboundDate { get; set; }

        public string Status { get; set; }

        public UpperVendor Customer { get; set; }

        public float TotalAmount { get; set; }

        public ICollection<FBAOrderDetail> FBAOrderDetails { get; set; }

        public ICollection<FBAPalletLocation> FBAPalletLocations { get; set; } 

        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }

        //public ICollection<FBAPalletLocation> FBAPalletLocations { get; set; }

        public FBAMasterOrder()
        {
            GrandNumber = string.Empty;
            TotalCBM = 0f;
            ActualPlts = 0;
            ActualCBM = 0f;
            ActualCtns = 0;
            TotalCtns = 0;
            InboundDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            Status = StaticClass.Status.NewCreated;
        }
    }
}