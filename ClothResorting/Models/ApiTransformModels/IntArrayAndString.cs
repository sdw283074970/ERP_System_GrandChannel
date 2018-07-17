using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class IntArrayIntString
    {
        public int[] arr { get; set; }
        public int preId { get; set; }
        public string container { get; set; }
    }
}