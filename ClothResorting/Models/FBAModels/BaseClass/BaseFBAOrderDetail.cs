﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels.BaseClass
{
    public class BaseFBAOrderDetail
    {
        public int Id { get; set; }

        public string Container { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public float ActualCBM { get; set; }

        public float ActualGrossWeight { get; set; }

        public float ActualQuantity { get; set; }

        public string Comment { get; set; }

        public BaseFBAOrderDetail()
        {
            Container = string.Empty;
            ShipmentId = string.Empty;
            AmzRefId = string.Empty;
            WarehouseCode = string.Empty;
            ActualCBM = 0f;
            ActualGrossWeight = 0f;
            ActualQuantity = 0;
        }

        public void AssembleFirstStringPart(string shipmentId, string amzRefId, string warehouseCode)
        {
            ShipmentId = shipmentId ?? string.Empty;
            AmzRefId = amzRefId ?? string.Empty;
            WarehouseCode = warehouseCode ?? string.Empty;
        }
    }
}