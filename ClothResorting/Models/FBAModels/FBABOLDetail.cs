using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBABOLDetail
    {
        public string CustoerOrderNumber { get; set; }

        public string Contianer { get; set; }

        public int CartonQuantity { get; set; }

        public float Weight { get; set; }

        public int PalletQuantity { get; set; }

        public string Location { get; set; }
    }
}