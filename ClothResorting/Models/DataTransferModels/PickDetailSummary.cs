using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.DataTransferModels
{
    public class PickDetailSummary
    {
        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string SizeBundle { get; set; }

        public int PickCtns { get; set; }

        public int PickPcs { get; set; }
    }
}