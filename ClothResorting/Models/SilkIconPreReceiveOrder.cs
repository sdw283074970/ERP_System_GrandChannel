using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class SilkIconPreReceiveOrder : PreReceiveOrder
    {
        public ICollection<SilkIconPackingList> SilkIconPackingLists { get; set; }
    }
}