using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class WarehouseLocationDto
    {
        public int Id { get; set; }

        public string WarehouseName { get; set; }

        public string WarehouseCode { get; set; }

        public string Address { get; set; }

        public string ContactPerson { get; set; }
    }
}