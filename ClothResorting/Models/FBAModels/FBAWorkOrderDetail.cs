using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAWorkOrderDetail
    {
        public int Id { get; set; }

        public string  Description { get; set; }

        public FBAWorkOrderTemplate FBAWorkOrderTemplate { get; set; }
    }
}