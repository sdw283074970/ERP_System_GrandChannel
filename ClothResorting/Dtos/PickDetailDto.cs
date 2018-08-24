using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class PickDetailDto
    {
        public int Id { get; set; }

        public string Container { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string CustomerCode { get; set; }

        public string SizeBundle { get; set; }

        public string PcsBundle { get; set; }

        public int PcsPerCarton { get; set; }

        public string Status { get; set; }

        public string Location { get; set; }

        public int PickCtns { get; set; }

        public int PickPcs { get; set; }

        public int LocationDetailId { get; set; }

        public string PickDate { get; set; }

        public string CartonRange { get; set; }

        public string Memo { get; set; }
    }
}