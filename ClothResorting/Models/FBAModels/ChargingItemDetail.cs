using ClothResorting.Models.FBAModels.StaticModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class ChargingItemDetail
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public bool IsOperation { get; set; }

        public string OriginalDescription { get; set; }

        public string Status { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreateBy { get; set; }

        public string Comment { get; set; }

        public string Result { get; set; }

        public string HandlingStatus { get; set; }

        public string ConfirmedBy { get; set; }

        public bool IsInstruction { get; set; }

        public bool IsCharging { get; set; }

        public FBAMasterOrder FBAMasterOrder { get; set; }

        public FBAShipOrder FBAShipOrder { get; set; }

        public bool VisibleToAgent { get; set; }

        //public UpperVendor Customer { get; set; }

        public ChargingItemDetail()
        {
            HandlingStatus = FBAStatus.New;

            Status = FBAStatus.NoNeedForCharging;

            CreateDate = new DateTime(1900, 1, 1, 0, 0, 0, 0);

            IsOperation = false;

            IsInstruction = false;

            IsCharging = false;

            VisibleToAgent = false;
        }
    }
}