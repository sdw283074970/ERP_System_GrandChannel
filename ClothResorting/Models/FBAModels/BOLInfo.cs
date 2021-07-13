using ClothResorting.Dtos.Fba;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class BOLInfo
    {
        public BOLDetail BOLDetail { get; set; }

        public IEnumerable<FBAOrderDetailDto> OrderDetails { get; set; }
    }

    public class BOLDetail
    {
        public string FreightCharge { get; set; }

        public string Operator { get; set; }

        public string BOLNumber { get; set; }

        public string Address { get; set; }

        public string WarehouseCode { get; set; }

        public string Carrier { get; set; }

        public int PltQuantity { get; set; }
    }
}