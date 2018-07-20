using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class PickingListDto
    {
        public int Id { get; set; }

        public string OrderPurchaseOrder { get; set; }

        public DateTime? CreateDate { get; set; }

        public string Status { get; set; }

        public string PickTicketsRange { get; set; }
    }
}