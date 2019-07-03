using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class NameCrossReference
    {
        public int Id { get; set; }

        public string StringType { get; set; }

        public string OriginalString { get; set; }

        public string Synonym { get; set; }
    }
}