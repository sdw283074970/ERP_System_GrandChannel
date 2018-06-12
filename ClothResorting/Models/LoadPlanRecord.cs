using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class LoadPlanRecord
    {
        public int Id { get; set; }

        public string PurchaseOrder { get; set; }

        public DateTime OutBoundDate { get; set; }

        public int OutBoundCtns { get; set; }

        public int OutBoundPcs { get; set; }

        public ICollection<RetrievingRecord> RetrievingRecords { get; set; }
    }
}