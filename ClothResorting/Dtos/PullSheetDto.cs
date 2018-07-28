using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class PullSheetDto
    {
        public int Id { get; set; }

        public string PickTicketsRange { get; set; }

        public string Status { get; set; }

        public string CreateDate { get; set; }
    }
}