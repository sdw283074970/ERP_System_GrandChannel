using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAShipOrderPickLog
    {
        public int Id { get; set; }

        public int CartonId { get; set; }

        public string Container { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public string PickQuantity { get; set; }

        public string QuantityBeforPick { get; set; }

        public string QuantityAfterPick { get; set; }

        public string FromLocation { get; set; }

        public FBAShipOrder FBAShipOrder { get; set; }
    }
}