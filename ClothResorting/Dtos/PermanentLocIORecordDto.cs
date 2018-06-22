﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class PermanentLocIORecordDto
    {
        public int Id { get; set; }

        public string PermanentLoc { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public int TargetPcs { get; set; }

        public int InvBefore { get; set; }

        public int InvChange { get; set; }

        public int InvAfter { get; set; }

        public string FromLocation { get; set; }

        public DateTime OperationDate { get; set; }

        public int TargetBanlance { get; set; }
    }
}