using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class PurchaseOrderInventoryDto
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string OrderType { get; set; }

        public string Vender { get; set; }

        public int InvPcs { get; set; }

        public int InvCtns { get; set; }
    }
}