using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class ArrPreIdLocationJsonObj
    {
        public int[] Arr { get; set; }

        public int PreId { get; set; }

        public string Location { get; set; }
    }
}