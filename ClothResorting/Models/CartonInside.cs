using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class CartonInside
    {
        public int Id { get; set; }

        public string Size { get; set; }

        public int Quantity { get; set; }

        public FCRegularLocationDetail FCRegularLocationDetail { get; set; }
    }
}