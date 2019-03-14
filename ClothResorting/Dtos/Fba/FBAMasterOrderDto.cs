using ClothResorting.Models.FBAModels.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class FBAMasterOrderDto : BaseFBAMasterOrder
    {
        public int Id { get; set; }

        public string GrandNumber { get; set; }

        public float TotalCBM { get; set; }

        public string InboundType { get; set; }

        public string InvoiceStatus { get; set; }

        public int TotalCtns { get; set; }

        public float ActualCBM { get; set; }

        public int ActualCtns { get; set; }

        public int ActualPlts { get; set; }

        public float TotalAmount { get; set; }

        public DateTime InboundDate { get; set; }

        public string Status { get; set; }
    }
}