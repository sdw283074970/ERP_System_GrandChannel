using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class CartonDetailRequestJsonObj : BasicFourAttrsJsonObj
    {
        public int GrandTotal { get; set; }
    }
}