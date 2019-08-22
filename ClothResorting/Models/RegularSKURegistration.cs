using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class RegularSKURegistration
    {
        public int Id { get; set; }

        public string  Customer { get; set; }

        public string UPCNumber { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public int SizeNumber { get; set; }
    }
}