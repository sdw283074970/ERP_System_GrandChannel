using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class ChargingItemDto
    {
        public int Id { get; set; }

        public string ChargingType { get; set; }

        public string Name { get; set; }

        public int Rate { get; set; }

        public string Description { get; set; }
    }
}