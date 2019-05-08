using ClothResorting.Models.FBAModels.StaticModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAShipOrder
    {
        public int Id { get; set; }

        public string ShipOrderNumber { get; set; }

        public string CustomerCode { get; set; }

        public string OrderType { get; set; }

        public string Destination { get; set; }

        public string PickReference { get; set; }

        public string InvoiceStatus { get; set; }

        public DateTime CreateDate { get; set; }

        public string EditBy { get; set; }

        public string CreateBy { get; set; }

        public string PlacedBy { get; set; }

        public DateTime PlaceTime { get; set; }

        public DateTime ShipDate { get; set; }

        public string PickMan { get; set; }

        public string Status { get; set; }

        public DateTime ReadyTime { get; set; }

        public string ReadyBy { get; set; }

        public string ReleasedBy { get; set; }

        public string ShippedBy { get; set; }

        public string BOLNumber { get; set; }

        public float TotalAmount { get; set; }

        public string ConfirmedBy { get; set; }

        public string Carrier { get; set; }

        public string Instruction { get; set; }

        public DateTime ETS { get; set; }

        public string OperationLog { get; set; }

        public string ETSTimeRange { get; set; }

        public DateTime CloseDate { get; set; }

        public string StartedBy { get; set; }

        public DateTime StartedTime { get; set; }

        public DateTime ReleasedDate { get; set; }

        public string PurchaseOrderNumber { get; set; }

        public string PickNumber { get; set; }

        public int TotalCtns { get; set; }

        public int TotalPlts { get; set; }

        public int TotalPltsFromInventory { get; set; }

        public int TotalNewPlts { get; set; }

        public string Instructor { get; set; }

        public string Lot { get; set; }

        public string Comment { get; set; }

        public ICollection<ChargingItemDetail> ChargingItemDetails { get; set; }

        public ICollection<FBAPickDetail> FBAPickDetails { get; set; }

        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }

        public ICollection<EFile> Efiles { get; set; }

        public ICollection<PullSheetDiagnostic> PullDiagnostics { get; set; }

        public FBAShipOrder()
        {
            CreateDate = DateTime.Now;
            Status = FBAStatus.NewCreated;
            PickMan = FBAStatus.Unassigned;
            ShippedBy = FBAStatus.Unassigned;
            CreateDate = new DateTime(1900, 1, 1, 0, 0, 0, 0);
            PlaceTime = new DateTime(1900, 1, 1, 0, 0, 0, 0);
            CloseDate = new DateTime(1900, 1, 1, 0, 0, 0, 0);
            ShipDate = new DateTime(1900, 1, 1, 0, 0, 0, 0);
            ReadyTime = new DateTime(1900, 1, 1, 0, 0, 0, 0);
            ReleasedDate = new DateTime(1900, 1, 1, 0, 0, 0, 0);
            ETS = new DateTime(1900, 1, 1, 0, 0, 0, 0);
            StartedTime = new DateTime(1900, 1, 1, 0, 0, 0, 0);
            ConfirmedBy = "";
            ShippedBy = "";
            InvoiceStatus = "Await";
        }

        public void AssembleBaseInfo(string shipOrderNumber, string customerCode, string orderType, string destination, string pickReference)
        {
            ShipOrderNumber = shipOrderNumber;
            CustomerCode = customerCode;
            OrderType = orderType;
            Destination = destination;
            PickReference = pickReference;
        }
    }
}