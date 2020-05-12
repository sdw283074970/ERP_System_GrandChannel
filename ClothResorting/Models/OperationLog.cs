using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class OperationLog
    {
        public int Id { get; set; }

        public string User { get; set; }

        public string OperationType { get; set; }

        public string Description { get; set; }

        public string RequestUri { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public string Exception { get; set; }

        public DateTime OperationDate { get; set; }

        public string Level { get; set; }

        public string EntityName { get; set; }

        public string UserIp { get; set; }

        public string RequestBody {get;set;}

        public string RequestId { get; set; }
    }
}