using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.StaticClass
{
    public static class OperationType
    {
        public const string Create = "Create";

        public const string Update = "Update";

        public const string Retrieve = "Retrieve";

        public const string Delete = "Delete";

        public const string Login = "Login";

        public const string Logout = "Logout";
    }

    public static class OperationLevel
    {
        public const string Normal = "Normal";

        public const string Mediunm = "Medium";

        public const string High = "High";

        public const string Danger = "Danger";
    }
}