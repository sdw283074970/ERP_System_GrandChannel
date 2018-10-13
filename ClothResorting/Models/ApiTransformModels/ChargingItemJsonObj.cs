using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class ChargingItemJsonObj
    {
        public string Vendor { get; set; }

        public string DepartmentCode { get; set; }

        public string ChargingType { get; set; }

        public string Name { get; set; }

        public double Rate { get; set; }

        public string Description { get; set; }

        public string Unit { get; set; }

    }
}