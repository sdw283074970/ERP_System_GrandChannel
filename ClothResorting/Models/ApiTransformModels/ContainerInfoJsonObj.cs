using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.ApiTransformModels
{
    public class ContainerInfoJsonObj
    {
        public int PreId { get; set; }

        public string Vendor { get; set; }

        public string ReceivedDate { get; set; }

        public string ContainerNumber { get; set; }

        public string Reference { get; set; }

        public string ReceiptNumber { get; set; }

        public string Remark { get; set; }
    }
}