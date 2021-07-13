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

        public string Barcode { get; set; }

        public string Symbology { get; set; }

        public float CBM { get; set; }

        public int Quantity { get; set; }

        public string Remark { get; set; }

        public string TempLocation { get; set; }

        public int ComsumedQuantity { get; set; }

        public int LabelFileNumbers { get; set; }

        public int CtnsPerLocation { get; set; }

        public int SelectedQuantity { get; set; }

        public FBAOrderDetailDto()
        {
            SelectedQuantity = 0;
        }
    }
}