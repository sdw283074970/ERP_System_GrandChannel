using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class CartonBreakdownRequestJsonObj : CartonDetailRequestJsonObj
    {
        public string Location { get; set; }
    }
}