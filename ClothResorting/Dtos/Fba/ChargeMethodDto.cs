using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class ChargeMethodDto
    {
        public int Id { get; set; }

        public string Period { get; set; }

        public int WeekNumber { get; set; }

        public int Fee { get; set; }

        public ChargeTemplate ChargeTemplate { get; set; }
    }
}