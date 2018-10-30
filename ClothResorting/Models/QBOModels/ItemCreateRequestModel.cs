using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.QBOModels
{
    public class ItemCreateRequestModel
    {
        public string Name { get; set; }

        public IncomeAccountRef IncomeAccountRef { get; set; }

        public string Type { get; set; }
    }
}