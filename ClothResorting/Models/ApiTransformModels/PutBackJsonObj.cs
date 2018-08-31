using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class PutBackJsonObj
    {
        public int PullSheetId { get; set; }

        public string Container { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string CartonRange { get; set; }

        public string Color { get; set; }

        public string Customer { get; set; }

        public string SizeBundle { get; set; }

        public string PcsBundle { get; set; }

        public int PcsPerCarton { get; set; }

        public int Cartons { get; set; }

        public int Quantity { get; set; }

        public string Location { get; set; }
    }
}