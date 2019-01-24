using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class FBAAddressBookDto
    {
        public int Id { get; set; }

        public string WarehouseCode { get; set; }

        public string Address { get; set; }

        public string Memo { get; set; }
    }
}