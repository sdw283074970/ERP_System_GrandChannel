using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class PermanentLocJsonObj : BasicFourAttrsJsonObj
    {
        public string Location { get; set; }

        public string UPCNumber { get; set; }

        public string Vender { get; set; }
    }
}