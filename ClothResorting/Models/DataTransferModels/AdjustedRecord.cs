using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.DataTransferModels
{
    public class AdjustedRecord
    {
        public DateTime AdjustedDate { get; set; }

        public string AdjustedPcs { get; set; }

        public string Balance { get; set; }
    }
}