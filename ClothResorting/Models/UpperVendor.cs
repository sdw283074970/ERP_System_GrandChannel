using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class UpperVendor
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Department { get; set; }

        public ICollection<PreReceiveOrder> WorkOrders { get; set; }
    }
}