using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class ChargeMethod
    {
        public int Id { get; set; }

        public string Period { get; set; }

        public int WeekNumber { get; set; }

        public double Fee { get; set; }

        public ChargeTemplate ChargeTemplate { get; set; }
    }
}