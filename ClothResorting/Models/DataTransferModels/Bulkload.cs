﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.DataTransferModels
{
    public class Bulkload
    {
        public string PurchaseOrder { get; set; }
        public string Style { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string NumberOfCartons { get; set; }
        public string PcsQuantity { get; set; }
        public string Location { get; set; }
    }
}