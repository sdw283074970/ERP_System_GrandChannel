using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels.StaticModels
{
    public static class FBAStatus
    {
        public const string NewCreated = "New Created";

        public const string InStock = "In Stock";

        public const string Picking = "Picking";

        public const string Shipped = "Shipped";

        public const string InPallet = "InPallet";

        public const string Unassigned = "Unassigned";

        public const string Relocated = "Relocated";
    }
}