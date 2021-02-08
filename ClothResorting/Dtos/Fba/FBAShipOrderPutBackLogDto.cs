﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class FBAShipOrderPutBackLogDto
    {
        public int Id { get; set; }

        public int CartonId { get; set; }

        public string Container { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public string PutBackQuantity { get; set; }

        public string QuantityBeforePutBack { get; set; }

        public string QuantityAfterPutBack { get; set; }

        public string NewLocation { get; set; }
    }
}