using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels.BaseClass
{
    public class BaseFBAOrderDetail
    {
        public int Id { get; set; }

        public string GrandNumber { get; set; }

        public string Container { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public float ActualCBM { get; set; }

        public float ActualGrossWeight { get; set; }

        public int ActualQuantity { get; set; }

        public string Comment { get; set; }

        public string HowToDeliver { get; set; }

        public string Status { get; set; }

        public BaseFBAOrderDetail()
        {
            Container = string.Empty;
            ShipmentId = string.Empty;
            AmzRefId = string.Empty;
            WarehouseCode = string.Empty;
            ActualCBM = 0f;
            ActualGrossWeight = 0f;
            ActualQuantity = 0;
            Status = string.Empty;
        }

        public void AssembleFirstStringPart(string shipmentId, string amzRefId, string warehouseCode)
        {
            ShipmentId = shipmentId ?? string.Empty;
            AmzRefId = amzRefId ?? string.Empty;
            WarehouseCode = warehouseCode ?? string.Empty;
        }

        public void AssembleActualDetails(float actualGrossWight, float actualCBM, int actualQuantity)
        {
            ActualCBM = actualCBM;
            ActualGrossWeight = actualGrossWight;
            ActualQuantity = actualQuantity;
        }

        public void AssembleUniqueIndex(string container, string grandNumber)
        {
            Container = container;
            GrandNumber = grandNumber;
        }
    }

    public class Instruction
    {
        public int Id { get; set; }

        public string Reference { get; set; }

        public string OrderType { get; set; }

        public string Description { get; set; }

        public bool IsInstruction { get; set; }

        public bool IsOperation { get; set; }

        public bool IsChargingItem { get; set; }

        public string Comment { get; set; }

        public string Result { get; set; }
    }
}