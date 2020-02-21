using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class InstructionTemplateDto
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsApplyToShipOrder { get; set; }

        public bool IsApplyToMasterOrder { get; set; }

        public string Status { get; set; }

        public string CreateBy { get; set; }

        public bool IsInstruction { get; set; }

        public bool IsOperation { get; set; }

        public bool IsCharging { get; set; }

        public bool IsApplyToAll { get; set; }
    }
}