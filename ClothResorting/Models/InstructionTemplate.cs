using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class InstructionTemplate
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreateBy { get; set; }

        public bool IsApplyToShipOrder { get; set; }

        public bool IsApplyToMasterOrder { get; set; }

        public UpperVendor Customer { get; set; }

        public InstructionTemplate()
        {
            CreateDate = new DateTime(1900, 1, 1, 0, 0, 0, 0);
        }
    }
}