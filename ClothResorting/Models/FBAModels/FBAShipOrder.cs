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

        public string CreateBy { get; set; }

        public DateTime PickDate { get; set; }

        public DateTime ShipDate { get; set; }

        public string PickMan { get; set; }

        public string Status { get; set; }

        public string ShippedBy { get; set; }

        public string BOLNumber { get; set; }

        public float TotalAmount { get; set; }

        public string Carrier { get; set; }

        public DateTime ETS { get; set; }

        public ICollection<ChargingItemDetail> ChargingItemDetails { get; set; }

        public ICollection<FBAPickDetail> FBAPickDetails { get; set; }

        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }

        public FBAShipOrder()
        {
            CreateDate = DateTime.Now;
            Status = FBAStatus.NewCreated;
            PickDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            PickMan = FBAStatus.Unassigned;
            ShippedBy = FBAStatus.Unassigned;
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