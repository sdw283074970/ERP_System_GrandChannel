using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class RegualrCartonBulkJsonObj
    {
        public int Id { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public string Pcs { get; set; }

        public int Pack { get; set; }
    }
}