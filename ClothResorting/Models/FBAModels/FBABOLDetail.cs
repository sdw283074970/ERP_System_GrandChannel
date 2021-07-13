using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBABOLDetail
    {
        public int ParentPalletId { get; set; }

        public string CustomerOrderNumber { get; set; }

        public string Contianer { get; set; }

        public int CartonQuantity { get; set; }

        public string AmzRef { get; set; }

        public float Weight { get; set; }

        public int ActualPallets { get; set; }

        public int PickPallets { get; set; }

        public string Location { get; set; }

        public string Memo { get; set; }

        //用于标记是否是宿主物品
        public bool IsMainItem { get; set; }
    }
}