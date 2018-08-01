using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class PullSheetDiagnosticDto
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public string DiagnosticDate { get; set; }

        public string Description { get; set; }
    }
}