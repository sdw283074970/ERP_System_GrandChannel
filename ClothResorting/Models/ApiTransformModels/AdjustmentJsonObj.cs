using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class AdjustmentJsonObj : BasicFourAttrsJsonObj
    {
        public int Adjust { get; set; }
    }
}