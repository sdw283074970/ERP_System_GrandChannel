using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class GeneralLocationSummary
    {
        public int Id { get; set; }

        public string Vendor { get; set; }

        public string UploadedFileName { get; set; }

        public string Operator { get; set; }

        public string CreatedDate { get; set; }

        public string InboundDate { get; set; }

        public int InboundPcs { get; set; }

        public ICollection<ReplenishmentLocationDetail> RelenishmentLocationDetails { get; set; }

        public PreReceiveOrder PreReceiveOrder { get; set; }
    }
}