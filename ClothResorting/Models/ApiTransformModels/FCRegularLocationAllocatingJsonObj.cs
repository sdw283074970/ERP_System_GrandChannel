using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class FCRegularLocationAllocatingJsonObj
    {
        public int PreId { get; set; }
        public int Id { get; set; }
        public int Cartons { get; set; }
        public string Location { get; set; }
    }
}