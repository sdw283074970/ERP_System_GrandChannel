using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class ChargeTemplateDto
    {
        public int Id { get; set; }

        public string TemplateName { get; set; }

        public string Customer { get; set; }

        public ICollection<ChargeMethod> ChargeMethods { get; set; }
    }
}