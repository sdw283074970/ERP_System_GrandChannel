using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class GeneralLocationSummaryDto
    {
        public int Id { get; set; }

        public string Vendor { get; set; }

        public string UploadedFileName { get; set; }

        public string Operator { get; set; }

        public string CreatedDate { get; set; }

        public string InboundDate { get; set; }

        public int InboundPcs { get; set; }
    }
}