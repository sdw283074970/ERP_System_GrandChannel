using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class FBAWorkOrderTemplateDto
    {
        public int Id { get; set; }

        public string TemplateName { get; set; }

        public string CustomerCode { get; set; }

        public string WorkOrderType { get; set; }
    }
}