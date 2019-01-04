using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.StaticClass
{
    public static class Status
    {
        public const string NewCreated = "New Created";

        public const string InStock = "In Stock";

        public const string Picking = "Picking";

        public const string Ready = "Ready";

        public const string Shipped = "Shipped";

        public const string ToBeAllocated = "To Be Allocated";

        public const string Allocating = "Allocating";

        public const string Reallocated = "Reallocated";

        public const string Unassigned = "Unassigned";

        public const string NotAvailable = "N/A";

        public const string Unknown = "Unknown";

        public const string Missing = "Missing";

        public const string Shortage = "Shortage";

        public const string Overage = "Overage";

        public const string ConcealedOverage = "Concealed Overage";

        public const string Allocated = "Allocated";

        public const string Delete = "Deleted";

        public const string Reallocating = "Reallocating";

        public const string Active = "Active";

        public const string Deactive = "Deactive";
    }
}