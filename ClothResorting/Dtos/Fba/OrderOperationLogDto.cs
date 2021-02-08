using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class OrderOperationLogDto
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public DateTime OperationDate { get; set; }

        public string Operator { get; set; }
    }
}