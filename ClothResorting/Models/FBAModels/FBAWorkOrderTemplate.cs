using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAWorkOrderTemplate
    {
        public int Id { get; set; }

        public string TemplateName { get; set; }

        public string CustomerCode { get; set; }

        public string WorkOrderType { get; set; }

        public ICollection<FBAWorkOrderDetail> FBAWorkOrderDetails { get; set; }
    }
}