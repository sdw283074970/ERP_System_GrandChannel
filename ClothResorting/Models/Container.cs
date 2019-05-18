using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class Container
    {
        public int Id { get; set; }

        public string Vendor { get; set; }

        public string ContainerNumber { get; set; }

        public string Reference { get; set; }

        public string ReceiptNumber { get; set; }

        public string ReceivedDate { get; set; }

        public DateTime InboundDate { get; set; }

        public string Remark { get; set; }

        public ICollection<POSummary> POSummaries { get; set; }

        public Container()
        {
            InboundDate = new DateTime(1900, 1, 1, 0, 0, 0, 0);
        }
    }
}