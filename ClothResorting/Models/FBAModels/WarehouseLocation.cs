using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class WarehouseLocation
    {
        public int Id { get; set; }

        public string WarehouseName { get; set; }

        public string WarehouseCode { get; set; }

        public string Address { get; set; }

        public string ContactPerson { get; set; }

        public bool IsActive { get; set; }
    }
}