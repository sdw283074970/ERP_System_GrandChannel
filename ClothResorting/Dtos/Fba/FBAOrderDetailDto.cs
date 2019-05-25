using ClothResorting.Models.FBAModels.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class FBAOrderDetailDto : BaseFBAOrderDetail
    {
        public string LotSize { get; set; }

        public float GrossWeight { get; set; }

        public float CBM { get; set; }

        public int Quantity { get; set; }

        public string Remark { get; set; }

        public int ComsumedQuantity { get; set; }

    }
}