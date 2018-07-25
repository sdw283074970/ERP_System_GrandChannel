using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.DataTransferModels
{
    public class PullSheetSummary
    {
        public string Style { get; set; }

        public string Color { get; set; }

        public string SizeBundle { get; set; }

        public string PcsBundle { get; set; }

        public int Cartons { get; set; }

        public int Quantity { get; set; }
    }
}