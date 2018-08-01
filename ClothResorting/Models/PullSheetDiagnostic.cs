using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class PullSheetDiagnostic
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public string DiagnosticDate { get; set; }

        public string Description { get; set; }

        public PullSheet PullSheet { get; set; }
    }
}