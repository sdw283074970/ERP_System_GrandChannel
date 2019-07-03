using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class NameCrossReferenceDto
    {
        public int Id { get; set; }

        public string NameType { get; set; }

        public string NameInSystem { get; set; }

        public string Synonym { get; set; }
    }
}