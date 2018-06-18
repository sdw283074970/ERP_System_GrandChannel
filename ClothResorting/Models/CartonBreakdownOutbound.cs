using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class CartonBreakdownOutbound
    {
        public int Id { get; set; }

        public string PickPurchaseNumber { get; set; }

        public DateTime TimeOfOutbound { get; set; }

        public int Pcs { get; set; }

        public CartonBreakDown CartonBreakdown { get; set; }
    }
}