using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class LocationDetailDto
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public int NumberOfCartons { get; set; }

        public int Pcs { get; set; }

        public string Location { get; set; }

        public DateTime InboundDate { get; set; }
    }
}