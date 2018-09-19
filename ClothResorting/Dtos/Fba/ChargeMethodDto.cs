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

        public int From { get; set; }

        public int To { get; set; }

        public int Duration { get; set; }

        public double Fee { get; set; }

        public string TimeUnit { get; set; }

        public string Currency { get; set; }
    }
}