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

        public float TotalCost { get; set; }

        public float Net { get; set; }

        public string SubCustomer { get; set; }

        public string CustomerCode { get; set; }

        public string CreatedBy { get; set; }

        public string UnloadingType { get; set; }

        public string StorageType { get; set; }

        public float TotalCBM { get; set; }

        public string DockNumber { get; set; }

        public string Palletizing { get; set; }

        public string InboundType { get; set; }

        public string InvoiceStatus { get; set; }

        public int TotalCtns { get; set; }

        public float ActualCBM { get; set; }

        public int ActualCtns { get; set; }

        public int ActualPlts { get; set; }

        public float TotalAmount { get; set; }

        public DateTime InboundDate { get; set; }

        public DateTime CloseDate { get; set; }

        public string ConfirmedBy { get; set; }

        public string Status { get; set; }

        public int OriginalPlts { get; set; }

        public string ReceivedBy { get; set; }

        public int SKUNumber { get; set; }

        public string UpdateLog { get; set; }

        public string Comment { get; set; }

        public string Lumper { get; set; }

        public string Instruction { get; set; }

        public DateTime PushTime { get; set; }

        public DateTime UnloadFinishTime { get; set; }

        public DateTime AvailableTime { get; set; }

        public DateTime UnloadStartTime { get; set; }

        public DateTime OutTime { get; set; }

        public string IsDamaged { get; set; }

    }
}