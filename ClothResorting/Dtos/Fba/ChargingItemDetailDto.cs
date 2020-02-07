using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class ChargingItemDetailDto
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public bool IsOperation { get; set; }

        public string OriginalDescription { get; set; }

        public string Status { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreateBy { get; set; }

        public string Comment { get; set; }

        public string Result { get; set; }

        public string HandlingStatus { get; set; }
    }
}